using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static PlayerModeManager;

public class UI_ModeChangeButton : MonoBehaviour
{
    Button _Button;
    TextMeshProUGUI buttonText;

    public string heroControlModeText;
    public string towerPlacementModeText;

    private void Awake()
    {
        _Button = GetComponent<Button>();
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
        _Button.onClick.AddListener(OnClickButton);
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
        _Button.interactable = isInteractable;
    }
}
