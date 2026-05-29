using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 타워 클릭 시 표시되는 정보 패널.
/// 타워 이름, 설명, 썸네일, 판매/업그레이드 버튼을 관리.
/// </summary>
public class UI_TowerInfo : MonoBehaviour
{
    [SerializeField, Tooltip("타워 썸네일 이미지")]
    private Image thumbnailImage;

    [SerializeField, Tooltip("타워 이름 텍스트")]
    private Text towerNameText;

    [SerializeField, Tooltip("타워 설명 텍스트")]
    private Text descriptionText;

    [SerializeField, Tooltip("판매 버튼")]
    private Button sellButton;

    [SerializeField, Tooltip("업그레이드 버튼")]
    private Button upgradeButton;

    [SerializeField, Tooltip("판매 버튼 텍스트")]
    private Text sellButtonText;

    private Tower _currentTower;
    private RectTransform _rectTransform;
    private Canvas _parentCanvas;


    /// <summary>타워 정보를 받아 패널을 업데이트하고 표시</summary>
    public void Show(Tower tower)
    {
        _currentTower = tower;

        var data = tower.towerData;

        if (thumbnailImage != null)
            thumbnailImage.sprite = data.thumbnail;

        if (towerNameText != null)
            towerNameText.text = data.description;

        if (descriptionText != null)
            descriptionText.text = data.upgradeDescription;

        // 판매 버튼: 판매 금액 표시 (구매가의 75%)
        if (sellButtonText != null)
            sellButtonText.text = "Sell (" + Mathf.RoundToInt(tower.TotalCost * 0.75f) + "G)";

        // 업그레이드 가능 여부에 따라 버튼 활성화
        if (upgradeButton != null)
            upgradeButton.gameObject.SetActive(data.upgradeTowers != null && data.upgradeTowers.Length > 0);

        sellButton.onClick.RemoveAllListeners();
        sellButton.onClick.AddListener(OnSellClicked);

        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(OnUpgradeClicked);

        //SetPositionToTower(tower);
        gameObject.SetActive(true);
    }
    /*
    private void SetPositionToTower(Tower tower)
    {
        if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();

        Vector2 screenPos = Camera.main.WorldToScreenPoint(tower.transform.position);

        // 화면 하단 절반이면 패널이 위로, 상단 절반이면 아래로 표시
        float pivotY = screenPos.y < Screen.height * 0.5f ? 0f : 1f;
        _rectTransform.pivot = new Vector2(0f, pivotY);
        _rectTransform.anchorMin = Vector2.zero;
        _rectTransform.anchorMax = Vector2.zero;
        _rectTransform.anchoredPosition = screenPos;
    }
    */

    /// <summary>패널 숨김</summary>
    public void Hide()
    {
        _currentTower = null;
        gameObject.SetActive(false);
    }

    private void OnSellClicked()
    {
        if (_currentTower == null) return;
        _currentTower.Sell();
        Hide();
    }

    private void OnUpgradeClicked()
    {
        if (_currentTower == null) return;
        var data = _currentTower.towerData;
        if (data.upgradeTowers != null && data.upgradeTowers.Length > 0)
            _currentTower.UpgradeTower(data.upgradeTowers[0]);
        Hide();
    }
}
