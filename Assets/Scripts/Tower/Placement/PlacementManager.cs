using Core.Utilities;
using TowerDefense.Towers.Placement;
using UnityEngine;


public class PlacementManager : MonoBehaviour
{
    TowerPlacementGrid[] grids;
    VirtualTower grabedVirtualTower;

    TowerPlacementGrid currentVirtualArea;
    IntVector2 currentVirtualGridPos;

    [SerializeField]
    Material invalidPlacementMaterial;

    LayerMask placementAreaMask;
    LayerMask gridMask;

    private void Awake()
    {
        grids = FindObjectsByType<TowerPlacementGrid>(FindObjectsSortMode.None);

        UserInputManager userInputManager = FindAnyObjectByType<UserInputManager>();

        userInputManager.OnLeftMouseReleased += TryTowerBuild;
        userInputManager.OnLeftMouseReleased += DeactivateAllGrids;

        UI_TowerSpawnButton[] towerSpawnButtons = FindObjectsByType<UI_TowerSpawnButton>(FindObjectsSortMode.None);
        foreach (UI_TowerSpawnButton button in towerSpawnButtons)
        {
            button.OnButtonClicked += ActivateAllGrids;
            button.OnButtonClicked += SpawnVirtualTower;
        }

        placementAreaMask = LayerMask.GetMask("Grid", "Ground");
        gridMask = LayerMask.GetMask("Grid");

        DeactivateAllGrids();
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
        Destroy(grabedVirtualTower.gameObject);
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
}
