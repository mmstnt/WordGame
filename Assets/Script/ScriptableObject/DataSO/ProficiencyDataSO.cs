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
        allEffect = GetFullEffectDescription();
    }

    public string GetFullEffectDescription()
    {
        if (levelSettings == null || levelSettings.Count == 0) return "無效果";

        Dictionary<Attribute, int> aggregatedAttributes = new Dictionary<Attribute, int>();
        List<string> specialEffects = new List<string>();
        int allExp = 0;

        foreach (var level in levelSettings)
        {
            allExp += level.needExp;
            foreach (var effect in level.effects)
            {
                if (int.TryParse(effect.value, out int val))
                {
                    if (aggregatedAttributes.ContainsKey(effect.type))
                        aggregatedAttributes[effect.type] += val;
                    else
                        aggregatedAttributes[effect.type] = val;
                }
                else
                {
                    string desc = FormatEffectText(effect.type, effect.value);
                    if (!specialEffects.Contains(desc)) specialEffects.Add(desc);
                }
            }
        }

        string result = $"【全等級效果】\n";
        result += $"滿級經驗需求:{allExp}\n";
        foreach (var kvp in aggregatedAttributes)
        {
            result += $"{FormatEffectText(kvp.Key, kvp.Value.ToString())}\n";
        }
        foreach (var spec in specialEffects)
        {
            result += $"{spec}\n";
        }

        return result;
    }

    public string FormatEffectText(Attribute effectType, string value)
    {
        switch (effectType)
        {
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
