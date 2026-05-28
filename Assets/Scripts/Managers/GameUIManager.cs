using UnityEngine;
using UnityEngine.UI;
using static PlayerModeManager;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    UI_TowerList towerList;
    UI_ModeChangeButton modeChangeButton;

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

    /// <summary>게임 오버 패널 표시</summary>
    public void ShowGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }
}
