using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
using UnityEngine;

public static class BattleCalculation
{
    public static int maxHPCalculation(UnitDataSO unitData) 
    {
        int maxHP = Mathf.RoundToInt(5 + ((unitData.constitution - 10) > 0 ? Mathf.Pow(unitData.constitution, 2) : Mathf.Pow(unitData.constitution, 2)) / 10);
        return maxHP;
    }

    public static int maxMPCalculation(UnitDataSO unitData) 
    {
        int maxMP = unitData.charisma / 5;
        return maxMP;
    }

    public static int maxACCalculation(UnitDataSO unitData)
    {
        int maxAC = 2 + unitData.dexterity / 5;
        return maxAC;
    }

    public static float hitRateCalculation(UnitDataSO unitData) 
    {
        float hitRate = 70 + unitData.wisdom;
        return hitRate;
    }

    public static float criticalHitRateCalculation(UnitDataSO unitData)
    {
        float criticalHitRate = 0.64f * unitData.strength + 0.32f * unitData.wisdom;
        return criticalHitRate;
    }

    public static float dodgeRateCalculation(UnitDataSO unitData)
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
}
