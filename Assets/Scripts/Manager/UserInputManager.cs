using System;
using UnityEngine;
using UnityEngine.EventSystems;
using static PlayerModeManager;

public class UserInputManager : MonoBehaviour
{
    public static UserInputManager Instance { get; private set; }

    public LayerMask towerLayer;

    public event Action OnLeftMouseReleased;
    public event Action OnRightMouseReleased;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {

        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            PlayerModeManager.Instance.RequestPlayerModeChange();
        }

        PlayerMode currentPlayerMode = PlayerModeManager.Instance.playerMode;
        if (currentPlayerMode == PlayerMode.TowerPlacementMode)
        {
            CheckMouseInput();
        }
    }

    void CheckMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            OnLeftButtonDown();
        }

        if (Input.GetMouseButtonUp(0))
        {
            OnLeftMouseUp();
        }
    }

    void OnLeftButtonDown()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, towerLayer))
        {
            var clickedTower = hit.collider.GetComponent<Tower>();
            if (clickedTower != null)
            {
                clickedTower.OnClicked();
            }
        }
        else
        {
            // 타워 info 비활성화
            GameUIManager.Instance.ShowTowerList();
        }
    }

    void OnLeftMouseUp()
    {
        OnLeftMouseReleased?.Invoke();
    }
}
