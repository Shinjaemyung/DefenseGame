using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_NumberTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    TextMeshProUGUI tooltipText;
    bool _isHovering;

    private void Awake()
    {
        tooltipText = GetComponent<TextMeshProUGUI>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovering = true;
        Show();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovering = false;
        TooltipManager.Instance?.Hide();
    }


    void Update()
    {
        if (!_isHovering)
            return;

        Show();
    }

    void Show()
    {
        if (tooltipText == null || TooltipManager.Instance == null)
            return;

        TooltipManager.Instance.Show(tooltipText.text);
    }

    void OnDisable()
    {
        if (!_isHovering)
            return;

        _isHovering = false;
        TooltipManager.Instance?.Hide();
    }

}