using UnityEngine;
using System;
using System.Collections;

public class UI_SystemMessage : MonoBehaviour
{
    public enum SystemMessageType
    {
        NotEnoughGold,
    }

    [SerializeField]
    float messageActivateTime;

    [SerializeField]
    GameObject message_NotEnoughGold;

    GameObject currentActiveMessage;

    private void Awake()
    {
        message_NotEnoughGold.SetActive(false);
    }

    public void ActivateMessage(SystemMessageType type)
    {
        if (currentActiveMessage != null) 
        {
            currentActiveMessage.SetActive(false);
        }

        switch (type)
        {
            case SystemMessageType.NotEnoughGold:
                message_NotEnoughGold.SetActive(true);
                currentActiveMessage = message_NotEnoughGold;
                break;
        }

        StopAllCoroutines();
        StartCoroutine(WaitForMessageTime());
    }

    IEnumerator WaitForMessageTime()
    {
        yield return new WaitForSeconds(messageActivateTime);
        currentActiveMessage.SetActive(false);
        currentActiveMessage = null;
    }

}
