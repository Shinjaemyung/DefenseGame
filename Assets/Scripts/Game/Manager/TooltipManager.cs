using TMPro;
using UnityEngine;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance { get; private set; }

    [SerializeField] RectTransform tooltip;
    [SerializeField] TMP_Text tooltipText;
    [SerializeField] Canvas canvas;
    [SerializeField] CanvasGroup canvasGroup;

    void Awake()
    {
        Instance = this;
        Hide();
    }

    void Update()
    {
        if (canvasGroup.alpha <= 0f)
            return;

        UpdatePosition();
    }

    public void Show(string text)
    {
        tooltipText.text = text;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = false;

        UpdatePosition();
    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    void UpdatePosition()
    {
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            Input.mousePosition,
            cam,
            out Vector2 localPosition
        );

        tooltip.anchoredPosition = localPosition + new Vector2(15f, -15f);
    }
}