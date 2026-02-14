using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.EventSystems.StandaloneInputModule;

public class BattleManager : MonoBehaviour
{
    public LayerMask unitLayer;
    [Header("監聽")]
    public VoidEventSO selectConfirmEvent;
    public VoidEventSO selectBackEvent;
    public IntEventSO recordLastButtonEvent;
    public StringEventSO castSkillEvent;
    public StringEventSO switchSkillGroupEvent;
    public Vector2EventSO selectEvent;

    [Header("戰場")]
    public PlayerDataSO playerBattleData;
    public Unit[] enemyUnit;
    public BattleState battleState;

    [Header("組件")]
    public Transform unitGroup;
    public Transform unitHPBarGroup;
    public GameObject unitGameObject;
    public GameObject unitHPBarGameObject;
    public GameObject skillButtonGameObject;
    
    [Header("UI")]
    public Transform actionGroup;
    public Transform skillGroup;

    private Unit curSelectUnit;
    private SkillDataSO curSkill;
    private Transform curUI;

    private enum InputMode { Mouse, Keyboard }
    private InputMode inputMode;
    private Vector2 lastMousePos;
    private int lastActionButtonIndex;
    private int lastSkillButtonIndex;

    private void Awake()
    {
        inputMode = InputMode.Mouse;
        battleState = BattleState.Ready;
    }

    private void OnEnable()
    {
        selectConfirmEvent.onEventRaised += onSelectConfirmEvent;
        selectBackEvent.onEventRaised += onSelectBackEvent;
        castSkillEvent.onEventRaised += onCastSkillEvent;
        recordLastButtonEvent.onEventRaised += onRecordLastButtonEvent;
        switchSkillGroupEvent.onEventRaised += onSwitchSkillGroupEvent;
        selectEvent.onEventRaised += onSelectEvent;
    }

    private void OnDisable()
    {
        selectConfirmEvent.onEventRaised -= onSelectConfirmEvent;
        selectBackEvent.onEventRaised -= onSelectBackEvent;
        castSkillEvent.onEventRaised -= onCastSkillEvent;
        recordLastButtonEvent.onEventRaised -= onRecordLastButtonEvent;
        switchSkillGroupEvent.onEventRaised -= onSwitchSkillGroupEvent;
        selectEvent.onEventRaised -= onSelectEvent;
    }

    public void Update()
    {
        switchInputModeToMouse();

        if (battleState == BattleState.Select && inputMode == InputMode.Mouse) 
        {
            mouseSelect();
        }
    }

