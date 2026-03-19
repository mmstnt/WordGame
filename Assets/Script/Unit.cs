using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public BaseUnitSO unitData;

    [Header("基本資源")]
    public int maxHP;
    public int maxMP;
    public int maxAC;

    [Header("基本概率")]
    public float hitRate;
    public float criticalHitRate;
    public float dodgeRate;

    [Header("組件")]
    public int curHP;
    public int curMP;
    public int curAC;

    private UnitHPBar unitHPBar;

    private void OnDestroy()
    {
        if (unitData is UnitDataSO)
            Destroy(unitHPBar.gameObject);
    }

    public void initialize(BaseUnitSO loadUnitData, UnitHPBar loadUnitHPBar) 
    {
        if(loadUnitData is UnitDataSO) 
        {
            unitData = loadUnitData;
            unitHPBar = loadUnitHPBar;

            this.transform.GetComponent<SpriteRenderer>().sprite = unitData.image;
            unitAttributeCalculation();

            curHP = maxHP;
            curMP = maxMP;
            curAC = maxAC;

            unitHPBar.initialize(this);
        }
        else if(loadUnitData is PlayerDataSO) 
        {
            unitData = loadUnitData;
            unitHPBar = loadUnitHPBar;

            unitAttributeCalculation();

            curHP = maxHP;
            curMP = maxMP;
            curAC = maxAC;

            unitHPBar.changeHP(this);
        }
    }

    public void unitAttributeCalculation() 
    {
        maxHP = BattleCalculation.maxHPCalculation(unitData);
        maxMP = BattleCalculation.maxMPCalculation(unitData);
        maxAC = BattleCalculation.maxACCalculation(unitData);

        hitRate = BattleCalculation.hitRateCalculation(unitData);
        criticalHitRate = BattleCalculation.criticalHitRateCalculation(unitData);
        dodgeRate = BattleCalculation.dodgeRateCalculation(unitData);
    }

    public void takeDamage(int damage) 
    {
        curHP -= damage;

        if (curHP < 0) curHP = 0;
        unitHPBar.changeHP(this);
    }
}
