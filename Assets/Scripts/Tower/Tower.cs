using Core.Utilities;
using System.Collections;
using System.Collections.Generic;
using TowerDefense.Towers.Placement;
using UnityEngine;
using System.Linq;
using Core.Health;
using TowerDefense.Affectors;
using UnityEngine.UI;

public class Tower : MonoBehaviour
{
    public VirtualTower virtualTower;

    public IntVector2 dimensions;

    public TowerData towerData;
    int totalCost = 0;

    IntVector2 gridPosition;
    IPlacementArea placementArea;

    public LayerMask enemyLayerMask;

    public LayerMask Mask { get; protected set; }

    Affector[] m_Affectors;

    protected Affector[] Affectors
    {
        get
        {
            if (m_Affectors == null)
            {
                m_Affectors = GetComponentsInChildren<Affector>();
            }
            return m_Affectors;
        }
    }

    private void Awake()
    {
        Initialize(enemyLayerMask);
    }

    public virtual void Initialize(IPlacementArea placementArea, IntVector2 destination, int cost)
    {
        if (placementArea == null || destination == null)
            return;

        this.placementArea = placementArea;
        gridPosition = destination;

        transform.position = placementArea.GridToWorld(destination, dimensions);
        //transform.rotation = placementArea.transform.rotation;
        placementArea.Occupy(destination, dimensions);

        totalCost = cost;
    }
    
    public virtual void Initialize(LayerMask enemyMask)
    {
        Mask = enemyMask;

        foreach (Affector effect in Affectors)
        {
            effect.Initialize(Mask);
        }
    }
    
    public void OnClicked()
    {
        Debug.Log(gameObject.name + "의 정보 UI 띄워주기");
    }

    public void UpgradeTower(Tower upgradeTower)
    {
        if (!towerData.upgradeTowers.Contains(upgradeTower))
            return;

        Tower spawnedTower = Instantiate(upgradeTower);
        int upgradeCost = upgradeTower.towerData.cost;
        spawnedTower.Initialize(placementArea, gridPosition, totalCost + upgradeCost);
        GameManager.Instance.UpdatePlayerGold(-upgradeCost);

        Destroy(gameObject);
    }

    public void Sell()
    {
        int sellCost = Mathf.RoundToInt(totalCost * 0.75f);
        GameManager.Instance.UpdatePlayerGold(sellCost);

        Remove();
    }

    public void Remove()
    {
        placementArea.Clear(gridPosition, dimensions);
        Destroy(gameObject);
    }
}
