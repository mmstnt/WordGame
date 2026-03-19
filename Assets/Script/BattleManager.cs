using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using static BattleSystemDataSO;

public class BattleManager : MonoBehaviour
{
    public LayerMask unitLayer;
    [Header("廣播")]
    public VoidEventSO battleUIInitializeEvent;
    public VoidEventSO enterSelectEvent;
    public VoidEventSO WaitActionReactivateEvent;
    public StringEventSO nextRoundEvent;

    [Header("監聽")]
    public VoidEventSO unitRoundEndEvent;
    public VoidEventSO setUnitColorEvent;
    public StringEventSO selectUnitConfirmEvent;
    public StringEventSO castSkillEvent;
    public Vector2EventSO keyboardSelectUnitEvent;
    public Vector2EventSO mouseMoveEvent;

    [Header("資料")]
    public BattleSystemDataSO battleSystemData;

    [Header("組件")]
    public Unit playerUnit;
    public Transform unitGroup;
    public Transform unitHPBarGroup;
    public GameObject unitGameObject;
    public GameObject unitHPBarGameObject;
    
    private enum InputMode { Mouse, Keyboard }
    private InputMode inputMode;

    private void Awake()
    {
        inputMode = InputMode.Mouse;
    }

    private void OnEnable()
    {
        unitRoundEndEvent.onEventRaised += onUnitRoundEndEvent;
        selectUnitConfirmEvent.onEventRaised += onSelectUnitConfirmEvent;
        castSkillEvent.onEventRaised += onCastSkillEvent;
        keyboardSelectUnitEvent.onEventRaised += onKeyboardSelectUnitEvent;
        mouseMoveEvent.onEventRaised += onMouseMoveEvent;
    }

    private void OnDisable()
    {
        unitRoundEndEvent.onEventRaised -= onUnitRoundEndEvent;
        selectUnitConfirmEvent.onEventRaised -= onSelectUnitConfirmEvent;
        castSkillEvent.onEventRaised -= onCastSkillEvent;
        keyboardSelectUnitEvent.onEventRaised -= onKeyboardSelectUnitEvent;
        mouseMoveEvent.onEventRaised -= onMouseMoveEvent;
    }

    private void onMouseMoveEvent(Vector2 mousePos)
    {
        inputMode = InputMode.Mouse;

        if (battleSystemData.battleState == BattleState.SelectUnit && inputMode == InputMode.Mouse)
        {
            mouseSelect(mousePos);
        }
    }

