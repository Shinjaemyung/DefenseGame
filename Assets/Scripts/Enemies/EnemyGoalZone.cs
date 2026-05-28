using UnityEngine;

/// <summary>
/// Enemy가 진입하면 플레이어 체력을 감소시키고 Enemy를 제거하는 목적지 구역.
/// </summary>
public class EnemyGoalZone : MonoBehaviour
{
    [SerializeField, Tooltip("Enemy 도달 시 감소할 플레이어 체력")]
    private int damageToPlayer = 10;

    private void OnTriggerEnter(Collider other)
    {
        var enemy = other.GetComponent<Enemy>();
        if (enemy == null) return;

        GameManager.Instance.UpdatePlayerHealth(-damageToPlayer); //enemy의 공격력만큼 데미지로 변경
        enemy.Remove();
    }
}
