using Ink.Parsed;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
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
    public Dictionary<string, Proficiency> proficiencyIndexDic = new Dictionary<string, Proficiency>();

    private Dictionary<Attribute, Dictionary<string, int>> attributeSourceDic = new Dictionary<Attribute, Dictionary<string, int>>();

    [System.Serializable]
    public class Proficiency 
    {
        public string id;
        public int allExp;
        public int curLevel;
        public int curExp;
    }

    private void OnValidate()
    {
        //getAllProficiency();
    }

    public void addProficiencyExp(string id, int addExp) 
    {
        ProficiencyDataSO proficiencyDataSO = DataManager.instance.proficiencyDataList.getData(id);
        if (proficiencyIndexDic[id].curLevel >= proficiencyDataSO.levelSettings.Count)
            return;

        proficiencyIndexDic[id].allExp += addExp;
        getAllProficiency();
    }

    public int getAllAttribute() 
    {
        int allAttribute = 0;
        allAttribute += this.strength;
        allAttribute += this.dexterity;
        allAttribute += this.constitution;
        allAttribute += this.intelligence;
        allAttribute += this.wisdom;
        allAttribute += this.charisma;

        return allAttribute;
    }

    public List<string> getProficiencyIDList(ProficiencyType type)
    {
        List<string> idList = proficiencyList
        .Where(l => type == DataManager.instance.proficiencyDataList.getData(l.id).type)
        .Select(l => l.id).ToList();

        return idList;
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
        proficiencyIndexDic.Clear();

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

        //取得所有修練項
        for (int i = 0; i < proficiencyList.Count; i++) 
        {
            ProficiencyDataSO curProficiency = DataManager.instance.proficiencyDataList.getData(proficiencyList[i].id);
            proficiencyIndexDic[proficiencyList[i].id] = proficiencyList[i];
            
            int curProficiencyAllExp = proficiencyList[i].allExp;
            int curProficiencyLevel = 0;

            //計算當前等級
            for (int j = 0; j < curProficiency.levelSettings.Count; j++) 
            {
                if (curProficiencyAllExp < curProficiency.levelSettings[j].needExp) 
                {
                    break;
                }
                else 
                {
                    curProficiencyAllExp -= curProficiency.levelSettings[j].needExp;
                }
                curProficiencyLevel = j + 1;
            }

            proficiencyList[i].curLevel = curProficiencyLevel;
            proficiencyList[i].curExp = curProficiencyAllExp;

            //取得所有等級屬性
            for (int j = 0; j < curProficiencyLevel; j++) 
            {
                //取得該等級所有屬性
                foreach(ProficiencyEffectData levelEffect in curProficiency.levelSettings[j].effects)
                {
                    getProficiencyAttribute(levelEffect.type, levelEffect.value, curProficiency.proficiencyName);
                }
            }
        }
    }

    private void getProficiencyAttribute(Attribute attributeType, string value, string attributeSourceName) 
    {
        switch (attributeType)
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

        if (attributeSourceDic[attributeType].ContainsKey(attributeSourceName)) 
        {
            attributeSourceDic[attributeType][attributeSourceName] += int.Parse(value);
        }
        else
        {
            attributeSourceDic[attributeType][attributeSourceName] = int.Parse(value);
        }
    }
}
