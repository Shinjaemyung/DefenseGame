using ActionGameFramework.Health;
using Core.Utilities;
using UnityEngine;

namespace TowerDefense.Towers.Projectiles
{
    /// <summary>
    /// 적에게 즉시 데미지를 입히는 Hitscan Attack
    /// </summary>
    [RequireComponent(typeof(Damager))]
    public class HitscanAttack : Poolable
    {
        /// <summary>
        /// The amount of time to delay
        /// </summary>
        public float delay;

        /// <summary>
        /// The delay timer
        /// </summary>
        protected Timer _Timer;

        /// <summary>
        /// The enemy this projectile will attack
        /// </summary>
        protected Targetable _Enemy;

        /// <summary>
        /// The Damager attached to the object
        /// </summary>
        protected Damager _Damager;

        /// <summary>
        /// The towers projectile position
        /// </summary>
        protected Vector3 _Origin;

        /// <summary>
        /// Configuration for pausing the timer delay timer
        /// without setting Time.timeScale to 0
        /// </summary>
        protected bool _PauseTimer;

        /// <summary>
        /// The delay configuration for the attacking
        /// </summary>
        /// <param name="origin">
        /// The point the attack will be fired from
        /// </param>
        /// <param name="enemy">
        /// The enemy to attack
        /// </param>
        public void AttackEnemy(Vector3 origin, Targetable enemy)
        {
            _Enemy = enemy;
            _Origin = origin;
            _Timer.Reset();
            _PauseTimer = false;
        }

        /// <summary>
        /// Hitscan 공격의 실제 공격 동작
        /// 공격할 적이 없으면 return
        /// </summary>
        protected void DealDamage()
        {
            if (_Enemy == null)
            {
                ReturnToPool();
                return;
            }

            ReturnToPool();

            // effects
            /* 이펙트는 나중에
            ParticleSystem pfxPrefab = m_Damager.collisionParticles;
            var attackEffect = PoolManager.Instance.GetObject(pfxPrefab.gameObject).GetComponent<ParticleSystem>();
            attackEffect.transform.position = m_Enemy.position;
            attackEffect.Play();
            */
            _Enemy.TakeDamage(_Damager.damage, _Enemy.Position, _Damager.AlignmentProvider);
            _PauseTimer = true;
        }

        /// <summary>
        /// 이 오브젝트에 Damager 컴포넌트 캐싱
        /// </summary>
        protected virtual void Awake()
        {
            _Damager = GetComponent<Damager>();
            _Timer = new Timer(delay, DealDamage);
        }

        /// <summary>
        /// _Timer가 사용 가능하다면 업데이트
        /// </summary>
        protected virtual void Update()
        {
            if (!_PauseTimer)
            {
                _Timer.Tick(Time.deltaTime);
            }
        }
    }
}