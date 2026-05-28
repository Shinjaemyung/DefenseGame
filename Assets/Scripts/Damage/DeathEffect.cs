using Core.Utilities;
using UnityEngine;

namespace Core.Health
{
    /// <summary>
    /// Simple class to instantiate a ParticleSystem on a given Damageable's death
    /// </summary>
    public class DeathEffect : MonoBehaviour
    {
        /// <summary>
        /// The DamageableBehaviour that will be used to assign the damageable
        /// </summary>
        [Tooltip("This field does not need to be populated here, it can be set up in code using AssignDamageable")]
        public DamageableBehaviour damageableBehaviour;

        /// <summary>
        /// Death particle system
        /// </summary>
        public ParticleSystem deathParticleSystemPrefab;

        /// <summary>
        /// World space offset of the <see cref="deathParticleSystemPrefab"/> position
        /// </summary>
        public Vector3 deathEffectOffset;

        /// <summary>
        /// The damageable
        /// </summary>
        protected Damageable m_Damageable;

        /// <summary>
        /// If damageableBehaviour is populated, assigns the damageable
        /// </summary>
        protected virtual void Awake()
        {
            if (damageableBehaviour != null)
            {
                AssignDamageable(damageableBehaviour.configuration);
            }
        }

        /// <summary>
        /// Subscribes to the damageable's died event
        /// </summary>
        /// <param name="damageable"></param>
        public void AssignDamageable(Damageable damageable)
        {
            if (m_Damageable != null)
            {
                m_Damageable.Died -= OnDied;
            }
            m_Damageable = damageable;
            m_Damageable.Died += OnDied;
        }

        /// <summary>
        /// Instantiate a death particle system
        /// </summary>
        void OnDied(HealthChangeInfo healthChangeInfo)
        {
            if (deathParticleSystemPrefab == null)
            {
                return;
            }
            
            /* Pool 기능에 Particle 관련도 확인 필요
            var pfx = Poolable.TryGetPoolable<ParticleSystem>(deathParticleSystemPrefab.gameObject);
            pfx.transform.position = transform.position + deathEffectOffset;
            pfx.Play();
            */
        }
    }
}