    private void mouseSelect(Vector2 mousePos) 
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, 0f, unitLayer);

        if (hit.collider != null)
        {
            Unit currentUnit = hit.collider.GetComponent<Unit>();

            if (currentUnit != null && currentUnit != battleSystemData.curSelectUnit)
            {
                battleSystemData.curSelectUnit = currentUnit;
                setUnitColorEvent.raiseEvent();
            }
        }
    }

    public void onCastSkillEvent(string skillID)
    {
        if (battleSystemData.battleState != BattleState.Ready)
            return;

        SkillDataSO castSkill = DataManager.instance.skillDataList.getData(skillID);
        if (castSkill.AC > battleSystemData.playerUnit.curAC || castSkill.MP > battleSystemData.playerUnit.curMP) 
            return;

        battleSystemData.curSelectSkill = DataManager.instance.skillDataList.getData(skillID);
        battleSystemData.battleState = BattleState.SelectUnit;

        enterSelectEvent.onEventRaised();

        setUnitColorEvent.raiseEvent();
    }


    public void onKeyboardSelectUnitEvent(Vector2 dir)
    {
        if (battleSystemData.battleState != BattleState.SelectUnit)
            return;

        inputMode = InputMode.Keyboard;
        battleSystemData.curSelectUnit = selectUnit(dir);
        setUnitColorEvent.raiseEvent();
    }

    public void onSelectUnitConfirmEvent(string inputDevice) 
    {
        if (battleSystemData.battleState != BattleState.SelectUnit)
            return;
        
        //動滑鼠時偵測單位
        if (inputDevice == "Mouse") 
        {
            Vector2 mousePosition = Pointer.current.position.ReadValue();
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, 0f, unitLayer);

            if (hit.collider == null || hit.collider.GetComponent<Unit>() != battleSystemData.curSelectUnit)
                return;
        }

        battleSystemData.playerUnit.curAC -= battleSystemData.curSelectSkill.AC;
        battleSystemData.playerUnit.curMP -= battleSystemData.curSelectSkill.MP;

        int damage = BattleCalculation.damageCalculation(battleSystemData.curSelectSkill);
        battleSystemData.curSelectUnit.takeDamage(damage);

        battleSystemData.battleState = BattleState.Ready;
        setUnitColorEvent.raiseEvent();
        WaitActionReactivateEvent.raiseEvent();
    }


    public void battleInitialize(string battleID, PlayerDataSO playerData) 
    {
        battleSystemData.initialize();
        battleSystemData.playerBattleData = playerData;
        //初始化玩家
        battleSystemData.playerUnit = playerUnit;
        //UnitHPBar HPBar = ;
        //battleSystemData.playerUnit.initialize(playerData, HPBar);

        //生成敵人單位
        BattleDataSO battleData = DataManager.instance.battleDataList.getData(battleID);
        battleSystemData.enemyUnit = new Unit[battleData.mapData.siteCount];
        for (int i = 0; i < battleData.enemyUnit.Length; i++) 
        {
            UnitDataSO unitData = DataManager.instance.unitDataList.getData(battleData.enemyUnit[i]);
            if (unitData != default) 
            {
                battleSystemData.enemyUnit[i] = Instantiate(unitGameObject, battleData.mapData.unitSite[i], Quaternion.identity, unitGroup).GetComponent<Unit>();
                UnitHPBar unitHPBar = Instantiate(unitHPBarGameObject, unitHPBarGroup).GetComponent<UnitHPBar>();
                //初始化敵人血條
                battleSystemData.enemyUnit[i].initialize(unitData, unitHPBar);
            }
        }
        //建立單位行動清單
        List<Unit> allUnit = new List<Unit>();
        allUnit.Add(battleSystemData.playerUnit);
        allUnit.AddRange(battleSystemData.enemyUnit);
        foreach (Unit unit in allUnit) 
        {
            unitActionPoint unitAC = new unitActionPoint();
            unitAC.unit = unit;
            unitAC.actionPoint = 0;
            battleSystemData.unitSpeedList.Add(unitAC);
        }

        battleSystemData.curSelectUnit = reSetSelectUnit();
        battleUIInitializeEvent.raiseEvent();
    }

    public void onUnitRoundEndEvent() 
    {
        //預測單位
        battleSystemData.preUnitSpeedList.Clear();
        List<unitActionPoint> preUnitActionPointList = battleSystemData.unitSpeedList.Select(u => new unitActionPoint
        {
            unit = u.unit,
            actionPoint = u.actionPoint
        }).ToList();

        for (int i = 0; i < 10; i++) 
        {
            battleSystemData.preUnitSpeedList.Add(preUnitActionPointList.OrderBy(u => (100 - u.actionPoint) / u.unit.unitData.dexterity).First().unit);
            BattleCalculation.unitSpeedCalculation(preUnitActionPointList);
        }

        //下一個單位
        battleSystemData.curActionUnit = battleSystemData.unitSpeedList.OrderBy(u => (100 - u.actionPoint) / u.unit.unitData.dexterity).First().unit;
        BattleCalculation.unitSpeedCalculation(battleSystemData.unitSpeedList);
        unitAction(battleSystemData.curActionUnit);

        //下一回合
        battleSystemData.curRound += 1;
    }

    public void unitAction(Unit curActionUnit) 
    {
        curActionUnit.curAC = curActionUnit.maxAC;

        if (curActionUnit.unitData is PlayerDataSO playerData) 
        {
            nextRoundEvent.raiseEvent("Player");
        }
        else if(curActionUnit.unitData is UnitDataSO unitData)
        {
            nextRoundEvent.raiseEvent("Unit");
            Debug.Log("回合結束");
            onUnitRoundEndEvent();
        }

    }

    public Unit reSetSelectUnit() 
    {
        for (int i = 0; i < battleSystemData.enemyUnit.Length; i++) 
        {
            if (battleSystemData.enemyUnit[i] != null)
            {
                return battleSystemData.enemyUnit[i];
            }
        }
        return null;
    }

    public Unit selectUnit(Vector2 dir) 
    {
        if (battleSystemData.curSelectUnit == null)
            return reSetSelectUnit();

        int index = Array.IndexOf(battleSystemData.enemyUnit, battleSystemData.curSelectUnit);
        if (dir.x > 0) 
        {
            for (int i = index + 1; i < battleSystemData.enemyUnit.Length; i++) 
            {
                if (battleSystemData.enemyUnit[i] != null) 
                    return battleSystemData.enemyUnit[i];
            }
        }
        else if (dir.x < 0) 
        {
            for (int i = index - 1; i >= 0; i--) 
            {
                if (battleSystemData.enemyUnit[i] != null)
                    return battleSystemData.enemyUnit[i];
            }
        }
        else if (dir.y > 0) 
        {
            for (int i = 0; i < battleSystemData.enemyUnit.Length; i++) 
            {
                if (battleSystemData.enemyUnit[i] != null)
                    return battleSystemData.enemyUnit[i];
            }
        }
        else if (dir.y < 0) 
        {
            for (int i = battleSystemData.enemyUnit.Length - 1; i >= 0; i--) 
            {
                if (battleSystemData.enemyUnit[i] != null)
                    return battleSystemData.enemyUnit[i];
            }
        }

        return battleSystemData.enemyUnit[index];
    }

}
