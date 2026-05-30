using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField, Tooltip("게임 시작 시 플레이어 체력")]
    int initialPlayerHealth = 100;
    [SerializeField, Tooltip("게임 시작 시 플레이어 골드")]
    int initialPlayerGold = 500;

    public int PlayerHealth { get; private set; }
    public int PlayerGold { get; private set; }

    [SerializeField, Tooltip("Hero 사망 시 체력 회복 속도")]
    float heroHealthRegenRate = 1;

    public event Action<int> OnPlayerHealthChanged;
    public event Action<int> OnPlayerGoldChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        PlayerHealth = initialPlayerHealth;
        PlayerGold = initialPlayerGold;
    }
    private void Start()
    {
        if (Hero.Instance != null)
            Hero.Instance.OnDied += OnHeroDied;
    }

    public void UpdatePlayerHealth(int amount)
    {
        PlayerHealth += amount;
        PlayerHealth = Mathf.Max(PlayerHealth, 0);
        OnPlayerHealthChanged?.Invoke(PlayerHealth);

        if (PlayerHealth <= 0)
            SetGameOverState();
    }

    public void UpdatePlayerGold(int amount)
    {
        PlayerGold += amount;
        PlayerGold = Mathf.Max(PlayerGold, 0);
        OnPlayerGoldChanged?.Invoke(PlayerGold);
    }

    void SetGameOverState()
    {
        Time.timeScale = 0f;
        GameUIManager.Instance.ShowGameOver();
    }

    private void OnHeroDied()
    {
        StartCoroutine(HeroRecoveryCoroutine());
    }

    private IEnumerator HeroRecoveryCoroutine()
    {
        var hero = Hero.Instance;
        while (hero.isDead)
        {
            yield return new WaitForSeconds(1f);
            hero.UpdateHealth(heroHealthRegenRate);

            if (hero.Health >= hero.MaxHealth)
                hero.Revive();
        }
    }

    private void OnDestroy()
    {
        if (Hero.Instance != null)
            Hero.Instance.OnDied -= OnHeroDied;
    }
}
