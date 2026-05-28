using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_PlayerHealth : MonoBehaviour
{
    TextMeshProUGUI healthText;

    private void Awake()
    {
        healthText = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Start()
    {
        GameManager.Instance.OnPlayerHealthChanged += SetHealthDisplay;
        SetHealthDisplay(GameManager.Instance.PlayerHealth);
    }

    void SetHealthDisplay(int playerHealth)
    {
        healthText.text = playerHealth.ToString();
    }
}
