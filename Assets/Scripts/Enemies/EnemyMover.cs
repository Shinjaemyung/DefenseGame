using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// NavMeshAgent를 이용해 웨이포인트를 순서대로 이동하는 컴포넌트.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMover : MonoBehaviour
{
    [SerializeField, Tooltip("이동 속도")]
    private float moveSpeed = 3.5f;

    [SerializeField, Tooltip("다음 웨이포인트로 넘어가는 도달 판정 거리")]
    private float waypointReachDistance = 1.5f;

    private NavMeshAgent _agent;
    private Transform[] _waypoints;
    public int _currentWaypointIndex;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.speed = moveSpeed;
        _agent.stoppingDistance = 0f;
    }

    void Update()
    {
        if (_waypoints == null || _waypoints.Length == 0) return;
        if (!_agent.isOnNavMesh || _agent.pathPending) return;
        if (_agent.remainingDistance <= waypointReachDistance)
            MoveToNextWaypoint();
    }

    /// <summary>풀 반환 시 상태 초기화</summary>
    public void ResetState()
    {
        _agent.ResetPath();
        _agent.enabled = false;
        _waypoints = null;
        _currentWaypointIndex = 0;
    }

    /// <summary>지정한 위치에서 활성화. Warp로 NavMeshAgent 위치를 강제 설정.</summary>
    public void ActivateAt(Vector3 position)
    {
        _agent.enabled = true;
        _agent.Warp(position);
    }

    /// <summary>웨이포인트 배열 설정 및 첫 번째 목적지로 이동 시작</summary>
    public void SetWaypoints(Transform[] waypoints)
    {
        _waypoints = waypoints;
        _currentWaypointIndex = 0;
        SetDestination(_waypoints[_currentWaypointIndex].position);
    }

    private void MoveToNextWaypoint()
    {
        _currentWaypointIndex++;
        if (_currentWaypointIndex >= _waypoints.Length)
        {
            _agent.ResetPath();
            return;
        }
        SetDestination(_waypoints[_currentWaypointIndex].position);
    }

    /// <summary>목적지 설정</summary>
    public void SetDestination(Vector3 destination)
    {
        if (_agent.isOnNavMesh)
            _agent.SetDestination(destination);
    }

    /// <summary>목적지 도달 여부 반환</summary>
    public bool HasReachedDestination()
    {
        if (!_agent.isOnNavMesh || _agent.pathPending) return false;
        return _agent.remainingDistance <= _agent.stoppingDistance;
    }
}
