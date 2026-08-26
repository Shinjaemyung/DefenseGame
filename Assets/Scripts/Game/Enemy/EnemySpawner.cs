using Cinemachine;
using Core.Health;
using Core.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField, Tooltip("스폰 위치")]
    public Transform spawnPoint;

    [SerializeField, Tooltip("순서대로 이동할 웨이포인트 배열")]
    private Transform[] waypoints;

    private float _timer;
    private int _remainingSpawnCount;
    private EnemyData _currentEnemyData;

    private bool _isSpawning;

    private float _currentSpawnInterval;
    private float? _currentMaxHealthOverride;
    private int? _currentScoreRewardOverride;

    CinemachineImpulseSource _impulseSource;

    /// <summary>이 스포너가 이번 wave에서 스폰한 적이 전부 죽었을 때 발생</summary>
    public event Action<EnemySpawner> WaveCleared;

    private int _aliveCount;

    private void Update()
    {
        if (!_isSpawning)
            return;

        _timer += Time.deltaTime;
        if (_timer >= _currentSpawnInterval)
        {
            _timer = 0f;
            SpawnEnemy(_currentEnemyData, _currentMaxHealthOverride, _currentScoreRewardOverride);

            _remainingSpawnCount--;
            if (_remainingSpawnCount <= 0)
            {
                _isSpawning = false;
            }
        }
    }

    public void ActivateSpawner(SpawnInfo spawnInfo)
    {
        _impulseSource = GetComponent<CinemachineImpulseSource>();
        _impulseSource.GenerateImpulseWithForce(0.5f);
        gameObject.SetActive(true);

        StartSpawn(spawnInfo);
    }

    /// <summary>
    /// WaveManager로부터 직접 전달받은 스폰 정보(적 데이터, 수량, 간격)로 스폰 시작
    /// </summary>
    void StartSpawn(SpawnInfo spawnInfo)
    {
        var enemyData = spawnInfo.enemyData;
        if (enemyData == null || enemyData.enemyPrefab == null)
        {
            Debug.LogWarning($"[EnemySpawner] {name} 에 전달된 EnemyData 또는 enemyPrefab이 비어있습니다.");
            return;
        }

        _currentEnemyData = enemyData;
        _remainingSpawnCount = spawnInfo.count;
        _currentMaxHealthOverride = null;
        _currentScoreRewardOverride = null;
        _currentSpawnInterval = spawnInfo.interval;
        _aliveCount = 0;
        _timer = _currentSpawnInterval; // 활성화 즉시 첫 적을 스폰
        _isSpawning = true;
    }

    /// <summary>
    /// WaveManager가 무한 웨이브 모드에서 직접 호출하는 단발성 스폰.
    /// StartSpawn과 달리 자체 타이머(수량/간격)를 사용하지 않고, 호출 시점에 즉시 한 마리만 스폰한다.
    /// 스폰 타이밍과 난이도(체력/간격) 계산은 모두 WaveManager가 담당한다.
    /// </summary>
    public void SpawnOnce(EnemyData enemyData, float? maxHealthOverride = null, int? scoreRewardOverride = null)
    {
        if (enemyData == null || enemyData.enemyPrefab == null)
        {
            Debug.LogWarning($"[EnemySpawner] {name} 에 전달된 EnemyData 또는 enemyPrefab이 비어있습니다.");
            return;
        }

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        SpawnEnemy(enemyData, maxHealthOverride, scoreRewardOverride);
    }

    private void SpawnEnemy(EnemyData enemyData, float? maxHealthOverride, int? scoreRewardOverride)
    {
        if (enemyData == null || spawnPoint == null || waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning("[EnemySpawner] enemyData / spawnPoint / waypoints 를 Inspector에서 연결해주세요.");
            return;
        }

        var enemyObject = PoolManager.Instance.GetObject(enemyData.enemyPrefab);

        var poolable = enemyObject.GetComponent<EnemyPoolable>();
        if (poolable != null)
            poolable.Init(enemyData.enemyPrefab);

        var enemy = enemyObject.GetComponent<Enemy>();
        enemy.Initialize(enemyData);
        if (maxHealthOverride.HasValue)
            enemy.SetMaxHealth(maxHealthOverride.Value);
        if (scoreRewardOverride.HasValue)
            enemy.SetScoreReward(scoreRewardOverride.Value);

        enemy.Died += GamePlayManager.Instance.OnEnemyDied;
        enemy.Removed += OnSpawnedEnemyDied;

        _aliveCount++;

        var mover = enemyObject.GetComponent<EnemyMover>();
        if (mover != null)
        {
            mover.SetWaypoints(waypoints);
            mover.ActivateAt(spawnPoint.position);
        }
    }

    private void OnSpawnedEnemyDied(DamageableBehaviour damageable)
    {
        damageable.Removed -= OnSpawnedEnemyDied;

        _aliveCount--;
        if (_aliveCount < 0)
            _aliveCount = 0;

        CheckWaveCleared();
    }

    /// <summary>이번 wave 분량 스폰이 다 끝났고, 스폰한 적도 전부 죽었으면 WaveCleared 발생</summary>
    private void CheckWaveCleared()
    {
        if (!_isSpawning && _aliveCount <= 0)
        {
            WaveCleared?.Invoke(this);
        }
    }
}
