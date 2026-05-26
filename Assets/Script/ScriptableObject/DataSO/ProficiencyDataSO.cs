using System.Collections.Generic;
using UnityEngine;
using static ProficiencyDataSO;

[CreateAssetMenu(menuName = "Data/ProficiencyDataSO")]
public class ProficiencyDataSO : ScriptableObject
{
    [Header("能力描述")]
    public string proficiencyName; 
    [TextArea(5, 10)]
    public string description;
    public string allNeedExp;
    [TextArea(3, 10)]
    public string allEffect;

    [Header("能力參數")]
    public Sprite image;
    public ProficiencyType type;
    public List<LevelEntry> levelSettings = new List<LevelEntry>();

    [System.Serializable]
    public struct LevelEntry
    {
        public int needExp;
        public List<ProficiencyEffectData> effects;
    }

    [System.Serializable]
    public struct ProficiencyEffectData
    {
        public Attribute type;
        public string value;
    }

    private void OnValidate()
    {
        allEffect = getFullEffectDescription(0,999);
        allNeedExp = getFullEffectExp(0, 999, 0);
    }

    public float getNeedExp(int getMinLevel, int getMaxLevel) 
    {
        if (levelSettings == null || levelSettings.Count == 0) return 0f;
        if (getMaxLevel > levelSettings.Count) return 1f;

        float allExp = 0;
        for (int i = Mathf.Max(getMinLevel - 1, 0); i < Mathf.Min(getMaxLevel, levelSettings.Count); i++)
        {
            allExp += levelSettings[i].needExp;
        }

        return allExp;
    }

    public string getFullEffectExp(int getMinLevel, int getMaxLevel, int curExp)
    {
        if (levelSettings == null || levelSettings.Count == 0) return "無等級";
        if (getMaxLevel > levelSettings.Count) return "已滿級";

        int allExp = 0;
        for (int i = Mathf.Max(getMinLevel - 1, 0); i < Mathf.Min(getMaxLevel, levelSettings.Count); i++)
        {
            allExp += levelSettings[i].needExp;
        }

        return $"{curExp} / {allExp}";
    }

    public string getFullEffectDescription(int getMinLevel,int getMaxLevel)
    {
        if (levelSettings == null || levelSettings.Count == 0 || getMaxLevel <= 0) return "無效果";
        if (getMaxLevel > levelSettings.Count) return "已滿級";

        Dictionary<Attribute, int> attributeDic = new Dictionary<Attribute, int>();
        List<string> otherEffectList = new List<string>();

        //取得所有等級效果
        for (int i = Mathf.Max(getMinLevel - 1, 0); i < Mathf.Min(getMaxLevel, levelSettings.Count); i++) 
        {
            foreach (ProficiencyEffectData effect in levelSettings[i].effects)
            {
                if (int.TryParse(effect.value, out int value))
                {
                    if (attributeDic.ContainsKey(effect.type))
                        attributeDic[effect.type] += value;
                    else
                        attributeDic[effect.type] = value;
                }
                else
                {
                    string desc = formatEffectText(effect.type, effect.value);
                    if (!otherEffectList.Contains(desc))
                        otherEffectList.Add(desc);
                }
            }
        }

        string result = "";
        foreach (var attribute in attributeDic)
        {
            result += $"{formatEffectText(attribute.Key, attribute.Value.ToString())}\n";
        }
        foreach (var otherEffect in otherEffectList)
        {
            result += $"{otherEffect}\n";
        }

        return result;
    }

    public string formatEffectText(Attribute effectType, string value)
    {
        switch (effectType)
        {
            case Attribute.HP: return $"生命+{value}";
            case Attribute.MP: return $"能量+{value}";
            case Attribute.Strength: return $"力量+{value}";
            case Attribute.Dexterity: return $"敏捷+{value}";
            case Attribute.Constitution: return $"體質+{value}";
            case Attribute.Intelligence: return $"智力+{value}";
            case Attribute.Wisdom: return $"感知+{value}";
            case Attribute.Charisma: return $"魅力+{value}";
            default: return $"{effectType}: {value}";
        }
    }


}
