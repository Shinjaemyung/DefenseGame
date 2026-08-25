using System;
using UnityEngine;
using UnityEngine.UI;
using static PlayerModeManager;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    [SerializeField]
    Canvas rootCanvas;

    UI_TowerListPanel towerListPanel;
    UI_TowerInfoPanel towerInfoPanel;
    UI_EnemyInfoPanel enemyInfoPanel;
    UI_ModeChangeButton modeChangeButton;
    UI_GameOverPanel gameOverPanel;
    UI_HeroInfoPanel heroInfoPanel;
    UI_SettingsPanel settingsPanel;
    UI_PlayerScorePanel playerScorePanel;
    UI_ModeChangeEffectPanel modeChangeEffectPanel;

    public event Action OnBeginTowerPlacementMode;
    public event Action OnCompleteTowerPlacementMode;
    public event Action OnBeginHeroControlMode;
    public event Action OnCompleteHeroControlMode;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        towerListPanel = rootCanvas.GetComponentInChildren<UI_TowerListPanel>(true);
        towerInfoPanel = rootCanvas.GetComponentInChildren<UI_TowerInfoPanel>(true);
        enemyInfoPanel = rootCanvas.GetComponentInChildren<UI_EnemyInfoPanel>(true);
        modeChangeButton = rootCanvas.GetComponentInChildren<UI_ModeChangeButton>(true);
        gameOverPanel = rootCanvas.GetComponentInChildren<UI_GameOverPanel>(true);
        heroInfoPanel = rootCanvas.GetComponentInChildren<UI_HeroInfoPanel>(true);
        settingsPanel = rootCanvas.GetComponentInChildren<UI_SettingsPanel>(true);
        playerScorePanel = rootCanvas.GetComponentInChildren<UI_PlayerScorePanel>(true);
        modeChangeEffectPanel = rootCanvas.GetComponentInChildren<UI_ModeChangeEffectPanel>(true);

        ShowTowerList();
    }

    private void Start()
    {
        if (Hero.Instance != null)
        {
            Hero.Instance.OnDied += OnHeroDied;
            Hero.Instance.OnRevived += OnHeroRevived;
        }

        modeChangeEffectPanel.Initialize();
    }

    private void OnHeroDied()
    {
        modeChangeButton.SetButtonInteractable(false);
    }

    private void OnHeroRevived()
    {
        modeChangeButton.SetButtonInteractable(true);
    }

    public void BeginTowerPlacementMode()
    {
        modeChangeButton.SetButtonInteractable(false);
        OnBeginTowerPlacementMode?.Invoke();
    }

    public void CompleteTowerPlacementMode()
    {
        towerListPanel.Show();
        modeChangeButton.ChangeButtonText(PlayerMode.TowerPlacementMode);
        modeChangeButton.SetButtonInteractable(true);
        OnCompleteTowerPlacementMode?.Invoke();
    }

    public void BeginHeroControlMode()
    {
        modeChangeButton.SetButtonInteractable(false);
        towerListPanel.Hide();
        towerInfoPanel.Hide();
        enemyInfoPanel.Hide();
        OnBeginHeroControlMode?.Invoke();
    }

    public void CompleteHeroControlMode()
    {
        modeChangeButton.ChangeButtonText(PlayerMode.HeroControlMode);
        modeChangeButton.SetButtonInteractable(true);
        OnCompleteHeroControlMode?.Invoke();
    }

    /// <summary>타워 리스트 패널 표시. 다른 패널은 비활성화.</summary>
    public void ShowTowerList()
    {
        if (towerInfoPanel == null) return;
        towerInfoPanel.Hide();
        enemyInfoPanel.Hide();
        towerListPanel.Show();
    }

    /// <summary>타워 정보 패널 표시. 다른 패널은 비활성화.</summary>
    public void ShowTowerInfo(Tower tower)
    {
        if (towerInfoPanel == null) return;
        towerListPanel.Hide();
        enemyInfoPanel.Hide();
        towerInfoPanel.ShowTowerInfo(tower);
    }

    /// <summary>적 정보 패널 표시. 다른 패널은 비활성화.</summary>
    public void ShowEnemyInfo(Enemy enemy)
    {
        if (enemyInfoPanel == null) return;
        towerListPanel.Hide();
        towerInfoPanel.Hide();
        enemyInfoPanel.ShowEnemyInfo(enemy);
    }

    public void ShowSettings()
    {
        if (settingsPanel == null) return;
        settingsPanel.transform.SetAsLastSibling();
        settingsPanel.Show();
    }

    public void HideSettings()
    {
        if (settingsPanel == null) return;
        settingsPanel.Hide();
    }

    public void ShowScorePanel()
    {
        if (playerScorePanel == null) return;
        playerScorePanel.Show();
    }

    /// <summary>게임 오버 패널 표시</summary>
    public void ShowGameOver()
    {
        if (gameOverPanel == null) return;
        gameOverPanel.Show();
    }

    private void OnDestroy()
    {
        if (Hero.Instance != null)
        {
            Hero.Instance.OnDied -= OnHeroDied;
            Hero.Instance.OnRevived -= OnHeroRevived;
        }
    }
}
