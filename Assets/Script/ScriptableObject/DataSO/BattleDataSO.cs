using UnityEngine;

[CreateAssetMenu(menuName = "Data/BattleDataSO")]
public class BattleDataSO : ScriptableObject
{
    public BattleMapDataSO mapData;
    public string[] enemyUnit;
    public string ourUnit;

    private void OnValidate()
    {
        if (mapData != null && enemyUnit.Length != mapData.unitSite.Length)
        {
            System.Array.Resize(ref enemyUnit, mapData.unitSite.Length);
        }
    }
}
