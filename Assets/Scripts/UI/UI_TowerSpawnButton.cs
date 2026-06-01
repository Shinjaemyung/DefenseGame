using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UI_SystemMessagePanel;

public class UI_TowerSpawnButton : MonoBehaviour
{
    [SerializeField]
    Tower spawnTower;

    Button _button;
    UI_SystemMessagePanel systemMessage;

    public event Action<Tower> OnButtonClicked;

    private void Awake()
    {
        _button = GetComponent<Button>();
        systemMessage = FindAnyObjectByType<UI_SystemMessagePanel>();
    }

    private void Start()
    {
        _button.onClick.AddListener(OnClickButton);
    }

    public void Initialize(Tower tower)
    {
        spawnTower = tower;
    }

    void OnClickButton()
    {
        if (GameManager.Instance.PlayerGold >= spawnTower.towerData.cost) 
        {
            OnButtonClicked?.Invoke(spawnTower);
        }
        else
        {
            systemMessage.ActivateMessage(SystemMessageType.NotEnoughGold);
        }
    }
}
