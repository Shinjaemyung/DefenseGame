using UnityEngine;

/// <summary>
/// Enemy 전용 Poolable. OnSpawn/OnDespawn에서 Enemy 전체 상태를 관리.
/// </summary>
public class EnemyPoolable : Poolable
{
    private Enemy _enemy;
    private EnemyMover _mover;
    private UI_EnemyHealthBar _healthBar;

    private void Awake()
    {
        _enemy = GetComponent<Enemy>();
        _mover = GetComponent<EnemyMover>();
        _healthBar = GetComponent<UI_EnemyHealthBar>();
    }

    public override void OnSpawn()
    {
        base.OnSpawn();
        _enemy.configuration.Init();
    }

    public override void OnDespawn()
    {
        _mover.ResetState();
        _healthBar.Hide();
        base.OnDespawn();
    }
}
