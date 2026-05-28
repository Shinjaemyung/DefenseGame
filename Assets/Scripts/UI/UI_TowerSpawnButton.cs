using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UI_SystemMessage;

public class UI_TowerSpawnButton : MonoBehaviour, IPointerDownHandler
{
    [SerializeField]
    Tower spawnTower;

    UI_SystemMessage systemMessage;

    public event Action<Tower> OnButtonClicked;

    private void Awake()
    {
        systemMessage = FindAnyObjectByType<UI_SystemMessage>();
    }

    public void Initialize(Tower tower)
    {
        spawnTower = tower;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnClickButton();
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
