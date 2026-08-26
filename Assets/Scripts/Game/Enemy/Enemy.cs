using ActionGameFramework.Health;
using Core.Health;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyMover))]
[RequireComponent(typeof(UI_EnemyHealthBar))]
[RequireComponent(typeof(EnemyPoolable))]
public class Enemy : Targetable
{
    public EnemyData _enemyData;

    /// <summary>지급 점수 오버라이드 값 (설정 시 EnemyData.scoreReward 대신 사용됨)</summary>
    private int? _scoreRewardOverride;

    /// <summary>실제 지급할 점수. 오버라이드가 없으면 EnemyData의 기본값을 사용</summary>
    public int ScoreReward => _scoreRewardOverride ?? _enemyData.scoreReward;

    private readonly List<StatusEffect> statusEffects = new();

    private UI_EnemyHealthBar healthBar;

    private EnemyPoolable _poolable;

    protected override void Awake()
    {
        base.Awake();
        healthBar = GetComponent<UI_EnemyHealthBar>();
        _poolable = GetComponent<EnemyPoolable>();
    }

    public void Initialize(EnemyData data)
    {
        _enemyData = data;
        configuration.Initialize(_enemyData);
        statusEffects.Clear();
    }

    protected override void HandleEnable()
    {
        Hit += OnHit;
    }

    protected override void HandleDisable()
    {
        Hit -= OnHit;
    }

    private void Update()
    {
        for (int i = statusEffects.Count - 1; i >= 0; i--)
        {
            var effect = statusEffects[i];

            effect.Tick(this, Time.deltaTime);

            if (effect.IsFinished)
            {
                effect.OnRemove(this);
                statusEffects.RemoveAt(i);
            }
        }
    }

    /// <summary>상태 이상 적용</summary>
    public void ApplyStatus(StatusEffect effect)
    {
        if (effect.Data.isStackable)
        {
            effect.OnApply(this);
            statusEffects.Add(effect);
            return;
        }

        StatusEffect existing = statusEffects.Find(x => x.GetType() == effect.GetType());

        if (existing != null)
        {
            existing.OnApply(this);
            return;
        }

        effect.OnApply(this);
        statusEffects.Add(effect);
    }

    private void OnHit(HitInfo hitInfo)
    {
        if (healthBar == null) return;
        healthBar.ShowAndUpdate(configuration.NormalisedHealth);
    }

    /// <summary>최대 체력을 재설정 (무한 웨이브처럼 웨이브마다 체력이 달라지는 경우 사용)</summary>
    public void SetMaxHealth(float maxHealth)
    {
        configuration.OverrideMaxHealth(maxHealth);
    }

    /// <summary>지급 점수를 재설정 (무한 웨이브처럼 웨이브마다 점수가 달라지는 경우 사용)</summary>
    public void SetScoreReward(int scoreReward)
    {
        _scoreRewardOverride = scoreReward;
    }

    /// <summary>적 클릭 시 호출되어 정보 패널을 표시</summary>
    public void OnClicked()
    {
        GameUIManager.Instance.ShowEnemyInfo(this);
    }

    public override void Remove()
    {
        base.Remove();
        _poolable.ReturnToPool();
    }
}
