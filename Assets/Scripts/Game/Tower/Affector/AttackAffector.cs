//using ActionGameFramework.Audio;
using ActionGameFramework.Health;
using Core.Health;
using System.Collections.Generic;
using TowerDefense.Targetting;
using TowerDefense.Towers;
using TowerDefense.Towers.Projectiles;
using UnityEngine;
using static AttackUtility;

namespace TowerDefense.Affectors
{
    /// <summary>
    /// 투사체 발사 공격을 처리하는 공통 클래스
    /// 
    /// 이 스크립트를 추가하기 전에 같은 GameObject에 ILauncher 구현체 추가 필수
    /// </summary>
    [RequireComponent(typeof(ILauncher))]
    public class AttackAffector : Affector, ITowerRadiusProvider
    {
        [SerializeField, Tooltip("공격에 사용할 투사체")]
        GameObject projectile;

        [SerializeField, Tooltip("공격 시 재생할 오디오 소스")]
        AudioClip fireClip;

        /// <summary>
        /// 투사체를 발사할 위치 리스트
        /// </summary>
        public Transform[] projectilePoints;

        /// <summary>
        /// 타워가 적을 탐색할 기준 중심점
        /// </summary>
        public Transform epicenter;

        /// <summary>
        /// 다중 공격(범위 공격) 여부 설정
        /// </summary>
        public bool isMultiAttack;

        /// <summary>
        /// 초당 공격 횟수
        /// </summary>
        [HideInInspector]
        public float fireRate;

        public Targetter towerTargetter;

        /// <summary>
        /// 공격 범위 시각화 색상
        /// </summary>
        public Color radiusEffectColor;

        /// <summary>
        /// bool 값을 반환하는 조건식 델리게이트
        /// </summary>
        public delegate bool Filter();

        public Filter searchCondition;
        public Filter fireCondition;

        protected ILauncher _launcher;

        /// <summary>
        /// TowerData 캐시
        /// </summary>
        protected TowerData _towerData;

        /// <summary>
        /// 다음 공격 가능까지 남은 시간
        /// </summary>
        protected float _fireTimer;

        /// <summary>
        /// 현재 추적 중인 적
        /// </summary>
        protected Targetable _trackingEnemy;

        /// <summary>
        /// 공격할 적
        /// </summary>
        protected Targetable _attackTarget;

        /// <summary>
        /// 공격할 적의 위치
        /// </summary>
        protected Vector3 _attackTargetPos;

        /// <summary>
        /// 애니메이션 시작 시점에 저장한 _attackTarget의 SpawnId.
        /// FireProjectile() 시점에 비교해서 그 사이에 pool 반환→재스폰됐는지 검증.
        /// </summary>
        private int _attackTargetSpawnId = -1;

        [SerializeField]
        Animator animator;
        AudioSource audioSource;

        /// <summary>
        /// Animator가 있는 GameObject에서 AnimationEvent를 대신 받아 전달해주는 Relay
        /// </summary>
        AnimationEventRelay _animationEventRelay;

        /// <summary>Attack 파라미터 해시</summary>
        private static readonly int AttackHash = Animator.StringToHash("Attack");

        public float EffectRadius => towerTargetter.EffectRadius;
        public Color EffectColor => radiusEffectColor;
        public Targetter Targetter => towerTargetter;


        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            Tower tower = GetComponentInParent<Tower>();
            epicenter = tower.GetComponent<Transform>();
            _towerData = tower.towerData;

            // Animator가 있는 GameObject(TowerMesh 등)에서 AnimationEvent(OnAttackAnimationEvent)를
            // 대신 받아줄 Relay를 찾아 구독한다. 없으면 추가한다.
            if (animator != null)
            {
                _animationEventRelay = animator.GetComponent<AnimationEventRelay>();
                if (_animationEventRelay == null)
                    _animationEventRelay = animator.gameObject.AddComponent<AnimationEventRelay>();

                _animationEventRelay.OnAttack += FireProjectile;
            }
        }

        /// <summary>
        /// AttackAffector 초기화
        /// </summary>
        public override void Initialize()
        {
            Initialize(-1);
        }

