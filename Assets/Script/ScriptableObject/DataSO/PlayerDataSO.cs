using Ink.Parsed;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ProficiencyDataSO;

[CreateAssetMenu(menuName = "Data/PlayerDataSO")]
public class PlayerDataSO : BaseUnitSO
{
    public string[] curMartial;
    public string[] curMagic;
    public string[] item;
    
    [Header("養成資訊")]
    public int developRound;
    public int developActionPoint;
    public List<Proficiency> proficiencyList;

    private Dictionary<Attribute, Dictionary<string, int>> attributeSourceDic = new Dictionary<Attribute, Dictionary<string, int>>();

    [System.Serializable]
    public struct Proficiency 
    {
        public int curExp;
        public string id;
    }

    public List<string> getProficiencyIDList(ProficiencyType type)
    {
        List<string> idList = proficiencyList
        .Where(l => type == DataManager.instance.proficiencyDataList.getData(l.id).type)
        .Select(l => l.id).ToList();

        return idList;
    }

    private void OnValidate()
    {
        //getAllProficiency();
    }

    public string getAttributeSource(Attribute attributeType) 
    {
        string attributeSource = "";
        switch (attributeType)
        {
            case Attribute.HP:              attributeSource += $"生命：{this.hp}\n";             break;
            case Attribute.MP:              attributeSource += $"能量：{this.mp}\n";             break;
            case Attribute.Strength:        attributeSource += $"力量：{this.strength}\n";       break;
            case Attribute.Dexterity:       attributeSource += $"敏捷：{this.dexterity}\n";      break;
            case Attribute.Constitution:    attributeSource += $"體質：{this.constitution}\n";   break;
            case Attribute.Intelligence:    attributeSource += $"智力：{this.intelligence}\n";   break;
            case Attribute.Wisdom:          attributeSource += $"感知：{this.wisdom}\n";         break;
            case Attribute.Charisma:        attributeSource += $"魅力：{this.charisma}\n";       break;
        }

        attributeSource += "－－－屬性來源－－－\n";

        foreach (var source in attributeSourceDic[attributeType]) 
        {
            attributeSource += $"來自{source.Key}：{source.Value}\n";
        }

        return attributeSource;
    }

    public void getAllProficiency() 
    {
        attributeSourceDic.Clear();
        attributeSourceDic[Attribute.HP] = new Dictionary<string, int>();
        attributeSourceDic[Attribute.MP] = new Dictionary<string, int>();
        attributeSourceDic[Attribute.Strength] = new Dictionary<string, int>();
        attributeSourceDic[Attribute.Dexterity] = new Dictionary<string, int>();
        attributeSourceDic[Attribute.Constitution] = new Dictionary<string, int>();
        attributeSourceDic[Attribute.Intelligence] = new Dictionary<string, int>();
        attributeSourceDic[Attribute.Wisdom] = new Dictionary<string, int>();
        attributeSourceDic[Attribute.Charisma] = new Dictionary<string, int>();

        this.strength = 0;
        this.dexterity = 0;
        this.constitution = 0;
        this.intelligence = 0;
        this.wisdom = 0;
        this.charisma = 0;

        foreach (Proficiency proficiency in proficiencyList) 
        {
            ProficiencyDataSO curProficiency = DataManager.instance.proficiencyDataList.getData(proficiency.id);
            int curProficiencyExp = proficiency.curExp;
            int curProficiencyLevel = 0;

            for (int i = 0; i < curProficiency.levelSettings.Count; i++) 
            {
                if (curProficiencyExp < curProficiency.levelSettings[i].needExp) 
                {
                    break;
                }
                else 
                {
                    curProficiencyExp -= curProficiency.levelSettings[i].needExp;
                }
                curProficiencyLevel = i + 1;
            }

            for(int i = 0; i < curProficiencyLevel; i++) 
            {
                foreach(ProficiencyEffectData levelEffect in curProficiency.levelSettings[i].effects)
                {
                    getProficiencyAttribute(levelEffect.type, levelEffect.value, curProficiency.proficiencyName);
                }
            }
        }
    }

    private void getProficiencyAttribute(Attribute effectType, string value, string attributeSourceName) 
    {
        switch (effectType)
        {
            case Attribute.HP:              this.hp += int.Parse(value);            break;
            case Attribute.MP:              this.mp += int.Parse(value);            break;
            case Attribute.Strength:        this.strength += int.Parse(value);      break;
            case Attribute.Dexterity:       this.dexterity += int.Parse(value);     break;
            case Attribute.Constitution:    this.constitution += int.Parse(value);  break;
            case Attribute.Intelligence:    this.intelligence += int.Parse(value);  break;
            case Attribute.Wisdom:          this.wisdom += int.Parse(value);        break;
            case Attribute.Charisma:        this.charisma += int.Parse(value);      break;
        }

        if (attributeSourceDic[effectType].ContainsKey(attributeSourceName)) 
        {
            attributeSourceDic[effectType][attributeSourceName] += int.Parse(value);
        }
        else
        {
            attributeSourceDic[effectType][attributeSourceName] = int.Parse(value);
        }
    }
}
