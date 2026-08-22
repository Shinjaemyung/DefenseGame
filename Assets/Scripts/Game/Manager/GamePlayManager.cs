using Core.Health;
using System;
using System.Collections;
using UnityEngine;
using static PlayerModeManager;

public class GamePlayManager : MonoBehaviour
{
    public static GamePlayManager Instance { get; private set; }

    [SerializeField, Tooltip("게임 시작 시 플레이어 체력")]
    int initialPlayerHealth = 100;
    [SerializeField, Tooltip("게임 시작 시 플레이어 골드")]
    int initialPlayerGold = 500;
    [SerializeField, Tooltip("적이 하나 죽을 때마다 늘어나는 스코어의 양")]
    int increasingScoreAmount = 10;

    public int PlayerHealth { get; private set; }
    public int PlayerGold { get; private set; }
    public int PlayerScore { get; private set; }

    [SerializeField, Tooltip("Hero 사망 시 체력 회복 속도")]
    float heroHealthRegenRate = 1;

    public event Action<int> PlayerHealthChanged;
    public event Action<int> PlayerGoldChanged;
    public event Action<int> PlayerScoreChanged;

    public bool IsPaused { get; private set; }
    public bool IsGameOvered { get; private set; }

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
        PlayerHealthChanged?.Invoke(PlayerHealth);

        if (PlayerHealth <= 0)
            SetGameOverState();
    }

    public void UpdatePlayerGold(int amount)
    {
        PlayerGold += amount;
        PlayerGold = Mathf.Max(PlayerGold, 0);
        PlayerGoldChanged?.Invoke(PlayerGold);
    }

    public void UpdatePlayerScore(int amount)
    {
        PlayerScore += amount;
        PlayerScore = Mathf.Max(PlayerScore, 0);
        PlayerScoreChanged?.Invoke(PlayerScore);
    }

    public void OnEnemyDied(DamageableBehaviour damageable)
    {
        Enemy enemy = damageable.GetComponent<Enemy>();

        if (enemy == null)
            return;

        UpdatePlayerGold(enemy._enemyData.goldReward);
        UpdatePlayerScore(increasingScoreAmount);

        enemy.Died -= OnEnemyDied;
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


    /// <summary>게임 일시 정지하고 설정 패널 표시</summary>
    public void PauseGame()
    {
        GameManager.Instance.PauseGame();
        MouseManager.Instance.SetCursorLockState(false);
        GameUIManager.Instance.ShowSettings();
        IsPaused = true;
    }

    /// <summary>게임 재생하고 설정 패널 숨김</summary>
    public void ResumeGame()
    {
        GameManager.Instance.ResumeGame();

        if (PlayerModeManager.Instance.playerMode == PlayerMode.HeroControlMode)
            MouseManager.Instance.SetCursorLockState(true);

        GameUIManager.Instance.HideSettings();
        IsPaused = false;
    }

    void SetGameOverState()
    {
        GameManager.Instance.PauseGame();
        GameUIManager.Instance.ShowGameOver();
        MouseManager.Instance.SetCursorLockState(false);
        IsGameOvered = true;
    }

    private void OnDestroy()
    {
        if (Hero.Instance != null)
            Hero.Instance.OnDied -= OnHeroDied;
    }
}
