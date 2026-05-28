using ActionGameFramework.Health;
using ActionGameFramework.Helpers;
using Core.Health;
using UnityEngine;

namespace ActionGameFramework.Projectiles
{
    /// <summary>
    /// LinearProjectile를 기반으로 목표물을 추적하는 호밍 투사체
    /// </summary>
    public class HomingLinearProjectile : LinearProjectile
    {
        public int leadingPrecision = 2;

        public bool leadTarget;

        public Targetable m_HomingTarget;
        protected Vector3 targetPos;

        protected Vector3 m_TargetVelocity;

        float destroyTimerProgress = 0;
        float destroyTimer = 5;

        /// <summary>
        /// 발사 후 추적할 대상 Transform 설정
        /// </summary>
        /// <param name="target">추적할 대상</param>
        public void Initialize(Targetable target)
        {
            m_HomingTarget = target;
            destroyTimerProgress = 0;
        }

        protected virtual void FixedUpdate()
        {
            if (m_HomingTarget == null)
            {
                return;
            }

            m_TargetVelocity = m_HomingTarget.Velocity;
        }

        protected override void Update()
        {
            TryDestroySelf();

            if (!m_Fired)
            {
                return;
            }

            if (m_HomingTarget == null)
            {
                m_Rigidbody.rotation = Quaternion.LookRotation(m_Rigidbody.linearVelocity);
                return;
            }

            Quaternion aimDirection = Quaternion.LookRotation(GetHeading());

            m_Rigidbody.rotation = aimDirection;
            m_Rigidbody.linearVelocity = transform.forward * m_Rigidbody.linearVelocity.magnitude;

            base.Update();
        }

        protected Vector3 GetHeading()
        {
            if (m_HomingTarget != null)
            {
                targetPos = m_HomingTarget.Position;
            }

            Vector3 heading;
            if (leadTarget)
            {
                heading = Ballistics.CalculateLinearLeadingTargetPoint(transform.position, targetPos,
                                                                       m_TargetVelocity, m_Rigidbody.linearVelocity.magnitude,
                                                                       acceleration,
                                                                       leadingPrecision) - transform.position;
            }
            else
            {
                heading = targetPos - transform.position;
            }

            return heading.normalized;
        }

        protected override void Fire(Vector3 firingVector)
        {
            if (m_HomingTarget == null)
            {
                Debug.LogError("목표물이 지정되지 않았습니다. 사격을 중단합니다.");
                return;
            }
            m_HomingTarget.Removed += OnTargetDied;

            base.Fire(firingVector);
        }

        void OnTargetDied(DamageableBehaviour targetable)
        {
            targetable.Removed -= OnTargetDied;
            m_HomingTarget = null;
        }

        protected void TryDestroySelf()
        {
            if (m_HomingTarget != null)
                return;

            destroyTimerProgress += Time.deltaTime;

            if (Vector3.Distance(transform.position, targetPos) < 0.1f || destroyTimerProgress > destroyTimer)
            {
                ReturnToPool();
            }
        }

    }
}