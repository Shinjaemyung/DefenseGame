using UnityEngine;
using UnityEngine.UI;
using static PlayerModeManager;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    UI_TowerList towerList;
    UI_TowerInfo towerInfoPanel;
    UI_ModeChangeButton modeChangeButton;
    UI_GameOverPanel gameOverPanel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        towerList = GetComponentInChildren<UI_TowerList>(true);
        towerInfoPanel = GetComponentInChildren<UI_TowerInfo>(true);
        modeChangeButton = GetComponentInChildren<UI_ModeChangeButton>(true);
        gameOverPanel = GetComponentInChildren<UI_GameOverPanel>(true);
    }

    public void SetTowerPlacementModeUI()
    {
        towerList.gameObject.SetActive(true);
        modeChangeButton.ChangeButtonText(PlayerMode.TowerPlacementMode);
    }

    public void SetHeroControlModeUI()
    {
        towerList.gameObject.SetActive(false);
        towerInfoPanel.gameObject.SetActive(false);
        modeChangeButton.ChangeButtonText(PlayerMode.HeroControlMode);
    }

    /// <summary>타워 정보 패널 표시. TowerList를 숨기고 타워 정보 패널을 표시.</summary>
    public void ShowTowerInfo(Tower tower)
    {
        Debug.Log("D");
        if (towerInfoPanel == null) return;
        towerList.gameObject.SetActive(false);
        towerInfoPanel.Show(tower);
        Debug.Log("a");
    }

    /// <summary>타워 정보 패널 숨김.</summary>
    public void ShowTowerList()
    {
        if (towerInfoPanel == null) return;
        towerInfoPanel.Hide();
        towerList.gameObject.SetActive(true);
    }

    /// <summary>게임 오버 패널 표시</summary>
    public void ShowGameOver()
    {
        if (gameOverPanel == null) return;
        gameOverPanel.transform.SetAsLastSibling();
        gameOverPanel.gameObject.SetActive(true);
    }
}