        /// <summary>
        /// 레이어 마스크로 AttackAffector 초기화
        /// </summary>
        public override void Initialize(LayerMask mask)
        {
            base.Initialize(mask);
            SetUpTimers();

            towerTargetter.ResetTargetter();
            //towerTargetter.alignment = affectorAlignment;
            towerTargetter.AcquiredTarget += OnAcquiredTarget;
            towerTargetter.LostTarget += OnLostTarget;
        }

        void OnLostTarget()
        {
            _trackingEnemy = null;
        }

        void OnAcquiredTarget(Targetable acquiredTarget)
        {
            _trackingEnemy = acquiredTarget;
        }

        /// <summary>
        /// 공격 타이머 초기화
        /// </summary>
        protected virtual void SetUpTimers()
        {
            if (_towerData == null)
            {
                Tower tower = GetComponentInParent<Tower>();
                if (tower != null) _towerData = tower.towerData;
            }

            _fireTimer = 1 / _towerData.attackSpeed;
            _launcher = GetComponent<ILauncher>();
        }

        /// <summary>
        /// 공격 타이머 갱신
        /// </summary>
        protected virtual void Update()
        {
            _fireTimer -= Time.deltaTime;
            if (_trackingEnemy != null && _fireTimer <= 0.0f)
            {
                OnFireAnimation();
                _fireTimer = 1 / _towerData.attackSpeed;
            }
        }

        /// <summary>
        /// 공격 주기마다 호출. 애니메이터가 있으면 애니메이션만 재생하고
        /// 실제 발사는 Animation Event(OnAttackAnimationEvent)에서 처리.
        /// 애니메이터가 없으면 즉시 발사.
        /// </summary>
        protected virtual void OnFireAnimation()
        {
            if (fireCondition != null && !fireCondition())
                return;

            _attackTarget = _trackingEnemy;

            // 애니메이션 시작 시점의 SpawnId를 저장.
            // 애니메이션 재생 도중 _attackTarget이 pool 반환→재스폰되면
            // SpawnId가 바뀌므로 FireProjectile()에서 발사 방식 변경
            var poolable = _attackTarget.GetComponent<Poolable>();
            _attackTargetSpawnId = poolable.SpawnId;
            _attackTargetPos = _attackTarget.gameObject.transform.position;

            if (animator != null)
            {
                // 공격 속도에 맞게 애니메이션 속도 조절
                var clips = animator.runtimeAnimatorController.animationClips;
                float clipLength = clips.Length > 0 ? clips[0].length : 1f;
                animator.speed = clipLength * _towerData.attackSpeed;;
                animator.ResetTrigger(AttackHash);
                animator.SetTrigger(AttackHash);
            }
            else
            {
                FireProjectile();
            }
        }

        /// <summary>
        /// 공격 시 수행되는 로직
        /// </summary>
        protected virtual void FireProjectile()
        {
            if (_attackTarget == null)
                return;

            // 애니메이션 시작 이후 _attackTarget이 pool 반환→재스폰됐는지 검증
            var poolable = _attackTarget.GetComponent<Poolable>();
            if (poolable != null && (poolable.SpawnId != _attackTargetSpawnId || poolable.IsPoolReturned))
            {
                _launcher.LaunchToPosition(_attackTargetPos, projectile, projectilePoints);
            }
            else
            {
                if (isMultiAttack)
                {
                    //List<Targetable> enemies = towerTargetter.GetAllTargets();
                    //_launcher.Launch(enemies, projectile, projectilePoints);
                }
                else
                {
                    AttackContext context = new AttackContext(_towerData.damage, _towerData.damageType, _towerData.statusEffects);
                    _launcher.Launch(_attackTarget, projectile, projectilePoints, context);
                }
            }

            if (audioSource != null && fireClip != null)
                audioSource.PlayOneShot(fireClip);
        }

        void OnDestroy()
        {
            towerTargetter.AcquiredTarget -= OnAcquiredTarget;
            towerTargetter.LostTarget -= OnLostTarget;

            if (_animationEventRelay != null)
                _animationEventRelay.OnAttack -= FireProjectile;
        }

        /*
#if UNITY_EDITOR
        /// <summary>
        /// 탐색 범위 시각화
        /// </summary>
        void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(epicenter.position, towerTargetter.effectRadius);
        }
#endif
        */
    }
}
