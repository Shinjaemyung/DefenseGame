using Core.Utilities;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField, Tooltip("스폰할 Enemy 프리팹")]
    private GameObject enemyPrefab;

    [SerializeField, Tooltip("스폰 위치")]
    private Transform spawnPoint;

    [SerializeField, Tooltip("순서대로 이동할 웨이포인트 배열")]
    private Transform[] waypoints;

    [SerializeField, Tooltip("스폰 간격 (초)")]
    private float spawnInterval = 5f;

    private float _timer;

    private void Awake()
    {
        spawnPoint = transform;
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= spawnInterval)
        {
            _timer = 0f;
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null || spawnPoint == null || waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning("[EnemySpawner] enemyPrefab / spawnPoint / waypoints 를 Inspector에서 연결해주세요.");
            return;
        }

        var enemy = PoolManager.Instance.GetObject(enemyPrefab);

        var poolable = enemy.GetComponent<EnemyPoolable>();
        if (poolable != null)
            poolable.Init(enemyPrefab);
        
        var mover = enemy.GetComponent<EnemyMover>();
        if (mover != null)
        {
            mover.ActivateAt(spawnPoint.position);
            mover.SetWaypoints(waypoints);
        }
    }
}
