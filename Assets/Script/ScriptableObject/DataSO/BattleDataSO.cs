using UnityEngine;

[CreateAssetMenu(menuName = "Data/BattleDataSO")]
public class BattleDataSO : ScriptableObject
{
    public BattleMapDataSO mapData;
    public string[] enemyUnit = new string[10];
    public string ourUnit;

    private void OnValidate()
    {
        if (mapData != null && enemyUnit.Length != mapData.unitSite.Count)
        {
            System.Array.Resize(ref enemyUnit, mapData.unitSite.Count);
        }
    }
}
