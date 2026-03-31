using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Unit : MonoBehaviour
{
    public BaseUnitSO unitData;
    public BattleFaction faction;

    [Header("基本資源")]
    public int maxHP;
    public int maxMP;
    public int maxAC;

    [Header("基本概率")]
    public float hitRate;
    public float criticalHitRate;
    public float dodgeRate;

    [Header("狀態")]
    public bool isDead;

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

    public void initialize(BaseUnitSO loadUnitData, UnitHPBar loadUnitHPBar, BattleFaction battleFaction) 
    {
        isDead = false;
        faction = battleFaction;

        unitData = loadUnitData;
        unitHPBar = loadUnitHPBar;
        unitAttributeCalculation();

        curHP = maxHP;
        curMP = maxMP;
        curAC = maxAC;

        if (loadUnitData is UnitDataSO) 
        {
            this.transform.GetComponent<SpriteRenderer>().sprite = unitData.image;

            unitHPBar.initialize(this);
        }
        else if(loadUnitData is PlayerDataSO) 
        {
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

        if (curHP < 0)
        {
            curHP = 0;
            isDead = true;
        }
        unitHPBar.changeHP(this);
    }

    public void die() 
    {
        if(unitData is UnitDataSO)
            Destroy(this.gameObject);
    }

    public IEnumerator hurtFlash(float duration)
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        Image image = GetComponent<Image>();

        //預設為原始顏色
        Color originalColor = Color.white;

        //兩組件都沒有就直接結束
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
        else if (image != null) originalColor = image.color;
        else yield break;

        //變紅
        if (spriteRenderer != null) spriteRenderer.color = Color.red;
        if (image != null) image.color = Color.red;
        
        //等待
        yield return new WaitForSeconds(duration);

        //還原顏色
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
        if (image != null) image.color = originalColor;
    }
}
