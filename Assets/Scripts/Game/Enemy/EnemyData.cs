using System.Collections.Generic;
using UnityEngine;
using static Core.Health.Damageable;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Tooltip("이 Enemy를 스폰할 때 사용할 프리팹 (WaveData/SpawnInfo에서 EnemyData만으로 스폰 가능하도록 함)")]
    public GameObject enemyPrefab;

    [Tooltip("이 Enemy의 이름")]
    public string enemyName;

    [Tooltip("이 Enemy의 최대 체력")]
    public float maxHealth;

    [Tooltip("Hero와 충돌 시 주는 데미지")]
    public float attackDamage;

    [Tooltip("사망 시 지급하는 골드")]
    public int goldReward;

    [Tooltip("사망 시 주는 점수")]
    public int scoreReward;

    [Tooltip("타입별 적에게 받는 데미지 비율")]
    public List<TypeCalculation> typeCalculations;
}
