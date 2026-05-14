using Ink.Parsed;
using System.Collections.Generic;
using UnityEngine;
using static ProficiencyDataSO;

[CreateAssetMenu(menuName = "Data/PlayerDataSO")]
public class PlayerDataSO : BaseUnitSO
{
    public string[] curMartial;
    public string[] curMagic;
    public string[] item;
    
    [Header("¾i¦¨¸ê°T")]
    public int developRound;
    public int developActionPoint;
    public List<Proficiency> proficiencyList;

    [System.Serializable]
    public struct Proficiency 
    {
        public int curExp;
        public string id;
    }

    private void OnValidate()
    {
        getAllProficiency();
    }

    private void getAllProficiency() 
    {
        this.strength = 0;
        this.dexterity = 0;
        this.constitution = 0;
        this.intelligence = 0;
        this.wisdom = 0;
        this.charisma = 0;

        foreach(Proficiency proficiency in proficiencyList) 
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
                    getProficiencyAttribute(levelEffect.type, levelEffect.value);
                }
            }
        }
    }

    private void getProficiencyAttribute(Attribute effectType, string value) 
    {
        switch (effectType)
        {
            case Attribute.Strength:
                this.strength += int.Parse(value);
                break;
            case Attribute.Dexterity:
                this.dexterity += int.Parse(value);
                break;
            case Attribute.Constitution:
                this.constitution += int.Parse(value);
                break;
            case Attribute.Intelligence:
                this.intelligence += int.Parse(value);
                break;
            case Attribute.Wisdom:
                this.wisdom += int.Parse(value);
                break;
            case Attribute.Charisma:
                this.charisma += int.Parse(value);
                break;
        }
    }
}
