using UnityEngine;
using static ProficiencyDataListSO;

[CreateAssetMenu(menuName = "DataList/ProficiencyDataListSO")]
public class ProficiencyDataListSO : DataListSO<ProficiencyData, ProficiencyDataSO>
{
    [System.Serializable]
    public struct ProficiencyData
    {
        public string id;
        public ProficiencyDataSO data;
    }

    protected override string getID(ProficiencyData entry) => entry.id;

    protected override ProficiencyDataSO getValue(ProficiencyData entry) => entry.data;
}
