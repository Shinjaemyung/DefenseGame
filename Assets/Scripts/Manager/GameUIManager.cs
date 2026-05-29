using UnityEngine;
using UnityEngine.UI;
using static PlayerModeManager;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    UI_TowerList towerList;
    UI_ModeChangeButton modeChangeButton;

    [SerializeField, Tooltip("타워 클릭 시 표시할 정보 패널")]
    UI_TowerInfo towerInfoPanel;

    [SerializeField, Tooltip("게임 오버 시 표시할 패널")]
    GameObject gameOverPanel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        towerList = GetComponentInChildren<UI_TowerList>();
        modeChangeButton = GetComponentInChildren<UI_ModeChangeButton>();
    }

    public void SetTowerPlacementModeUI()
    {
        towerList.gameObject.SetActive(true);
        modeChangeButton.ChangeButtonText(PlayerMode.TowerPlacementMode);
    }

    public void SetHeroControlModeUI()
    {
        towerList.gameObject.SetActive(false);
        modeChangeButton.ChangeButtonText(PlayerMode.HeroControlMode);
    }

    /// <summary>타워 정보 패널 표시</summary>
    public void ShowTowerInfo(Tower tower)
    {
        if (towerInfoPanel != null)
            towerInfoPanel.Show(tower);
    }

    /// <summary>타워 정보 패널 숨김</summary>
    public void HideTowerInfo()
    {
        if (towerInfoPanel != null)
            towerInfoPanel.Hide();
    }

    /// <summary>게임 오버 패널 표시</summary>
    public void ShowGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }
}
