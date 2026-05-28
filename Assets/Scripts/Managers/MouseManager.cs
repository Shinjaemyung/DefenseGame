using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;
using static PlayerModeManager;

public class MouseManager : MonoBehaviour
{
    public static MouseManager Instance { get; private set; }

    float mousePosX;
    float mousePosY;

    float moveScreenEdgeThickness = 10f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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

        MainCameraController.Instance.Move(direction);
    }

    void CameraZoom()
    {
        float scroollWheel = Input.GetAxis("Mouse ScrollWheel");

        if (scroollWheel == 0)
            return;

        MainCameraController.Instance.Zoom(scroollWheel);
    }

    public void SetCursorLockState(bool newState)
    {
        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
    }
}
