using System;
using UnityEngine;

namespace Core.Health
{
    /// <summary>
    /// 이벤트 기반 체력을 처리하는 담당하는 클래스
    /// 플레이어, 적 등 파괴 가능한 오브젝트에 사용
    /// </summary>
    [Serializable]
    public class Damageable
    {
        /// <summary>
        /// 최대 체력
        /// </summary>
        public float maxHealth;

        public float startingHealth;

        /// <summary>
        /// 현재 체력
        /// </summary>
        public float CurrentHealth { get; protected set; }



        /// <summary>
        /// The alignment of the damager
        /// </summary>
        public SerializableIAlignmentProvider alignment;


        /// <summary>
        /// 정규화된 체력
        /// </summary>
        public float NormalisedHealth
        {
            get
            {
                if (Math.Abs(maxHealth) <= Mathf.Epsilon)
                {
                    Debug.LogError("Max Health is 0");
                    maxHealth = 1f;
                }
                return CurrentHealth / maxHealth;
            }
        }

        /// <summary>
        /// 이 인스턴스의 <see cref="IAlignmentProvider"/>를 가져옵니다
        /// </summary>
        public IAlignmentProvider AlignmentProvider
        {
            get
            {
                return alignment?.GetInterface();
            }
        }

        /// <summary>
        /// 사망했는지 여부
        /// </summary>
        public bool IsDead
        {
            get { return CurrentHealth <= 0f; }
        }

        /// <summary>
        /// 현재 최대 체력인지 여부
        /// </summary>
        public bool IsAtMaxHealth
        {
            get { return Mathf.Approximately(CurrentHealth, maxHealth); }
        }

        public event Action ReachedMaxHealth;

        public event Action<HealthChangeInfo> Damaged, Healed, Died, HealthChanged;

        /// <summary>
        /// 현재 체력을 시작 체력 값으로 초기화
        /// </summary>
        public virtual void Init()
        {
            CurrentHealth = startingHealth;
        }

        /// <summary>
        /// 최대 체력과 시작 체력을 동일한 값으로 설정합니다
        /// </summary>
        public void SetMaxHealth(float health)
        {
            if (health <= 0)
            {
                return;
            }
            maxHealth = startingHealth = health;
        }

        /// <summary>
        /// 최대 체력과 시작 체력을 설정
        /// </summary>
        public void SetMaxHealth(float health, float startingHealth)
        {
            if (health <= 0)
            {
                return;
            }
            maxHealth = health;
            this.startingHealth = startingHealth;
        }

        /// <summary>
        /// 이 인스턴스의 체력을 직접 설정
        /// </summary>
        /// <param name="health">
        /// <see cref="CurrentHealth"/>에 설정할 값
        /// </param>
        public void SetHealth(float health)
        {
            var info = new HealthChangeInfo
            {
                damageable = this,
                newHealth = health,
                oldHealth = CurrentHealth
            };

            CurrentHealth = health;

            if (HealthChanged != null)
            {
                HealthChanged(info);
            }
        }

        /// <summary>
        /// 데미지를 받을 수 있는 관계인지 alignment를 사용해 확인
        /// </summary>
        /// <param name="damage">
        /// 받을 데미지
        /// </param>
        /// <param name="damageAlignment">
        /// 상대 전투원의 alignment
        /// </param>
        /// <param name="output">
        /// 데미지를 받은 경우의 출력 데이터
        /// </param>
        /// <returns>
        /// 데미지가 적용되었으면 true,
        /// 이미 사망했거나 데미지를 받을 수 없는 상태면 false.
        /// </returns>
        public bool TakeDamage(float damage, IAlignmentProvider damageAlignment, out HealthChangeInfo output)
        {
            output = new HealthChangeInfo
            {
                damageAlignment = damageAlignment,
                damageable = this,
                newHealth = CurrentHealth,
                oldHealth = CurrentHealth
            };

            bool canDamage = damageAlignment == null || AlignmentProvider == null ||
                             damageAlignment.CanHarm(AlignmentProvider);

            if (IsDead || !canDamage)
            {
                return false;
            }

            ChangeHealth(-damage, output);
            Damaged?.Invoke(output);
            if (IsDead)
            {
                Died?.Invoke(output);
            }
            return true;
        }

        /// <summary>
        /// 체력을 증가시키는 로직입니다.
        /// </summary>
        /// <param name="health">체력.</param>
        public HealthChangeInfo IncreaseHealth(float health)
        {
            var info = new HealthChangeInfo { damageable = this };
            ChangeHealth(health, info);
            Healed?.Invoke(info);
            if (IsAtMaxHealth)
            {
                ReachedMaxHealth?.Invoke();
            }

            return info;
        }

        /// <summary>
        /// 현재 체력을 변경하고 변경 이벤트를 호출합니다.
        /// </summary>
        /// <param name="healthIncrement">체력 증감량.</param>
        /// <param name="info">체력 변경 정보를 담는 객체.</param>
        protected void ChangeHealth(float healthIncrement, HealthChangeInfo info)
        {
            info.oldHealth = CurrentHealth;
            CurrentHealth += healthIncrement;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, maxHealth);
            info.newHealth = CurrentHealth;

            if (HealthChanged != null)
            {
                HealthChanged(info);
            }
        }
    }
}