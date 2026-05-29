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
        /// Targetable이 Target 콜라이더에 들어올 때 발동
        /// </summary>
        public event Action<Targetable> TargetEntersRange;

        /// <summary>
        /// Targetable이 Target 콜라이더를 나갈 때 발동
        /// </summary>
        public event Action<Targetable> TargetExitsRange;

        /// <summary>
        /// 적절한 목표물이 발견되면 발사
        /// </summary>
        public event Action<Targetable> AcquiredTarget;

        /// <summary>
        /// Fires when the current target was lost
        /// </summary>
        public event Action LostTarget;

        /// <summary>
        /// The transform to point at the target
        /// </summary>
        public Transform mesh;

        /// <summary>
        /// 터렛의 x축 회전 범위
        /// </summary>
        public Vector2 turretXRotationRange = new Vector2(0, 359);

        /// <summary>
        /// If m_Turret rotates freely or only on y;
        /// </summary>
        public bool onlyYTurretRotation;

        /// <summary>
        /// The search rate in searches per second
        /// </summary>
        public float searchRate;

        /// <summary>
        /// Y rotation speed while the turret is idle in degrees per second
        /// </summary>
        public float idleRotationSpeed = 39f;

        /// <summary>
        /// The time it takes for the tower to correct its x rotation on idle in seconds
        /// </summary>
        public float idleCorrectionTime = 2.0f;

        /// <summary>
        /// The collider attached to the targetter
        /// </summary>
        public Collider attachedCollider;

        /// <summary>
        /// How long the turret waits in its idle form before spinning in seconds
        /// </summary>
        public float idleWaitTime = 2.0f;

        /// <summary>
        /// The current targetables in the collider
        /// </summary>
        protected List<Targetable> _TargetsInRange = new List<Targetable>();

        /// <summary>
        /// The seconds until a search is allowed
        /// </summary>
        protected float _SearchTimer = 0.0f;

        /// <summary>
        /// The seconds until the tower starts spinning
        /// </summary>
        protected float _WaitTimer = 0.0f;

        /// <summary>
        /// The current targetable
        /// </summary>
        protected Targetable _CurrrentTargetable;

        /// <summary>
        /// Counter used for x rotation correction
        /// </summary>
        protected float _XRotationCorrectionTime;

        /// <summary>
        /// If there was a targetable in the last frame
        /// </summary>
        protected bool _HadTarget;

        /// <summary>
        /// How fast this turret is spinning
        /// </summary>
        protected float _CurrentRotationSpeed;

        /// <summary>
        /// The alignment of the affector
        /// </summary>
        public IAlignmentProvider alignment;

        /// <summary>
        /// returns the radius of the collider whether
        /// its a sphere or capsule
        /// </summary>
        public float EffectRadius
        {
            get
            {
                var sphere = attachedCollider as SphereCollider;
                if (sphere != null)
                {
                    return sphere.radius;
                }
                var capsule = attachedCollider as CapsuleCollider;
                if (capsule != null)
                {
                    return capsule.radius;
                }
                return 0;
            }
        }

        /// <summary>
        /// Returns the current target
        /// </summary>
        public Targetable GetTarget()
        {
            return _CurrrentTargetable;
        }

        /// <summary>
        /// Clears the list of current targets and clears all events
        /// </summary>
        public void ResetTargetter()
        {
            _TargetsInRange.Clear();
            _CurrrentTargetable = null;

            TargetEntersRange = null;
            TargetExitsRange = null;
            AcquiredTarget = null;
            LostTarget = null;

            // Reset turret facing
            if (mesh != null)
            {
                mesh.localRotation = Quaternion.identity;
            }
        }

        /// <summary>
        /// Returns all the targets within the collider. This list must not be changed as it is the working
        /// list of the targetter. Changing it could break the targetter
        /// </summary>
        public List<Targetable> GetAllTargets()
        {
            return _TargetsInRange;
        }

        /// <summary>
        /// Checks if the targetable is a valid target
        /// </summary>
        /// <param name="targetable"></param>
        /// <returns>true if targetable is vaild, false if not</returns>
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
        /// On exiting the trigger, a valid targetable is removed from the tracking list.
        /// </summary>
        /// <param name="other">The other collider in the collision</param>
        protected virtual void OnTriggerExit(Collider other)
        {
            var targetable = other.GetComponent<Targetable>();
            if (!IsTargetableValid(targetable))
            {
                return;
            }

            _TargetsInRange.Remove(targetable);
            TargetExitsRange?.Invoke(targetable);
            if (targetable == _CurrrentTargetable)
            {
                OnTargetRemoved(targetable);
            }
            else
            {
                // Only need to remove if we're not our actual target, otherwise OnTargetRemoved will do the work above
                targetable.Removed -= OnTargetRemoved;
            }
        }

        /// <summary>
        /// On entering the trigger, a valid targetable is added to the tracking list.
        /// </summary>
        /// <param name="other">The other collider in the collision</param>
        protected virtual void OnTriggerEnter(Collider other)
        {
            var targetable = other.GetComponent<Targetable>();
            if (!IsTargetableValid(targetable))
            {
                return;
            }
            targetable.Removed += OnTargetRemoved;
            _TargetsInRange.Add(targetable);
            TargetEntersRange?.Invoke(targetable);
        }

        /// <summary>
        /// Returns the nearest targetable within the currently tracked targetables 
        /// </summary>
        /// <returns>The nearest targetable if there is one, null otherwise</returns>
        protected virtual Targetable GetNearestTargetable()
        {
            int length = _TargetsInRange.Count;

            if (length == 0)
            {
                return null;
            }

            Targetable nearest = null;
            float distance = float.MaxValue;
            for (int i = length - 1; i >= 0; i--)
            {
                Targetable targetable = _TargetsInRange[i];
                if (targetable == null || targetable.IsDead)
                {
                    _TargetsInRange.RemoveAt(i);
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
        /// Starts the search timer
        /// </summary>
        protected virtual void Start()
        {
            _SearchTimer = searchRate;
            _WaitTimer = idleWaitTime;
        }

        /// <summary>
        /// Checks if any targets are destroyed and aquires a new targetable if appropriate
        /// </summary>
        protected virtual void Update()
        {
            _SearchTimer -= Time.deltaTime;

            if (_SearchTimer <= 0.0f && _CurrrentTargetable == null && _TargetsInRange.Count > 0)
            {
                _CurrrentTargetable = GetNearestTargetable();
                if (_CurrrentTargetable != null)
                {
                    AcquiredTarget?.Invoke(_CurrrentTargetable);
                    _SearchTimer = searchRate;
                }
            }

            AimTurret();
            _HadTarget = _CurrrentTargetable != null;
        }

        /// <summary>
        /// Fired by the agents died event or when the current target moves out of range,
        /// Fires the lostTarget event.
        /// </summary>
        void OnTargetRemoved(DamageableBehaviour target)
        {
            target.Removed -= OnTargetRemoved;
            if (_CurrrentTargetable != null && target.configuration == _CurrrentTargetable.configuration)
            {
                if (LostTarget != null)
                {
                    LostTarget();
                }
                _HadTarget = false;
                _TargetsInRange.Remove(_CurrrentTargetable);
                _CurrrentTargetable = null;
                _XRotationCorrectionTime = 0.0f;
            }
            else //wasnt the current target, find and remove from targets list
            {
                for (int i = 0; i < _TargetsInRange.Count; i++)
                {
                    if (_TargetsInRange[i].configuration == target.configuration)
                    {
                        _TargetsInRange.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 현재 목표물을 향해 조준
        /// </summary>
        protected virtual void AimTurret()
        {
            if (mesh == null)
            {
                return;
            }

            if (_CurrrentTargetable == null) // 대기 상태 회전
            {
                if (_WaitTimer > 0)
                {
                    _WaitTimer -= Time.deltaTime;
                    if (_WaitTimer <= 0)
                    {
                        _CurrentRotationSpeed = (Random.value * 2 - 1) * idleRotationSpeed;
                    }
                }
                else
                {
                    Vector3 euler = mesh.rotation.eulerAngles;
                    euler.x = Mathf.Lerp(Wrap180(euler.x), 0, _XRotationCorrectionTime);
                    _XRotationCorrectionTime = Mathf.Clamp01((_XRotationCorrectionTime + Time.deltaTime) / idleCorrectionTime);
                    euler.y += _CurrentRotationSpeed * Time.deltaTime;

                    mesh.eulerAngles = euler;
                }
            }
            else
            {
                _WaitTimer = idleWaitTime;

                Vector3 targetPosition = _CurrrentTargetable.Position;
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