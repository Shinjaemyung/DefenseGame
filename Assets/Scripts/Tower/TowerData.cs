using UnityEngine;

[CreateAssetMenu(fileName = "TowerData", menuName = "Scriptable Objects/TowerData")]
public class TowerData : ScriptableObject
{
    public string description;
    public string upgradeDescription;

    public int cost;
    //public int sell;

    public Sprite thumbnail;

    public Tower[] upgradeTowers;
}
