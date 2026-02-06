using UnityEngine;
using static BattleDataListSO;

[CreateAssetMenu(menuName = "DataList/BattleDataListSO")]
public class BattleDataListSO : DataListSO<BattleData, BattleDataSO>
{
    [System.Serializable]
    public struct BattleData
    {
        public string id;
        public BattleDataSO data;
    }

    protected override string getID(BattleData entry) => entry.id;

    protected override BattleDataSO getValue(BattleData entry) => entry.data;
}