using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [Header("監聽")]
    public VoidEventSO gameConfirmEvent;
    public SkillEventSO castSkillEvent;

    [Header("戰場")]
    public Unit[] enemyUnit;

    [Header("組件")]
    public Transform unitGroup;
    public Transform unitHPBarGroup;
    public Transform skillGroup;
    public GameObject unitGameObject;
    public GameObject unitHPBarGameObject;
    public GameObject skillButton;

    private void Awake()
    {
        
    }

    private void OnEnable()
    {
        castSkillEvent.onEventRaised += onCastSkillEvent;
    }

    private void OnDisable()
    {
        castSkillEvent.onEventRaised -= onCastSkillEvent;
    }

    public void battleInitialize(string battleID) 
    {
        BattleDataSO battleData = DataManager.instance.battleDataList.getData(battleID);
        enemyUnit = new Unit[battleData.mapData.unitSite.Count];
        for (int i = 0; i < battleData.enemyUnit.Length; i++) 
        {
            UnitDataSO unitData = DataManager.instance.unitDataList.getData(battleData.enemyUnit[i]);
            if (unitData != default) 
            {
                enemyUnit[i] = Instantiate(unitGameObject, battleData.mapData.unitSite[i], Quaternion.identity, unitGroup).GetComponent<Unit>();
                UnitHPBar unitHPBar = Instantiate(unitHPBarGameObject, unitHPBarGroup).GetComponent<UnitHPBar>();

                enemyUnit[i].initialize(unitData, unitHPBar);
            }
        }
    }

    public void switchSkillBox() 
    {
        for (int i = 0; i < skillGroup.childCount; i++)
        {
            Destroy(skillGroup.GetChild(i).gameObject);
        }
    }

    public void onCastSkillEvent(string skillID) 
    {
        int damage = BattleCalculation.damageCalculation(DataManager.instance.skillDataList.getData(skillID));
        Debug.Log(damage);
        enemyUnit[0].takeDamage(damage);
    }
}
