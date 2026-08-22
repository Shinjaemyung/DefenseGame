using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_PlayerInfoPanel : UI_Panel
{
    [SerializeField]
    TextMeshProUGUI text_Health;

    [SerializeField]
    TextMeshProUGUI text_Gold;

    private void Start()
    {
        GamePlayManager.Instance.PlayerHealthChanged += SetHealthDisplay;
        SetHealthDisplay(GamePlayManager.Instance.PlayerHealth);

        GamePlayManager.Instance.PlayerGoldChanged += SetGoldDisplay;
        SetGoldDisplay(GamePlayManager.Instance.PlayerGold);
    }

    void SetHealthDisplay(int playerHealth)
    {
        text_Health.text = playerHealth.ToString();
    }

    void SetGoldDisplay(int playerGold)
    {
        text_Gold.text = playerGold.ToString();
    }

    private void OnDestroy()
    {
        GamePlayManager.Instance.PlayerHealthChanged -= SetHealthDisplay;
        GamePlayManager.Instance.PlayerGoldChanged -= SetGoldDisplay;
    }
}