    private void mouseSelect() 
    {
        Vector2 mousePosition = Pointer.current.position.ReadValue();
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePosition);

        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, 0f, unitLayer);

        if (hit.collider != null)
        {
            Unit currentUnit = hit.collider.GetComponent<Unit>();

            if (currentUnit != null)
            {
                if (currentUnit != curSelectUnit)
                {
                    curSelectUnit = currentUnit;
                    setUnitColor();
                }

                if (Pointer.current.press.wasPressedThisFrame)
                {
                    onSelectConfirmEvent();
                }
            }
        }
    }

    private void switchInputModeToMouse()
    {
        Vector2 curMousePos = Pointer.current.position.ReadValue();

        //計算滑鼠移動距離
        float mouseDelta = Vector2.Distance(curMousePos, lastMousePos);

        if (mouseDelta > 0)
        {
            inputMode = InputMode.Mouse; //滑鼠動了，切換回滑鼠模式
        }

        lastMousePos = curMousePos;
    }

    public void onCastSkillEvent(string skillID)
    {
        if (battleState != BattleState.Ready)
            return;
        
        curSkill = DataManager.instance.skillDataList.getData(skillID);

        enterSelect();
        setUnitColor();
    }

    public void enterSelect() 
    {
        //選擇單位模式
        curUI.gameObject.SetActive(false);
        battleState = BattleState.Select;
    }

    public void onSelectBackEvent() 
    {
        if (battleState == BattleState.Select)
        {
            battleState = BattleState.Ready;
            setUnitColor();
        }
        else if (curUI = skillGroup) 
        {
            Debug.Log("1");
            curUI.gameObject.SetActive(false);
            curUI = actionGroup;
        }

        StartCoroutine(waitActionReactivate());
    }

    public void onSelectEvent(Vector2 dir)
    {
        if (battleState != BattleState.Select)
            return;

        inputMode = InputMode.Keyboard;
        curSelectUnit = selectUnit(dir);
        setUnitColor();
    }

    public void onSelectConfirmEvent() 
    {
        if (battleState != BattleState.Select)
            return;

        int damage = BattleCalculation.damageCalculation(curSkill);
        curSelectUnit.takeDamage(damage);

        battleState = BattleState.Ready;
        setUnitColor();

        StartCoroutine(waitActionReactivate());
    }

    public IEnumerator waitActionReactivate() 
    {
        curUI.GetComponent<CanvasGroup>().interactable = false;
        curUI.gameObject.SetActive(true);

        yield return new WaitForEndOfFrame();

        curUI.GetComponent<CanvasGroup>().interactable = true;
        
        int index = 0;
        if (curUI == actionGroup)
            index = lastActionButtonIndex;
        else if (curUI == skillGroup)
            index = lastSkillButtonIndex;

        if (curUI.childCount > index) 
        {
            //獲取上個按鈕
            EventSystem.current.SetSelectedGameObject(curUI.transform.GetChild(index).gameObject);
        }
        else if (curUI.childCount > 0) 
        {
            //獲取UI子物件第一個按鈕
            EventSystem.current.SetSelectedGameObject(curUI.transform.GetChild(0).gameObject);
        }
    }

    public void onSwitchSkillGroupEvent(string skillType)
    {
        for (int i = 0; i < skillGroup.childCount; i++)
        {
            Destroy(skillGroup.GetChild(i).gameObject);
        }

        curUI.gameObject.SetActive(false);
        curUI = skillGroup;
        lastSkillButtonIndex = 0;
        StartCoroutine(waitActionReactivate());
        string[] skillList = null;

        switch (skillType) 
        {
            case "Martial":
                skillList = playerBattleData.curMartial;
                break;
            case "Magic":
                skillList = playerBattleData.curMagic;
                break;
            case "Item":
                skillList = playerBattleData.item;
                break;
        }
        createSkillButton(skillList);
    }

    public void createSkillButton(string[] skillList)
    {
        for (int i = 0; i < skillList.Length; i++)  
        {
            int index = i;
            int x = (i % 4) * 200;
            int y = (i / 4) * 80;
            Vector2 site = new Vector2(-300 + x, 40 + y);
            string skillID = skillList[i];

            GameObject skillButton = Instantiate(skillButtonGameObject, skillGroup);
            skillButton.GetComponent<RectTransform>().anchoredPosition = site;
            skillButton.GetComponentInChildren<TMP_Text>().text = DataManager.instance.skillDataList.getData(skillID).skillName;
            skillButton.GetComponent<Button>().onClick.AddListener
                (
                delegate
                {
                    onRecordLastButtonEvent(index);
                    onCastSkillEvent(skillID);
                }
                );
        }
    }

    public void onRecordLastButtonEvent(int index) 
    {
        if (curUI == actionGroup) 
        {
            lastActionButtonIndex = index;
        }
        else if (curUI == skillGroup) 
        {
            lastSkillButtonIndex = index;
        }
    }

    public void battleInitialize(string battleID, PlayerDataSO playerData) 
    {
        playerBattleData = playerData;

        BattleDataSO battleData = DataManager.instance.battleDataList.getData(battleID);
        enemyUnit = new Unit[battleData.mapData.siteCount];
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

        curUI = actionGroup;
        curSelectUnit = reSetSelectUnit();
        setUnitColor();
        StartCoroutine(waitActionReactivate());
    }


    public void setUnitColor() 
    {
        foreach(var unit in enemyUnit) 
        {
            unit.GetComponent<SpriteRenderer>().color = new Color(0.75f, 0.75f, 0.75f);
        }
        if (battleState == BattleState.Select && curSelectUnit != null)  
        {
            curSelectUnit.GetComponent<SpriteRenderer>().color = Color.white;
        }
    }

    public Unit reSetSelectUnit() 
    {
        for (int i = 0; i < enemyUnit.Length; i++) 
        {
            if (enemyUnit[i] != null)
            {
                return enemyUnit[i];
            }
        }
        return null;
    }

    public Unit selectUnit(Vector2 dir) 
    {
        if (curSelectUnit == null)
            return reSetSelectUnit();

        int index = Array.IndexOf(enemyUnit, curSelectUnit);
        if (dir.x > 0) 
        {
            for (int i = index + 1; i < enemyUnit.Length; i++) 
            {
                if (enemyUnit[i] != null) 
                    return enemyUnit[i];
            }
        }
        else if (dir.x < 0) 
        {
            for (int i = index - 1; i >= 0; i--) 
            {
                if (enemyUnit[i] != null)
                    return enemyUnit[i];
            }
        }
        else if (dir.y > 0) 
        {
            for (int i = 0; i < enemyUnit.Length; i++) 
            {
                if (enemyUnit[i] != null)
                    return enemyUnit[i];
            }
        }
        else if (dir.y < 0) 
        {
            for (int i = enemyUnit.Length - 1; i >= 0; i--) 
            {
                if (enemyUnit[i] != null)
                    return enemyUnit[i];
            }
        }

        return enemyUnit[index];
    }

}
