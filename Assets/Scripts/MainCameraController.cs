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
    public CinemachineVirtualCamera virtualCamera;

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
        SetTowerPlacementModeView();
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
        Vector3 newPosition = transform.position + direction * moveSpeed * Time.deltaTime;

        newPosition.x = Mathf.Clamp(newPosition.x, cameraRangeX.x, cameraRangeX.y);
        newPosition.z = Mathf.Clamp(newPosition.z, cameraRangeZ.x, cameraRangeZ.y);

        transform.position = newPosition;
    }

    public void Zoom(float scroollWheel)
    {
        float zoomAmount = scroollWheel * zoomSpeed * Time.deltaTime;
        currentZoom -= zoomAmount;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

        mainCamera.fieldOfView = currentZoom;
    }

    public void SetTowerPlacementModeView()
    {
        cinemachineBrain.enabled = false;
        virtualCamera.gameObject.SetActive(false);

        mainCamera.transform.position = startPosition;
        mainCamera.transform.eulerAngles = startRotation;
    }

    public void SetHeroControlModeView()
    {
        cinemachineBrain.enabled = true;
        virtualCamera.gameObject.SetActive(true);
    }
}
