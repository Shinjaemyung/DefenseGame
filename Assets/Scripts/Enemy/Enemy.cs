using ActionGameFramework.Health;
using Core.Health;
using UnityEngine;

[RequireComponent(typeof(EnemyMover))]
[RequireComponent(typeof(UI_EnemyHealthBar))]
[RequireComponent(typeof(EnemyPoolable))]
public class Enemy : Targetable
{
    [SerializeField] private UI_EnemyHealthBar healthBar;

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
