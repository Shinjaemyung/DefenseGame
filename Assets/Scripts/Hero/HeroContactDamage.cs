using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hero가 Enemy와 접촉 시 데미지를 받는 컴포넌트.
/// 같은 Enemy에 대해 damageInterval 쿨타임 적용, 다른 Enemy는 독립적으로 처리.
/// </summary>
[RequireComponent(typeof(Hero))]
public class HeroContactDamage : MonoBehaviour
{
    [SerializeField, Tooltip("같은 Enemy에게 다시 데미지를 받기까지의 간격 (초)")]
    private float damageInterval = 1f;

    private Hero _hero;

    // Enemy별 마지막 데미지 수신 시각
    private readonly Dictionary<Enemy, float> _enemyDamageCooltime = new Dictionary<Enemy, float>();

    private void Awake()
    {
        _hero = GetComponent<Hero>();
    }

    private void OnTriggerEnter(Collider other)
    {
        var enemy = other.GetComponent<Enemy>();
        if (enemy == null) return;

        // 쿨타임 체크
        if (_enemyDamageCooltime.TryGetValue(enemy, out float lastTime))
            if (Time.time - lastTime < damageInterval) return;

        _enemyDamageCooltime[enemy] = Time.time;
        _hero.UpdateHealth(-enemy.attackDamage);
    }
}
