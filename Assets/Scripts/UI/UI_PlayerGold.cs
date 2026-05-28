using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_PlayerGold : MonoBehaviour
{
    TextMeshProUGUI goldText;

    private void Awake()
    {
        goldText = GetComponentInChildren<TextMeshProUGUI>(); 
    }

    private void Start()
    {
        GameManager.Instance.OnPlayerGoldChanged += SetGoldDisplay;
        SetGoldDisplay(GameManager.Instance.PlayerGold);
    }

    void SetGoldDisplay(int playerGold)
    {
        goldText.text = playerGold.ToString();
    }
}
