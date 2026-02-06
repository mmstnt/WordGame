using UnityEngine;
using static SkillDataListSO;

[CreateAssetMenu(menuName = "DataList/SkillDataListSO")]
public class SkillDataListSO : DataListSO<SkillData, SkillDataSO>
{
    [System.Serializable]
    public struct SkillData
    {
        public string id;
        public SkillDataSO data;
    }

    protected override string getID(SkillData entry) => entry.id;

    protected override SkillDataSO getValue(SkillData entry) => entry.data;
}
