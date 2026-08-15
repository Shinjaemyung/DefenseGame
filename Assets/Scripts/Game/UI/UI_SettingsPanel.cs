using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// ESC로 열리는 설정(일시정지) 패널.
/// </summary>
public class UI_SettingsPanel : UI_Panel
{
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button howToPlayButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private UI_Panel howToPlayPanel;

    private void Awake()
    {
        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeClicked);

        if (howToPlayButton != null)
            howToPlayButton.onClick.AddListener(OnClickHowToPlayClick);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    private void OnResumeClicked()
    {
        GamePlayManager.Instance.ResumeGame();
    }

    private void OnClickHowToPlayClick()
    {
        howToPlayPanel.Show();
    }

    private void OnMainMenuClicked()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    public override void Show()
    {
        howToPlayPanel.Hide();
        base.Show();
        transform.SetAsLastSibling();
    }

    private void OnDestroy()
    {
        if (resumeButton != null)
            resumeButton.onClick.RemoveListener(OnResumeClicked);
    }
}
