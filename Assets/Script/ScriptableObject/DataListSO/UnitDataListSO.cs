using UnityEngine;
using static UnitDataListSO;

[CreateAssetMenu(menuName = "DataList/UnitDataListSO")]
public class UnitDataListSO : DataListSO<UnitData, UnitDataSO>
{
    [System.Serializable]
    public struct UnitData 
    {
        public string id;
        public UnitDataSO data;
    }

    protected override string getID(UnitData entry) => entry.id;

    protected override UnitDataSO getValue(UnitData entry) => entry.data;
}
