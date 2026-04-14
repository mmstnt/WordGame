using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using static BattleSystemDataSO;
using static UnityEngine.GraphicsBuffer;
using Random =UnityEngine.Random;

public class BattleManager : MonoBehaviour
{
    public LayerMask unitLayer;
    [Header("廣播")]
    public VoidEventSO battleUIInitializeEvent;
    public VoidEventSO enterSelectUnitEvent;
    public VoidEventSO waitActionReactivateEvent;
    public VoidEventSO battleResultEvent;
    public StringEventSO nextRoundEvent;
    public SkillEffectEventSO skillEffectEvent;

    [Header("監聽")]
    public VoidEventSO unitRoundEndEvent;
    public VoidEventSO setUnitColorEvent;
    public StringEventSO selectUnitConfirmEvent;
    public StringEventSO chooseCastSkillEvent;
    public Vector2EventSO keyboardSelectUnitEvent;
    public Vector2EventSO mouseMoveEvent;

    [Header("資料")]
    public BattleSystemDataSO battleSystemData;

    [Header("組件")]
    public Unit playerUnit;
    public UnitHPBar playerHPBar;
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
        chooseCastSkillEvent.onEventRaised += onChooseCastSkillEvent;
        keyboardSelectUnitEvent.onEventRaised += onKeyboardSelectUnitEvent;
        mouseMoveEvent.onEventRaised += onMouseMoveEvent;
    }

    private void OnDisable()
    {
        unitRoundEndEvent.onEventRaised -= onUnitRoundEndEvent;
        selectUnitConfirmEvent.onEventRaised -= onSelectUnitConfirmEvent;
        chooseCastSkillEvent.onEventRaised -= onChooseCastSkillEvent;
        keyboardSelectUnitEvent.onEventRaised -= onKeyboardSelectUnitEvent;
        mouseMoveEvent.onEventRaised -= onMouseMoveEvent;
    }

    private void onMouseMoveEvent(Vector2 mousePos)
    {
        inputMode = InputMode.Mouse;

        if (battleSystemData.battleState == BattleState.SelectUnit && inputMode == InputMode.Mouse)
        {
            mouseSelectUnit(mousePos);
        }
    }

    private void mouseSelectUnit(Vector2 mousePos) 
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

    public void onChooseCastSkillEvent(string skillID)
    {
        if (battleSystemData.battleState != BattleState.Ready)
            return;

        //判斷選擇的技能能否施放
        SkillDataSO castSkill = DataManager.instance.skillDataList.getData(skillID);
        if (castSkill.AC > battleSystemData.playerUnit.curAC || castSkill.MP > battleSystemData.playerUnit.curMP)
            return;

        battleSystemData.curSelectSkill = DataManager.instance.skillDataList.getData(skillID);
        battleSystemData.battleState = BattleState.SelectUnit;

        if (battleSystemData.curSelectUnit == null)
            battleSystemData.curSelectUnit = reSetSelectUnit();

        enterSelectUnitEvent.onEventRaised();
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

        //施放技能
        StartCoroutine(castSkill(battleSystemData.playerUnit, battleSystemData.curSelectUnit, battleSystemData.curSelectSkill));

        battleSystemData.battleState = BattleState.Ready;
        setUnitColorEvent.raiseEvent();
        waitActionReactivateEvent.raiseEvent();
    }

    public IEnumerator castSkill(Unit caster, Unit target, SkillDataSO skill)
    {
        if (skill.AC > caster.curAC || skill.MP > caster.curMP)
            yield break;

        //扣除技能消耗
        caster.curAC -= skill.AC;
        caster.curMP -= skill.MP;

        //播放動畫
        bool isAniFinish = false;
        skillEffectEvent.raiseEvent(skill, target.transform.position, () =>
        {
            isAniFinish = true;
        }
        );

        yield return new WaitUntil(() => isAniFinish);

        //造成傷害
        int damage = BattleCalculation.damageCalculation(skill);
        target.takeDamage(damage);

        yield return StartCoroutine(target.hurtFlash(0.1f));

        //判斷單位死亡
        if (target.isDead) 
        {
            target.die();
            if (battleSystemData.enemyUnit.All(u => u == null || u.isDead))
            {
                //廣播給UI管理器播放戰鬥結束動畫(還沒做)
                battleResultEvent.raiseEvent();
                //測試結算
                GameEventManager.instance.endBattle(true);
                battleSystemData.battleState = BattleState.End;
                StopAllCoroutines();
                Debug.Log("結算");
                yield break;
            }
            else if (battleSystemData.playerUnit.isDead) 
            {

                GameEventManager.instance.endBattle(false);

                battleSystemData.battleState = BattleState.End; 
                StopAllCoroutines();
                Debug.Log("結算");
                yield break;
            }
        }

        preUnitSpeed();
        waitActionReactivateEvent.raiseEvent() ;

        Debug.Log(damage+skill.name);
    }

    public void battleInitialize(string battleID, PlayerDataSO playerData) 
    {
        battleSystemData.initialize();
        battleSystemData.playerBattleData = playerData;
        //初始化玩家
        battleSystemData.playerUnit = playerUnit;
        battleSystemData.playerUnit.initialize(battleSystemData.playerBattleData, playerHPBar, BattleFaction.Player);

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
                battleSystemData.enemyUnit[i].initialize(unitData, unitHPBar, BattleFaction.Enemy);
            }
        }
        //建立單位行動清單
        List<Unit> allUnit = new List<Unit>();
        allUnit.Add(battleSystemData.playerUnit);
        allUnit.AddRange(battleSystemData.enemyUnit);
        foreach (Unit unit in allUnit) 
        {
            unitActionPoint unitACPoint = new unitActionPoint();
            unitACPoint.unit = unit;
            unitACPoint.actionPoint = 0;
            battleSystemData.unitSpeedList.Add(unitACPoint);
        }

        battleSystemData.curSelectUnit = reSetSelectUnit();
        battleUIInitializeEvent.raiseEvent();
        
        onUnitRoundEndEvent();
    }

    public void onUnitRoundEndEvent() 
    {
        //移除所有空單位
        battleSystemData.unitSpeedList.RemoveAll(u => u.unit == null || u.unit.isDead);

        //下一個單位
        battleSystemData.curActionUnit = battleSystemData.unitSpeedList.OrderBy(u => (100 - u.actionPoint) / u.unit.unitData.dexterity).First().unit;
        BattleCalculation.unitSpeedCalculation(battleSystemData.unitSpeedList);
        
        //預測單位
        preUnitSpeed();

        //單位行動
        StartCoroutine(unitAction(battleSystemData.curActionUnit));

        //下一回合
        battleSystemData.curRound += 1;
    }

    public void preUnitSpeed() 
    {
        battleSystemData.preUnitSpeedList.Clear();

        List<unitActionPoint> preUnitActionPointList = battleSystemData.unitSpeedList
        .Where(u => u.unit != null && !u.unit.isDead)
        .Select(u => new unitActionPoint
        {
            unit = u.unit,
            actionPoint = u.actionPoint
        }).ToList();

        for (int i = 0; i < 8; i++)
        {
            if (i == 0) 
            {
                battleSystemData.preUnitSpeedList.Add(battleSystemData.curActionUnit);
            }
            else 
            {
                battleSystemData.preUnitSpeedList.Add(preUnitActionPointList.OrderBy(u => (100 - u.actionPoint) / u.unit.unitData.dexterity).First().unit);
                BattleCalculation.unitSpeedCalculation(preUnitActionPointList);
            }
        }
    }

    public IEnumerator unitAction(Unit curActionUnit) 
    {
        curActionUnit.curAC = curActionUnit.maxAC;

        if (curActionUnit.unitData is PlayerDataSO playerData) 
        {
            nextRoundEvent.raiseEvent("Player");
        }
        else if(curActionUnit.unitData is UnitDataSO unitData)
        {
            nextRoundEvent.raiseEvent("Unit");
            //抽取單位技能建立施放清單
            List<string> unitCastSkillList = createUnitCastSkillList(curActionUnit);
            foreach(string castSkillID in unitCastSkillList) 
            {
                Unit target = battleSystemData.playerUnit;
                SkillDataSO skill = DataManager.instance.skillDataList.getData(castSkillID);
                yield return StartCoroutine(castSkill(curActionUnit, target, skill));
            }
            Debug.Log("回合結束");
            onUnitRoundEndEvent();
        }
    }

    public List<string> createUnitCastSkillList(Unit curUnit)
    {
        List<string> unitActionSkillList = new List<string>();
        int preAC = curUnit.curAC;
        int preMP = curUnit.curMP;

        //抽取單位施放的技能
        string castSkill = drawCastSkill(curUnit, preAC, preMP);

        while (castSkill != null) 
        {
            //添加抽取技能至行動清單
            unitActionSkillList.Add(castSkill);

            //更新單位狀態
            preAC -= DataManager.instance.skillDataList.getData(castSkill).AC;
            preMP -= DataManager.instance.skillDataList.getData(castSkill).MP;

            //再次抽取技能
            castSkill = drawCastSkill(curUnit, preAC, preMP);
        }

        return unitActionSkillList;
    }

    public string drawCastSkill(Unit curUnit, int preAC, int preMP)
    {
        //轉為單位數據
        UnitDataSO unitData = curUnit.unitData as UnitDataSO;

        //取得目前可施放的技能清單
        string[] drawSkillList = unitData.unitSkill.Where
            (s =>
            (DataManager.instance.skillDataList.getData(s).AC) <= preAC &&
            (DataManager.instance.skillDataList.getData(s).MP) <= preMP
            ).ToArray();

        if (drawSkillList.Length <= 0)
            return null;

        //抽取施放技能
        int index = Random.Range(0, drawSkillList.Length);
        string skillID = drawSkillList[index];
        
        return skillID;
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
