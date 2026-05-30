using System;
using UnityEngine;

public class Hero : MonoBehaviour
{
    public static Hero Instance { get; private set; }

    [SerializeField, Tooltip("최대 체력")]
    private float maxHealth = 100f;

    public float Health { get; private set; }
    public float MaxHealth => maxHealth;

    /// <summary>체력 변경 시 발생. (currentHealth, maxHealth)</summary>
    public event Action<float, float> OnHealthChanged;

    /// <summary>사망 시 발생</summary>
    public event Action OnDied;

    /// <summary>부활 시 발생</summary>
    public event Action OnRevived;

    public bool isDead;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        Health = maxHealth;
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void UpdateHealth(float amount)
    {
        Health = Mathf.Clamp(Health + amount, 0f, maxHealth);
        OnHealthChanged?.Invoke(Health, maxHealth);

        if (Health <= 0f)
            Die();
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        OnDied?.Invoke();
    }

    public void Revive()
    {
        if (!isDead) return;
        isDead = false;
        OnRevived?.Invoke();
    }
}
