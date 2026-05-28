using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static PlayerModeManager;

public class UI_ModeChangeButton : MonoBehaviour
{
    Button m_Button;
    TextMeshProUGUI buttonText;

    public string heroControlModeText;
    public string towerPlacementModeText;

    private void Awake()
    {
        m_Button = GetComponent<Button>();
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
        m_Button.onClick.AddListener(OnClickButton);
    }

    void OnClickButton()
    {
        PlayerModeManager.Instance.RequestPlayerModeChange();
    }

    public void ChangeButtonText(PlayerMode mode)
    {
        switch (mode)
        {
            case PlayerMode.TowerPlacementMode:
                buttonText.text = heroControlModeText;
                break;
            case PlayerMode.HeroControlMode:
                buttonText.text = towerPlacementModeText;
                break;
        }
    }

    public void SetButtonInteractable(bool isInteractable)
    {
        m_Button.interactable = isInteractable;
    }
}
