using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;
using static PlayerModeManager;

public class MouseManager : MonoBehaviour
{
    public static MouseManager Instance { get; private set; }

    [SerializeField, Tooltip("기본 커서 텍스처")]
    private Texture2D defaultCursor;

    [SerializeField, Tooltip("커서 핫스팟 (클릭 기준점)")]
    private Vector2 hotspot = Vector2.zero;

    float mousePosX;
    float mousePosY;

    float moveScreenEdgeThickness = 10f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        ApplyCursor(defaultCursor);
    }

    private void Update()
    {
        mousePosX = Input.mousePosition.x;
        mousePosY = Input.mousePosition.y;

        PlayerMode currentPlayerMode = PlayerModeManager.Instance.playerMode;
        if (currentPlayerMode == PlayerMode.TowerPlacementMode)
        {
            CameraMove();
            CameraZoom();
        }
    }

    void CameraMove()
    {
        Vector3 direction = Vector3.zero;

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        if (mousePosX <= moveScreenEdgeThickness)
        {
            direction += Vector3.left;
        }

        if (mousePosX >= screenWidth - moveScreenEdgeThickness)
        {
            direction += Vector3.right;
        }

        if (mousePosY <= moveScreenEdgeThickness)
        {
            direction += Vector3.back;
        }

        if (mousePosY >= screenHeight - moveScreenEdgeThickness)
        {
            direction += Vector3.forward;
        }

        CameraController.Instance.Move(direction);
    }

    void CameraZoom()
    {
        float scroollWheel = Input.GetAxis("Mouse ScrollWheel");

        if (scroollWheel == 0)
            return;

        CameraController.Instance.Zoom(scroollWheel);
    }

    /// <summary>커서 락 상태 변경</summary>
    public void SetCursorLockState(bool newState)
    {
        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
    }

    /// <summary>커서 텍스처 변경</summary>
    public void ApplyCursor(Texture2D cursor)
    {
        Cursor.SetCursor(cursor, hotspot, CursorMode.Auto);
    }
}
