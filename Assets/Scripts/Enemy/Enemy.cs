using ActionGameFramework.Health;
using Core.Health;
using UnityEngine;

[RequireComponent(typeof(EnemyMover))]
[RequireComponent(typeof(UI_EnemyHealthBar))]
[RequireComponent(typeof(EnemyPoolable))]
public class Enemy : Targetable
{
    [SerializeField] private UI_EnemyHealthBar healthBar;

    [SerializeField, Tooltip("Hero와 충돌 시 주는 데미지 (임시)")]
    public float attackDamage = 10f;

    private EnemyPoolable _poolable;

    protected override void Awake()
    {
        base.Awake();
        if (healthBar == null)
            healthBar = GetComponent<UI_EnemyHealthBar>();
        _poolable = GetComponent<EnemyPoolable>();
        Hit += OnHit;
    }

    void OnDestroy()
    {
        Hit -= OnHit;
    }

    private void OnHit(HitInfo hitInfo)
    {
        if (healthBar == null) return;
        healthBar.ShowAndUpdate(configuration.NormalisedHealth);
    }

    public override void Remove()
    {
        base.Remove();
        _poolable.ReturnToPool();
    }
}
