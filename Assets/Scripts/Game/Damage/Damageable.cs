using System;
using System.Collections.Generic;
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
        [Serializable]
        public class TypeCalculation
        {
            public DamageType damageType;
            public float multiplier = 1f;
        }

        /// <summary>
        /// 최대 체력
        /// </summary>
        float maxHealth;

        public float MaxHealth => maxHealth;

        List<TypeCalculation> _typeCalculations;

        /// <summary>
        /// 현재 체력
        /// </summary>
        public float CurrentHealth { get; protected set; }

        /// <summary>
        /// The alignment of the damager
        /// </summary>
        public SerializableIAlignmentProvider alignment;

        public event Action ReachedMaxHealth;

        public event Action<HealthChangeInfo> Damaged, Healed, Died, HealthChanged;


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


        /// <summary>
        /// Damageable 초기화
        /// </summary>
        public void Initialize(EnemyData data)
        {
            if (data.maxHealth <= 0)
            {
                return;
            }
            maxHealth = CurrentHealth = data.maxHealth;

            _typeCalculations = data.typeCalculations;
        }

        /// <summary>
        /// 최대 체력을 직접 재정의하고, 체력을 그 최댓값으로 채웁니다.
        /// EnemyData(공용 에셋)를 수정하지 않고 인스턴스마다 다른 최대 체력을 주고 싶을 때 사용합니다.
        /// (예: 무한 웨이브에서 웨이브가 오를수록 체력이 늘어나는 경우)
        /// </summary>
        public void OverrideMaxHealth(float newMaxHealth)
        {
            if (newMaxHealth <= 0f)
            {
                return;
            }

            maxHealth = newMaxHealth;
            CurrentHealth = maxHealth;
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

            HealthChanged?.Invoke(info);
        }

        /// <summary>
        /// 데미지를 받을 수 있는 관계인지 alignment를 사용해 확인하고,
        /// DamageType은 Normal(배율 1)로 처리합니다.
        /// </summary>
        /// <param name="damage">받을 데미지</param>
        /// <param name="damageAlignment">상대 전투원의 alignment</param>
        /// <param name="output">데미지를 받은 경우의 출력 데이터</param>
        /// <returns>데미지가 적용되었으면 true, 이미 사망했거나 데미지를 받을 수 없는 상태면 false.</returns>
        public bool TakeDamage(float damage, IAlignmentProvider damageAlignment, out HealthChangeInfo output)
        {
            return TakeDamage(damage, damageAlignment, DamageType.Normal, out output);
        }

        /// <summary>
        /// 데미지를 받을 수 있는 관계인지 alignment를 사용해 확인하고,
        /// typeCalculations에 등록된 배율을 damageType에 맞게 적용한 뒤 데미지를 적용합니다.
        /// </summary>
        /// <param name="damage">받을 데미지(배율 적용 전 원본 값)</param>
        /// <param name="damageAlignment">상대 전투원의 alignment</param>
        /// <param name="damageType">데미지의 속성. typeCalculations에서 배율을 찾는 데 사용</param>
        /// <param name="output">데미지를 받은 경우의 출력 데이터</param>
        /// <returns>데미지가 적용되었으면 true, 이미 사망했거나 데미지를 받을 수 없는 상태면 false.</returns>
        public bool TakeDamage(float damage, IAlignmentProvider damageAlignment, DamageType damageType, out HealthChangeInfo output)
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

            float scaledDamage = ApplyTypeCalculation(damage, damageType);

            ChangeHealth(-scaledDamage, output);
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
        /// typeCalculations 목록에서 damageType에 해당하는 배율을 찾아 damage에 적용
        /// 해당 타입에 대한 항목이 없으면 배율 1(원본 데미지)을 그대로 사용
        /// </summary>
        protected float ApplyTypeCalculation(float damage, DamageType damageType)
        {
            if (_typeCalculations == null)
            {
                return damage;
            }

            for (int i = 0; i < _typeCalculations.Count; i++)
            {
                if (_typeCalculations[i].damageType == damageType)
                {
                    return damage * _typeCalculations[i].multiplier;
                }
            }

            return damage;
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

            HealthChanged?.Invoke(info);
        }
    }
}
