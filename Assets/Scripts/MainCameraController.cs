using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraController : MonoBehaviour
{
    public static MainCameraController Instance { get; private set; }

    Camera mainCamera;

    [SerializeField] Vector3 startPosition;
    [SerializeField] Vector3 startRotation;

    [SerializeField] float moveSpeed = 20f;
    [SerializeField] Vector2 cameraRangeX = new Vector2(-50f, 50f);
    [SerializeField] Vector2 cameraRangeZ = new Vector2(-50f, 50f);

    float currentZoom;

    [SerializeField] float minZoom = 20f;
    [SerializeField] float maxZoom = 80f;
    [SerializeField] float zoomSpeed = 4000f;

    CinemachineBrain cinemachineBrain;
    [SerializeField, Tooltip("Hero 모드 가상 카메라")]
    CinemachineVirtualCamera heroFollowCamera;

    [SerializeField, Tooltip("타워 배치 모드 가상 카메라")]
    CinemachineVirtualCamera towerPlacementCamera;


    /// <summary>카메라 블렌드 중인지 여부 반환</summary>
    public bool IsBlending => cinemachineBrain.IsBlending;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        mainCamera = GetComponent<Camera>();
        cinemachineBrain = GetComponent<CinemachineBrain>();

        currentZoom = mainCamera.fieldOfView;
    }

    private void Start()
    {
        mainCamera.transform.position = startPosition;
        mainCamera.transform.eulerAngles = startRotation;
    }

    public void Move(Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            MoveCamera(direction.normalized);
        }
    }

    void MoveCamera(Vector3 direction)
    {
        Vector3 newPosition = towerPlacementCamera.transform.position + direction * moveSpeed * Time.deltaTime;
        newPosition.x = Mathf.Clamp(newPosition.x, cameraRangeX.x, cameraRangeX.y);
        newPosition.z = Mathf.Clamp(newPosition.z, cameraRangeZ.x, cameraRangeZ.y);

        towerPlacementCamera.transform.position = newPosition;
    }

    public void Zoom(float scroollWheel)
    {
        float zoomAmount = scroollWheel * zoomSpeed * Time.deltaTime;
        currentZoom -= zoomAmount;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

        towerPlacementCamera.m_Lens.FieldOfView = currentZoom;
    }

    public void SetTowerPlacementModeView()
    {
        towerPlacementCamera.Priority = 20;
        heroFollowCamera.Priority = 10;
    }

    public void SetHeroControlModeView()
    {
        heroFollowCamera.Priority = 20;
        towerPlacementCamera.Priority = 10;
    }
}
