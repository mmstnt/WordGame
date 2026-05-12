using UnityEngine;
using static DevelopEventDataListSO;

[CreateAssetMenu(menuName = "DataList/DevelopEventDataListSO")]
public class DevelopEventDataListSO : DataListSO<DevelopEventData, DevelopEventDataSO>
{
    [System.Serializable]
    public struct DevelopEventData
    {
        public string id;
        public DevelopEventDataSO data;
    }

    protected override string getID(DevelopEventData entry) => entry.id;

    protected override DevelopEventDataSO getValue(DevelopEventData entry) => entry.data;
}
