using System;
using UnityEngine;

namespace Core.Health
{
    /// <summary>
    /// 데미지를 받을 수 있는 MonoBehaviour
    /// </summary>
    public class DamageableBehaviour : MonoBehaviour
    {
        /// <summary>
        /// Damageable
        /// </summary>
        public Damageable configuration;

        /// <summary>
        /// 오브젝트 사망 여부
        /// </summary>
        /// <value>죽었다면 True</value>
        public bool IsDead => configuration.IsDead;

        /// <summary>
        /// 오브젝트의 position
        /// </summary>
        public virtual Vector3 Position => transform.position; 

        /// <summary>
        /// 데미지를 받았을 때 발생하는 이벤트
        /// </summary>
        public event Action<HitInfo> Hit;

        /// <summary>
        /// 사망 시 호출되는 이벤트
        /// </summary>
        public event Action<DamageableBehaviour> Died;

        /// <summary>
        /// 풀로 반환되거나 파괴될 때 호출되는 이벤트
        /// </summary>
        public event Action<DamageableBehaviour> Removed;

        protected virtual void Awake()
        {

        }

        protected void OnEnable()
        {
            configuration.Died += OnConfigurationDied;
            HandleEnable();
        }

        private void OnDisable()
        {
            configuration.Died -= OnConfigurationDied;
            HandleDisable();
        }

        protected virtual void HandleEnable() { }
        protected virtual void HandleDisable() { }

        /// <summary>
        /// 실제 데미지 적용. typeCalculations에 등록된 배율을 damageType에 맞게 적용한 뒤 데미지를 적용.
        /// </summary>
        /// <param name="damageValue">입힐 데미지(배율 적용 전 원본 값)</param>
        /// <param name="damagePoint">피격 지점</param>
        /// <param name="alignment">Alignment 정보</param>
        /// <param name="damageType">데미지의 속성</param>
        public virtual void TakeDamage(float damageValue, DamageType damageType, Vector3 damagePoint, IAlignmentProvider alignment)
        {
            HealthChangeInfo info;
            configuration.TakeDamage(damageValue, alignment, damageType, out info);
            var damageInfo = new HitInfo(info, damagePoint);
            Hit?.Invoke(damageInfo);
        }

        /// <summary>
        /// 이 Damageable을 사망 상태로 만듬
        /// </summary>
        protected virtual void Kill()
        {
            HealthChangeInfo healthChangeInfo;
            configuration.TakeDamage(configuration.CurrentHealth, null, out healthChangeInfo);
        }


        /// <summary>
        /// 이 Damageable을 제거
        /// </summary>
        public virtual void Remove()
        {
            // Set health to zero so that this behaviour appears to be dead. This will not fire death events
            configuration.SetHealth(0);
            OnRemoved();
        }

        /// <summary>
        /// 사망 이벤트 발생
        /// </summary>
        void OnDeath()
        {
            Died?.Invoke(this);
        }

        /// <summary>
        /// 제거 이벤트 발생
        /// </summary>
        void OnRemoved()
        {
            Removed?.Invoke(this);
        }

        /// <summary>
        /// 공격을 받아 죽었을 때 발생
        /// </summary>
        void OnConfigurationDied(HealthChangeInfo changeInfo)
        {
            OnDeath();
            Remove();
        }
    }
}
