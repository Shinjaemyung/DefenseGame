using System;
using System.Collections.Generic;
using ActionGameFramework.Health;
using Core.Health;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TowerDefense.Targetting
{
    /// <summary>
    /// Affector의 대상을 추적하는 데 사용되는 클래스
    /// </summary>
    public class Targetter : MonoBehaviour
    {
        /// <summary>
        /// Targetable이 범위에 들어올 때 발동
        /// </summary>
        public event Action<Targetable> TargetEntersRange;

        /// <summary>
        /// Targetable이 범위를 나갈 때 발동
        /// </summary>
        public event Action<Targetable> TargetExitsRange;

        /// <summary>
        /// 적절한 목표물이 발견되면 발사
        /// </summary>
        public event Action<Targetable> AcquiredTarget;

        /// <summary>
        /// 현재 타깃을 잃었을 때 발생하는 이벤트
        /// </summary>
        public event Action LostTarget;

        /// <summary>
        /// 타깃을 향해 회전할 mesh
        /// </summary>
        public Transform mesh;

        /// <summary>
        /// 터렛의 X축 회전 허용 범위
        /// </summary>
        public Vector2 turretXRotationRange = new Vector2(0, 359);

        /// <summary>
        /// 터렛이 Y축으로만 회전할지 여부
        /// </summary>
        public bool onlyYTurretRotation = true;

        /// <summary>
        /// 초당 타깃 탐색 횟수
        /// </summary>
        public float searchRate;

        /// <summary>
        /// 대기 상태에서의 Y축 회전 속도
        /// </summary>
        public float idleRotationSpeed = 39f;

        /// <summary>
        /// 대기 상태일 때 X축 회전을 원래 방향으로 복구하는 데 걸리는 시간
        /// </summary>
        public float idleCorrectionTime = 2.0f;

        /// <summary>
        /// Targetter에 연결된 콜라이더 (Awake에서 자동 설정)
        /// </summary>
        public Collider attachedCollider;

        /// <summary>
        /// TowerData 참조 — attackRange를 사거리(콜라이더 반지름)로 사용
        /// </summary>
        TowerData _towerData;

        /// <summary>
        /// 대기 상태에서 회전을 시작하기 전까지 기다리는 시간
        /// </summary>
        public float idleWaitTime = 2.0f;

        /// <summary>
        /// 현재 사거리(콜라이더) 안에 있는 타깃 목록
        /// </summary>
        protected List<Targetable> _targetsInRange = new List<Targetable>();

        /// <summary>
        /// 다음 탐색 가능 시점까지 남은 시간 타이머
        /// </summary>
        protected float _searchTimer = 0.0f;

        /// <summary>
        /// 대기 회전을 시작하기까지 남은 시간 타이머
        /// </summary>
        protected float _waitTimer = 0.0f;

        /// <summary>
        /// 현재 타깃(Targetable)
        /// </summary>
        protected Targetable _currentTargetable;
        /// <summary>
        /// 타깃이 잠겨 있는 동안(예: 공격 애니메이션 재생 중)에는
        /// 현재 타깃을 잃기 전까지 새 타깃으로 교체되지 않음
        /// </summary>
        protected bool _targetLocked;

        /// <summary>
        /// 타깃 잠금 여부
        /// </summary>
        public bool IsTargetLocked => _targetLocked;

        /// <summary>
        /// 타깃을 잠가 새로운 타깃으로 자동 교체되지 않도록 함
        /// (공격 애니메이션이 재생되는 동안 사용)
        /// </summary>
        public void LockTarget()
        {
            _targetLocked = true;
        }

        /// <summary>
        /// 타깃 잠금을 해제
        /// (공격 애니메이션 종료 시점에 호출)
        /// </summary>
        public void UnlockTarget()
        {
            if (!_targetLocked)
            {
                return;
            }

            _targetLocked = false;

            TryAcquireNearestTarget();
        }

        /// <summary>
        /// 사거리 내에서 가장 가까운 타깃을 찾아 지정
        /// </summary>
        protected void TryAcquireNearestTarget()
        {
            if (_targetLocked || _currentTargetable != null || _targetsInRange.Count == 0)
            {
                return;
            }

            _currentTargetable = GetNearestTargetable();
            if (_currentTargetable != null)
            {
                AcquiredTarget?.Invoke(_currentTargetable);
                _searchTimer = searchRate;
            }
        }



        /// <summary>
        /// X축 회전 보정에 사용되는 카운터
        /// </summary>
        protected float _xRotationCorrectionTime;

        /// <summary>
        /// 이전 프레임에 타깃이 있었는지 여부
        /// </summary>
        protected bool _hadTarget;

        /// <summary>
        /// 현재 터렛의 회전 속도
        /// </summary>
        protected float _currentRotationSpeed;

        /// <summary>
        /// affector의 alignment
        /// </summary>
        public IAlignmentProvider alignment;

        /// <summary>
        /// 콜라이더의 반지름을 반환
        /// </summary>
        public float EffectRadius
        {
            get
            {
                var sphere = attachedCollider as SphereCollider;
                if (sphere != null)
                    return sphere.radius;
 
                return 0;
            }
        }
        /// <summary>
        /// 현재 타깃 목록을 비우고 모든 이벤트를 초기화
        /// </summary>
        public void ResetTargetter()
        {
            _targetsInRange.Clear();
            _currentTargetable = null;
            _targetLocked = false;

            TargetEntersRange = null;
            TargetExitsRange = null;
            AcquiredTarget = null;
            LostTarget = null;

            if (mesh != null)
            {
                mesh.localRotation = Quaternion.identity;
            }
        }

        /// <summary>
        /// 적 타깃의 Targetable이 유효한지 확인
        /// </summary>
        /// <param name="targetable">검사할 타깃</param>
        /// <returns>Targetable이 있으면 true, 없으면 false</returns>
        protected virtual bool IsTargetableValid(Targetable targetable)
        {
            if (targetable == null)
            {
                return false;
            }

            IAlignmentProvider targetAlignment = targetable.configuration.AlignmentProvider;
            bool canDamage = alignment == null || targetAlignment == null ||
                             alignment.CanHarm(targetAlignment);

            return canDamage;
        }

        /// <summary>
        /// Targetable이 사거리 밖으로 나가면 타깃 리스트에서 제거
        /// </summary>
        /// <param name="other">충돌한 상대 콜라이더</param>
        protected virtual void OnTriggerExit(Collider other)
        {
            var targetable = other.GetComponent<Targetable>();
            if (!IsTargetableValid(targetable))
            {
                return;
            }

            _targetsInRange.Remove(targetable);
            TargetExitsRange?.Invoke(targetable);
            if (targetable == _currentTargetable)
            {
                OnTargetRemoved(targetable);
            }
            else
            {
                // 현재 타깃이 아닌 경우에만 여기서 제거
                // 현재 타겟이라면 OnTargetRemoved가 알아서 처리해 준다
                targetable.Removed -= OnTargetRemoved;
            }
        }

        /// <summary>
        /// Targetable이 사거리 안으로 들어오면 타깃 리스트에 추가
        /// </summary>
        /// <param name="other">충돌한 상대 콜라이더</param>
        protected virtual void OnTriggerEnter(Collider other)
        {
            var targetable = other.GetComponent<Targetable>();
            if (!IsTargetableValid(targetable))
            {
                return;
            }
            targetable.Removed += OnTargetRemoved;
            _targetsInRange.Add(targetable);
            TargetEntersRange?.Invoke(targetable);
        }

        /// <summary>
        /// 현재 추적 중인 Targetable 중 가장 가까운 대상 반환
        /// </summary>
        /// <returns>가장 가까운 Targetable. 없으면 null.</returns>
        protected virtual Targetable GetNearestTargetable()
        {
            int length = _targetsInRange.Count;

            if (length == 0)
            {
                return null;
            }

            Targetable nearest = null;
            float distance = float.MaxValue;
            for (int i = length - 1; i >= 0; i--)
            {
                Targetable targetable = _targetsInRange[i];
                if (targetable == null || targetable.IsDead)
                {
                    _targetsInRange.RemoveAt(i);
                    continue;
                }
                float currentDistance = Vector3.Distance(transform.position, targetable.Position);
                if (currentDistance < distance)
                {
                    distance = currentDistance;
                    nearest = targetable;
                }
            }

            return nearest;
        }

        /// <summary>
        /// SphereCollider를 가져오거나 없으면 생성하고,
        /// towerData.attackRange로 반지름을 설정한 뒤 트리거로 활성화
        /// </summary>
        protected virtual void Awake()
        {
            attachedCollider = GetComponent<SphereCollider>();
            if (attachedCollider == null)
            {
                attachedCollider = gameObject.AddComponent<SphereCollider>();
            }
            attachedCollider.isTrigger = true;

            _towerData = GetComponentInParent<Tower>().towerData;

            if (_towerData != null)
            {
                ((SphereCollider)attachedCollider).radius = _towerData.attackRange;
            }
            else
            {
                Debug.LogWarning($"[Targetter] {gameObject.name}: towerData가 할당되지 않았습니다. attackRange를 적용할 수 없습니다.");
            }

            attachedCollider.isTrigger = true;
            //attachedCollider.hideFlags = HideFlags.HideInInspector;
        }

        /// <summary>
        /// 타이머 초기화
        /// </summary>
        protected virtual void Start()
        {
            _searchTimer = searchRate;
            _waitTimer = idleWaitTime;
        }

        /// <summary>
        /// 파괴된 타깃을 정리하고, 필요 시 새 타깃을 획득
        /// </summary>
        protected virtual void Update()
        {
            _searchTimer -= Time.deltaTime;

            if (_searchTimer <= 0.0f)
            {
                TryAcquireNearestTarget();
            }

            AimTurret();
            _hadTarget = _currentTargetable != null;
        }

        /// <summary>
        /// 현재 타깃이 죽거나 사거리 밖으로 나갔을 때 호출
        /// LostTarget 이벤트를 발생
        /// </summary>
        void OnTargetRemoved(DamageableBehaviour target)
        {
            target.Removed -= OnTargetRemoved;
            if (_currentTargetable != null && target.configuration == _currentTargetable.configuration)
            {
                if (LostTarget != null)
                {
                    LostTarget();
                }
                _hadTarget = false;
                _targetsInRange.Remove(_currentTargetable);
                _currentTargetable = null;
                _xRotationCorrectionTime = 0.0f;
            }
            else //wasnt the current target, find and remove from targets list
            {
                for (int i = 0; i < _targetsInRange.Count; i++)
                {
                    if (_targetsInRange[i].configuration == target.configuration)
                    {
                        _targetsInRange.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 현재 타깃을 향해 조준
        /// </summary>
        protected virtual void AimTurret()
        {
            if (mesh == null)
            {
                return;
            }

            if (_currentTargetable == null) // 대기 상태 회전
            {
                if (_waitTimer > 0)
                {
                    _waitTimer -= Time.deltaTime;
                    if (_waitTimer <= 0)
                    {
                        _currentRotationSpeed = (Random.value * 2 - 1) * idleRotationSpeed;
                    }
                }
                else
                {
                    Vector3 euler = mesh.rotation.eulerAngles;
                    euler.x = Mathf.Lerp(Wrap180(euler.x), 0, _xRotationCorrectionTime);
                    _xRotationCorrectionTime = Mathf.Clamp01((_xRotationCorrectionTime + Time.deltaTime) / idleCorrectionTime);
                    //euler.y += _currentRotationSpeed * Time.deltaTime;

                    mesh.eulerAngles = euler;
                }
            }
            else
            {
                _waitTimer = idleWaitTime;

                Vector3 targetPosition = _currentTargetable.Position;
                if (onlyYTurretRotation)
                {
                    targetPosition.y = mesh.position.y;
                }
                Vector3 direction = targetPosition - mesh.position;
                Quaternion look = Quaternion.LookRotation(direction, Vector3.up);
                Vector3 lookEuler = look.eulerAngles;
                float x = Wrap180(lookEuler.x);
                lookEuler.x = Mathf.Clamp(x, turretXRotationRange.x, turretXRotationRange.y);
                look.eulerAngles = lookEuler;
                mesh.rotation = look;
            }
        }

        /// <summary>
        /// 각도를 -180 ~ 180 범위로 정규화
        /// </summary>
        static float Wrap180(float angle)
        {
            angle %= 360;
            if (angle < -180)
            {
                angle += 360;
            }
            else if (angle > 180)
            {
                angle -= 360;
            }
            return angle;
        }
    }
}
