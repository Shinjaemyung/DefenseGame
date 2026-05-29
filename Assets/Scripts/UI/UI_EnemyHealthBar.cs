using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 적이 데미지를 받았을 때 체력바를 표시하는 컴포넌트.
/// Resources/EnemyHealthBarUI 프리팹을 런타임에 자식으로 자동 생성.
/// </summary>
public class UI_EnemyHealthBar : MonoBehaviour
{
    private const string PrefabResourcePath = "EnemyHealthBarUI";

    [Header("Settings")]
    [Tooltip("데미지를 받은 후 체력바가 유지되는 시간(초)")]
    [SerializeField] private float displayDuration = 2.5f;
    [Tooltip("체력바 색상 - 체력이 많을 때")]
    [SerializeField] private Color healthyColor = Color.green;
    [Tooltip("체력바 색상 - 체력이 적을 때")]
    [SerializeField] private Color lowHealthColor = Color.red;
    [Tooltip("체력이 낮다고 판단하는 비율 (0~1)")]
    [SerializeField] private float lowHealthThreshold = 0.3f;

    private Canvas _canvas;
    private Image _fillImage;
    private RectTransform _fillRect;
    private Coroutine _hideCoroutine;
    private Camera _mainCamera;

    void Awake()
    {
        _mainCamera = Camera.main;

        var prefab = Resources.Load<GameObject>(PrefabResourcePath);
        if (prefab == null)
        {
            Debug.LogWarning("[EnemyHealthBar] Resources/" + PrefabResourcePath + " 를 찾을 수 없습니다.");
            return;
        }

        var instance = Instantiate(prefab, transform);
        instance.transform.localPosition = prefab.transform.localPosition;
        instance.transform.localRotation = Quaternion.identity;

        _canvas = instance.GetComponent<Canvas>();

        var fillTransform = instance.transform.Find("Fill");
        if (fillTransform != null)
        {
            _fillImage = fillTransform.GetComponent<Image>();
            _fillRect  = fillTransform.GetComponent<RectTransform>();
        }

        // 시작 시 숨김
        if (_canvas != null)
            _canvas.gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (_canvas == null || !_canvas.gameObject.activeSelf) return;
        if (_mainCamera == null) _mainCamera = Camera.main;
        if (_mainCamera != null)
            _canvas.transform.LookAt(_canvas.transform.position + _mainCamera.transform.forward);
    }

    /// <summary>체력 비율(0~1)을 받아 체력바를 업데이트하고 표시합니다.</summary>
    public void ShowAndUpdate(float ratio)
    {
        if (_canvas == null) return;
        ratio = Mathf.Clamp01(ratio);

        // 앵커 방식으로 Fill 너비 조절
        if (_fillRect != null)
        {
            _fillRect.anchorMax = new Vector2(ratio, _fillRect.anchorMax.y);
            _fillRect.sizeDelta = new Vector2(0f, _fillRect.sizeDelta.y);
        }

        // 색상: lowHealthThreshold 이하면 빨간색 그라디언트, 이상이면 초록색
        if (_fillImage != null)
        {
            _fillImage.color = (ratio <= lowHealthThreshold)
                ? Color.Lerp(lowHealthColor, healthyColor, ratio / Mathf.Max(lowHealthThreshold, 0.001f))
                : healthyColor;
        }

        _canvas.gameObject.SetActive(true);

        // 기존 숨김 코루틴 취소 후 재시작
        if (_hideCoroutine != null) StopCoroutine(_hideCoroutine);
        _hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    /// <summary>즉시 체력바를 숨깁니다.</summary>
    public void Hide()
    {
        if (_hideCoroutine != null) { StopCoroutine(_hideCoroutine); _hideCoroutine = null; }
        if (_canvas != null) _canvas.gameObject.SetActive(false);
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        if (_canvas != null) _canvas.gameObject.SetActive(false);
        _hideCoroutine = null;
    }
}
