using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using static BattleSystemDataSO;

public static class BattleCalculation
{
    public static int maxHPCalculation(BaseUnitSO unitData) 
    {
        int maxHP = Mathf.RoundToInt(5 + ((unitData.constitution - 10) > 0 ? Mathf.Pow(unitData.constitution, 2) : Mathf.Pow(unitData.constitution, 2)) / 10);
        return maxHP;
    }

    public static int maxMPCalculation(BaseUnitSO unitData) 
    {
        int maxMP = unitData.charisma / 5;
        return maxMP;
    }

    public static int maxACCalculation(BaseUnitSO unitData)
    {
        int maxAC = 2 + unitData.dexterity / 5;
        return maxAC;
    }

    public static float hitRateCalculation(BaseUnitSO unitData) 
    {
        float hitRate = 70 + unitData.wisdom;
        return hitRate;
    }

    public static float criticalHitRateCalculation(BaseUnitSO unitData)
    {
        float criticalHitRate = 0.64f * unitData.strength + 0.32f * unitData.wisdom;
        return criticalHitRate;
    }

    public static float dodgeRateCalculation(BaseUnitSO unitData)
    {
        float dodgeRate = 0.48f * unitData.dexterity + 0.24f * unitData.wisdom;
        return dodgeRate;
    }

    public static int damageCalculation(SkillDataSO skillData) 
    {
        if (skillData == default) return 0;

        int damage = 0;
        for (int i = 0; i < skillData.damageDiceCount; i++) 
        {
            damage += Random.Range(1, skillData.damageDiceSide + 1);
        }
        damage += skillData.modifier;
        return damage;
    }


    /*public static List<unitActionPoint> AunitSpeedCalculation(List<unitActionPoint> unitSpeedList) 
    {
        List<unitActionPoint> preUnitActionPointList = unitSpeedList.Select(u => new unitActionPoint
        {
            unit = u.unit,
            actionPoint = u.actionPoint
        }).ToList();

        List<unitActionPoint> unitActionList = new List<unitActionPoint>();

        for(int i = 0; i < 10; i++) 
        {
            unitActionPoint nextUnit = preUnitActionPointList.OrderBy(u => (100 - u.actionPoint) / u.unit.unitData.dexterity).First();
            float needTime = (100 - nextUnit.actionPoint) / nextUnit.unit.unitData.dexterity;

            foreach(var unit in preUnitActionPointList) 
            {
                unit.actionPoint += unit.unit.unitData.dexterity * needTime;
            }

            unitActionList.Add(nextUnit);
            nextUnit.actionPoint -= 100;
        }

        

        return unitActionList;
    }*/

    public static void unitSpeedCalculation(List<unitActionPoint> unitSpeedList)
    {
        unitActionPoint nextUnit = unitSpeedList.OrderBy(u => (100 - u.actionPoint) / u.unit.unitData.dexterity).First();
        float needTime = (100 - nextUnit.actionPoint) / nextUnit.unit.unitData.dexterity;

        foreach (var unit in unitSpeedList)
        {
            unit.actionPoint += unit.unit.unitData.dexterity * needTime;
        }

        nextUnit.actionPoint -= 100;
    }
}
