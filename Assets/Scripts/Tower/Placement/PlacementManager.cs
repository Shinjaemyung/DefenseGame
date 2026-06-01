using Core.Utilities;
using TowerDefense.Towers.Placement;
using UnityEngine;
using UnityEngine.EventSystems;


public class PlacementManager : MonoBehaviour
{
    public static PlacementManager Instance { get; private set; }

    TowerPlacementGrid[] grids;
    VirtualTower grabedVirtualTower;

    TowerPlacementGrid currentVirtualArea;
    IntVector2 currentVirtualGridPos;

    [SerializeField]
    Material invalidPlacementMaterial;

    UI_TowerSpawnButton[] towerSpawnButtons;

    LayerMask placementAreaMask;
    LayerMask gridMask;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        grids = FindObjectsByType<TowerPlacementGrid>(FindObjectsSortMode.None);

        towerSpawnButtons = FindObjectsByType<UI_TowerSpawnButton>(FindObjectsSortMode.None);

        placementAreaMask = LayerMask.GetMask("Grid", "Ground");
        gridMask = LayerMask.GetMask("Grid");

        DeactivateAllGrids();
    }

    private void Start()
    {
        foreach (UI_TowerSpawnButton button in towerSpawnButtons)
        {
            button.OnButtonClicked += ActivateAllGrids;
            button.OnButtonClicked += SpawnVirtualTower;
        }

        UserInputManager.Instance.OnLeftMouseReleased += TryTowerBuild;
        UserInputManager.Instance.OnLeftMouseReleased += DeactivateAllGrids;
        UserInputManager.Instance.OnRightMouseReleased += RemoveVirtualTower;
    }

    private void Update()
    {
        SetVirtualTowerPosition();
    }

    void SetVirtualTowerPosition()
    {
        if (grabedVirtualTower == null)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitArea;
        RaycastHit hitGrid;

        if (!Physics.Raycast(ray, out hitArea, float.MaxValue, placementAreaMask))
            return;

        if (Physics.Raycast(ray, out hitGrid, float.MaxValue, gridMask))
        {
            TowerPlacementGrid grid = hitGrid.collider.GetComponent<TowerPlacementGrid>();

            if (grid == null)
                return;

            Tower towerToBuild = grabedVirtualTower.MyTower;

            IntVector2 gridPos = grid.WorldToGrid(hitGrid.point, towerToBuild.dimensions);
            Vector3 movePos = grid.GridToWorld(gridPos, towerToBuild.dimensions);
            TowerFitStatus fits = grid.GetFits(gridPos, towerToBuild.dimensions);
            bool placementPossible = (fits == TowerFitStatus.Fits);

            grabedVirtualTower.Move(movePos, placementPossible);

            currentVirtualArea = grid;
            currentVirtualGridPos = gridPos;
        }
        else
        {
            float height = grabedVirtualTower.MyTower.transform.position.y;
            Vector3 movePos = hitArea.point + new Vector3(0, height, 0);
            grabedVirtualTower.Move(movePos, false);
        }
    }
    void TryTowerBuild()
    {
        if (grabedVirtualTower == null)
            return;

        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (grabedVirtualTower.isPlacementValid)
        {
            Tower spawnedTower = Instantiate(grabedVirtualTower.MyTower);
            spawnedTower.Initialize(currentVirtualArea, currentVirtualGridPos, spawnedTower.towerData.cost);
            GameManager.Instance.UpdatePlayerGold(-spawnedTower.towerData.cost);
        }

        RemoveVirtualTower();
    }

    void SpawnVirtualTower(Tower tower)
    {
        grabedVirtualTower = Instantiate(tower.virtualTower);
        grabedVirtualTower.Initialize(tower, invalidPlacementMaterial);
    }

    void RemoveVirtualTower()
    {
        if (grabedVirtualTower == null)
            return;

        Destroy(grabedVirtualTower.gameObject);
        grabedVirtualTower = null;
    }

    void ActivateAllGrids(Tower tower)
    {
        foreach (TowerPlacementGrid grid in grids) 
        {
            grid.gameObject.SetActive(true);
        }
    }

    void DeactivateAllGrids()
    {
        foreach (TowerPlacementGrid grid in grids)
        {
            grid.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        foreach (UI_TowerSpawnButton button in towerSpawnButtons)
        {
            button.OnButtonClicked -= ActivateAllGrids;
            button.OnButtonClicked -= SpawnVirtualTower;
        }
    }
}
