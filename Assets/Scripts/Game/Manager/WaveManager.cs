using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [SerializeField, Tooltip("Spawner 활성화 시 효과음")]
    private AudioClip spanwerActivateClip;

    [SerializeField]
    private EnemySpawner enemySpanwer_East;
    [SerializeField]
    private EnemySpawner enemySpanwer_West;
    [SerializeField]
    private EnemySpawner enemySpanwer_South;
    [SerializeField]
    private EnemySpawner enemySpanwer_North;

    [Header("일반 웨이브 설정")]
    [SerializeField, Tooltip("wave 순서대로 적용될 WaveData (리스트의 index = wave 번호)")]
    private List<WaveData> waveDataList;

    [SerializeField, Tooltip("카메라 이동이 완료되기까지 걸리는 시간(초)")]
    private float cameraMoveDuration = 1.5f;

    [SerializeField, Tooltip("카메라 이동이 끝난 후 Spawner가 활성화되기까지 걸리는 시간(초)")]
    private float activateSpawnerDelayAfterCameraMove = 0.5f;

    [Header("무한 웨이브 설정")]
    [SerializeField, Tooltip("무한 웨이브 모드 진입 후 다음 wave로 넘어가기까지 걸리는 시간(초)")]
    private float infiniteWaveAutoAdvanceInterval = 10f;

    [SerializeField, Tooltip("무한 웨이브 모드에서 소환할 Enemy 종류")]
    private List<EnemyData> infiniteEnemyDatas = new();

    [SerializeField, Tooltip("무한 웨이브 시작 시 적의 최대 체력")]
    private float infiniteBaseMaxHealth = 30f;

    [SerializeField, Tooltip("무한 웨이브에서 적이 한 마리 스폰될 때마다 늘어나는 최대 체력")]
    private float infiniteHealthIncreasePerWave = 1f;

    [SerializeField, Tooltip("무한 웨이브 시작 시 적의 지급 점수")]
    private int infiniteBaseScoreReward = 10;

    [SerializeField, Tooltip("무한 웨이브에서 적이 한 마리 스폰될 때마다 늘어나는 지급 점수")]
    private int infiniteScoreRewardIncreasePerWave = 1;

    [SerializeField, Tooltip("무한 웨이브 시작 wave(=infiniteWaveStartIndex)에서 스폰 간격(초)")]
    private float infiniteBaseSpawnInterval = 10f;

    [SerializeField, Tooltip("무한 웨이브에서 wave가 1 증가할 때마다 줄어드는 스폰 간격(초)")]
    private float infiniteSpawnIntervalDecreasePerWave = 0.1f;

    [SerializeField, Tooltip("무한 웨이브 스폰 간격이 줄어들 수 있는 최소값(초)")]
    private float infiniteMinSpawnInterval = 0.5f;

    [SerializeField, Tooltip("무한 웨이브에서 몇 마리를 소환할 때마다 enemy 종류를 새로 랜덤하게 바꿀지")]
    private int infiniteEnemyChangeInterval = 10;

    private Coroutine _infiniteTimerCoroutine;
    private Coroutine _infiniteSpawnCoroutine;
    private float _currentInfiniteSpawnInterval;
    private float _currentInfiniteMaxHealth;
    private int _currentInfiniteScoreReward;

    private int currentWaveIndex;
    private int _pendingSpawnerCount;

    private AudioSource _audioSource;

    public event Action TutorialWaveCleared;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        _audioSource = GetComponent<AudioSource>();
    }

    public void OnClickStartWave()
    {
        if (currentWaveIndex >= waveDataList.Count)
        {
            StartInfiniteAutoProgress(
                enemySpanwer_East,
                enemySpanwer_West,
                enemySpanwer_South,
                enemySpanwer_North);
        }
        else
        {
            StartSpawnerActivate();
        }
    }

    public void StartSpawnerActivate()
    {
        var waveData = GetCurrentWaveData();
        if (waveData == null || waveData.spawnInfos == null || waveData.spawnInfos.Count == 0)
        {
            Debug.LogWarning($"[WaveManager] wave {currentWaveIndex} 에 대한 WaveData가 없습니다.");
            return;
        }

        var cameraTargetSpawner = GetSpawner(waveData.spawnInfos[0].spawnPoint);
        CameraManager.Instance.MoveTowerPlacementCameraTo(cameraTargetSpawner.spawnPoint.position, cameraMoveDuration, OnCameraMoveCompleted);
    }

    void OnCameraMoveCompleted()
    {
        StartCoroutine(ActivateSpawnerAfterCameraMove());
    }

    IEnumerator ActivateSpawnerAfterCameraMove()
    {
        yield return new WaitForSeconds(activateSpawnerDelayAfterCameraMove);

        ActivateSpawner();
        CameraManager.Instance.UnlockTowerPlacementCamera();
    }

    void ActivateSpawner()
    {
        var waveData = GetCurrentWaveData();
        if (waveData == null || waveData.spawnInfos == null || waveData.spawnInfos.Count == 0)
        {
            Debug.LogWarning($"[WaveManager] wave {currentWaveIndex} 에 대한 WaveData가 없습니다.");
            return;
        }

        if (spanwerActivateClip != null)
            _audioSource.PlayOneShot(spanwerActivateClip);

        _pendingSpawnerCount = 0;

        foreach (var spawnInfo in waveData.spawnInfos)
        {
            var spawner = GetSpawner(spawnInfo.spawnPoint);
            if (spawner == null)
                continue;

            spawner.WaveCleared += OnSpawnerWaveCleared;
            spawner.ActivateSpawner(spawnInfo);
            _pendingSpawnerCount++;
        }

        if (_pendingSpawnerCount == 0)
        {
            Debug.LogWarning($"[WaveManager] wave {currentWaveIndex} 에서 유효한 스포너를 찾지 못했습니다.");
        }
    }

    /// <summary>스포너가 이번 wave에 스폰한 적을 모두 스폰하고, 그 적들이 전부 죽었을 때 호출됨</summary>
    void OnSpawnerWaveCleared(EnemySpawner spawner)
    {
        spawner.WaveCleared -= OnSpawnerWaveCleared;

        _pendingSpawnerCount--;
        if (_pendingSpawnerCount <= 0)
        {
            ClearTutorialWave();
        }
    }

    // 마지막 적 죽으면 실행
    void ClearTutorialWave()
    {
        currentWaveIndex++;
        TutorialWaveCleared?.Invoke();
    }

    /// <summary>현재 wave 번호에 해당하는 WaveData를 반환</summary>
    private WaveData GetCurrentWaveData()
    {
        if (waveDataList == null || currentWaveIndex < 0 || currentWaveIndex >= waveDataList.Count)
            return null;

        return waveDataList[currentWaveIndex];
    }

    /// <summary>지정한 스폰 방향에 매칭된 씬의 EnemySpawner를 반환한다.</summary>
    private EnemySpawner GetSpawner(SpawnPoint direction)
    {
        switch (direction)
        {
            case SpawnPoint.East:
                return enemySpanwer_East;
            case SpawnPoint.West:
                return enemySpanwer_West;
            case SpawnPoint.South:
                return enemySpanwer_South;
            case SpawnPoint.North:
                return enemySpanwer_North;
        }
        return null;
    }


    /// <summary>
    /// 무한 웨이브 모드에 진입. 자동으로 난이도(체력/스폰 간격)를 갱신.
    /// </summary>
    void StartInfiniteAutoProgress(params EnemySpawner[] spawners)
    {
        StopAllInfiniteCoroutines();

        _currentInfiniteMaxHealth = infiniteBaseMaxHealth;
        _currentInfiniteScoreReward = infiniteBaseScoreReward;
        ApplyInfiniteWaveDifficulty(currentWaveIndex);

        _infiniteSpawnCoroutine = StartCoroutine(InfiniteSpawnRoutine(spawners));
        _infiniteTimerCoroutine = StartCoroutine(InfiniteWaveAutoProgressRoutine());
    }

    IEnumerator InfiniteWaveAutoProgressRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(infiniteWaveAutoAdvanceInterval);

            currentWaveIndex++;
            ApplyInfiniteWaveDifficulty(currentWaveIndex);
        }
    }

    /// <summary>
    /// 무한 스폰 루틴
    /// </summary>
    IEnumerator InfiniteSpawnRoutine(EnemySpawner[] spawners)
    {
        var currentEnemyDatas = new EnemyData[spawners.Length];
        var spawnCountSinceChange = new int[spawners.Length];
        for (int i = 0; i < spawners.Length; i++)
        {
            currentEnemyDatas[i] = GetRandomInfiniteEnemyData();
        }

        while (true)
        {
            for (int i = 0; i < spawners.Length; i++)
            {
                spawners[i].SpawnOnce(currentEnemyDatas[i], _currentInfiniteMaxHealth, _currentInfiniteScoreReward);

                spawnCountSinceChange[i]++;
                if (infiniteEnemyChangeInterval > 0 && spawnCountSinceChange[i] >= infiniteEnemyChangeInterval)
                {
                    spawnCountSinceChange[i] = 0;
                    currentEnemyDatas[i] = GetRandomInfiniteEnemyData();
                }
            }
            _currentInfiniteMaxHealth += infiniteHealthIncreasePerWave;
            _currentInfiniteScoreReward += infiniteScoreRewardIncreasePerWave;

            yield return new WaitForSeconds(_currentInfiniteSpawnInterval);
        }
    }

    /// <summary>waveDataList.Count로부터 몇 wave 지났는지를 기준으로 스폰 간격을 계산해 적용</summary>
    private void ApplyInfiniteWaveDifficulty(int waveIndex)
    {
        int stepsIntoInfinite = Mathf.Max(0, waveIndex - waveDataList.Count);

        _currentInfiniteSpawnInterval = Mathf.Max(
            infiniteMinSpawnInterval,
            infiniteBaseSpawnInterval - infiniteSpawnIntervalDecreasePerWave * stepsIntoInfinite);
    }

    /// <summary>무한 웨이브 모드에서 소환할 EnemyData를 랜덤으로 반환</summary>
    private EnemyData GetRandomInfiniteEnemyData()
    {
        if (infiniteEnemyDatas == null || infiniteEnemyDatas.Count == 0)
            return null;

        int index = UnityEngine.Random.Range(0, infiniteEnemyDatas.Count);
        return infiniteEnemyDatas[index];
    }

    /// <summary>무한 웨이브 관련 코루틴을 전부 정지한다.</summary>
    private void StopAllInfiniteCoroutines()
    {
        if (_infiniteSpawnCoroutine != null)
        {
            StopCoroutine(_infiniteSpawnCoroutine);
            _infiniteSpawnCoroutine = null;
        }

        if (_infiniteTimerCoroutine != null)
        {
            StopCoroutine(_infiniteTimerCoroutine);
            _infiniteTimerCoroutine = null;
        }
    }

    private void OnDestroy()
    {
        StopAllInfiniteCoroutines();
    }


}
