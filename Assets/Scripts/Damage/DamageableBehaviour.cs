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
        public bool IsDead
        {
            get { return configuration.IsDead; }
        }

        /// <summary>
        /// 오브젝트의 position
        /// </summary>
        public virtual Vector3 Position
        {
            get { return transform.position; }
        }

        /// <summary>
        /// 데미지를 받았을 때 발생하는 이벤트
        /// </summary>
        public event Action<HitInfo> Hit;

        /// <summary>
        /// 풀로 반환되거나 파괴될 때 호출되는 이벤트
        /// </summary>
        public event Action<DamageableBehaviour> Removed;

        /// <summary>
        /// 사망 시 호출되는 이벤트
        /// </summary>
        public event Action<DamageableBehaviour> Died;

        protected virtual void Awake()
        {
            configuration.Init();
            configuration.Died += OnConfigurationDied;
        }

        /// <summary>
        /// Takes the damage and also provides a position for the damage being dealt
        /// </summary>
        /// <param name="damageValue">Damage value.</param>
        /// <param name="damagePoint">Damage point.</param>
        /// <param name="alignment">Alignment value</param>
        public virtual void TakeDamage(float damageValue, Vector3 damagePoint, IAlignmentProvider alignment)
        {
            HealthChangeInfo info;
            configuration.TakeDamage(damageValue, alignment, out info);
            var damageInfo = new HitInfo(info, damagePoint);
            if (Hit != null)
            {
                Hit(damageInfo);
            }
        }

        /// <summary>
        /// Kills this damageable
        /// </summary>
        protected virtual void Kill()
        {
            HealthChangeInfo healthChangeInfo;
            configuration.TakeDamage(configuration.CurrentHealth, null, out healthChangeInfo);
        }


        /// <summary>
        /// Removes this damageable without killing it
        /// </summary>
        public virtual void Remove()
        {
            // Set health to zero so that this behaviour appears to be dead. This will not fire death events
            configuration.SetHealth(0);
            OnRemoved();
        }

        /// <summary>
        /// Fires kill events
        /// </summary>
        void OnDeath()
        {
            Died?.Invoke(this);
        }

        /// <summary>
        /// Fires the removed event
        /// </summary>
        void OnRemoved()
        {
            Removed?.Invoke(this);
        }

        /// <summary>
        /// Event fired when Damageable takes critical damage
        /// </summary>
        void OnConfigurationDied(HealthChangeInfo changeInfo)
        {
            OnDeath();
            Remove();
        }
    }
}