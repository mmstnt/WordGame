using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BattleUIManager : MonoBehaviour
{
    [Header("廣播")]
    public StringEventSO castSkillEvent;

    [Header("監聽")]
    public VoidEventSO battleUIInitializeEvent;
    public VoidEventSO setUnitColorEvent;
    public VoidEventSO selectBackEvent;
    public VoidEventSO enterSelectEvent;
    public VoidEventSO WaitActionReactivateEvent;
    public IntEventSO recordLastButtonEvent;
    public StringEventSO switchSkillGroupEvent;
    public StringEventSO nextRoundEvent;

    [Header("資料")]
    public BattleSystemDataSO battleSystemData;

    [Header("UI群組")]
    public Transform actionGroup;
    public Transform skillGroup;
    public Transform acGrounp;
    public Transform mpGrounp;

    [Header("玩家UI")]
    public UnitHPBar playerHPBar;

    [Header("組件")]
    public GameObject skillButtonGameObject;
    public GameObject pointGameObject;

    private Transform curUI;
    private int lastActionButtonIndex;
    private int lastSkillButtonIndex;

    private void OnEnable()
    {
        battleUIInitializeEvent.onEventRaised += battleUIInitialize;
        setUnitColorEvent.onEventRaised += setUnitColor;
        selectBackEvent.onEventRaised += onSelectBackEvent;
        enterSelectEvent.onEventRaised += onEnterSelect;
        WaitActionReactivateEvent.onEventRaised += onWaitActionReactivateEvent;
        nextRoundEvent.onEventRaised += onNextRoundEvent;
        recordLastButtonEvent.onEventRaised += onRecordLastButtonEvent;
        switchSkillGroupEvent.onEventRaised += onSwitchSkillGroupEvent;
    }

    private void OnDisable()
    {
        battleUIInitializeEvent.onEventRaised -= battleUIInitialize;
        setUnitColorEvent.onEventRaised -= setUnitColor;
        selectBackEvent.onEventRaised -= onSelectBackEvent;
        enterSelectEvent.onEventRaised -= onEnterSelect;
        WaitActionReactivateEvent.onEventRaised -= onWaitActionReactivateEvent;
        nextRoundEvent.onEventRaised -= onNextRoundEvent;
        recordLastButtonEvent.onEventRaised -= onRecordLastButtonEvent;
        switchSkillGroupEvent.onEventRaised -= onSwitchSkillGroupEvent;
    }

    public void battleUIInitialize() 
    {
        for (int i = acGrounp.childCount - 1; i >= 0; i--) 
        {
            GameObject acGameObject = acGrounp.GetChild(i).gameObject;
            acGameObject.transform.SetParent(null);
            Destroy(acGameObject);
        }

        for (int i = mpGrounp.childCount - 1; i >= 0; i--) 
        {
            GameObject mpGameObject = mpGrounp.GetChild(i).gameObject;
            mpGameObject.transform.SetParent(null);
            Destroy(mpGameObject);
        }

        curUI = actionGroup;
        //初始化玩家血條
        battleSystemData.playerUnit.initialize(battleSystemData.playerBattleData, playerHPBar);
        for(int i = 0; i < battleSystemData.playerUnit.maxAC; i++) 
        {
            GameObject acPoint = Instantiate(pointGameObject, acGrounp);
            acPoint.GetComponent<Image>().sprite = DataManager.instance.uiImageDataList.getData("1");
        }
        for (int i = 0; i < battleSystemData.playerUnit.maxMP; i++)
        {
            GameObject mpPoint = Instantiate(pointGameObject, mpGrounp);
            mpPoint.GetComponent<Image>().sprite = DataManager.instance.uiImageDataList.getData("3");
        }

        setUnitColor();
        onWaitActionReactivateEvent();
    }

    public void onEnterSelect()
    {
        //選擇單位模式
        curUI.gameObject.SetActive(false);
    }

    public void onSelectBackEvent()
    {
        if (battleSystemData.battleState == BattleState.SelectUnit)
        {
            battleSystemData.battleState = BattleState.Ready;
            setUnitColorEvent.raiseEvent();
        }
        else if (curUI = skillGroup)
        {
            curUI.gameObject.SetActive(false);
            curUI = actionGroup;
        }

        StartCoroutine(waitActionReactivate());
    }

    public void onNextRoundEvent(string unitKind) 
    {
        //看誰的回合
        switch (unitKind) { 
            case "Player":
                curUI.gameObject.SetActive(false);
                curUI = actionGroup;
                lastActionButtonIndex = 0;
                onWaitActionReactivateEvent();
                break;
            case "Unit":
                curUI.gameObject.SetActive(false);
                break;
        }
    }

    public void onWaitActionReactivateEvent()
    {
        updateUI();
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
                skillList = battleSystemData.playerBattleData.curMartial;
                break;
            case "Magic":
                skillList = battleSystemData.playerBattleData.curMagic;
                break;
            case "Item":
                skillList = battleSystemData.playerBattleData.item;
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
                    castSkillEvent.onEventRaised(skillID);
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

    public void setUnitColor()
    {
        foreach (var unit in battleSystemData.enemyUnit)
        {
            unit.GetComponent<SpriteRenderer>().color = new Color(0.75f, 0.75f, 0.75f);
        }
        if (battleSystemData.battleState == BattleState.SelectUnit && battleSystemData.curSelectUnit != null)
        {
            battleSystemData.curSelectUnit.GetComponent<SpriteRenderer>().color = Color.white;
        }
    }

    public void updateUI() 
    {
        for(int i = 0; i < acGrounp.childCount; i++) 
        {
            if (i < battleSystemData.playerUnit.curAC) 
            {
                
                acGrounp.GetChild(i).GetComponent<Image>().sprite = DataManager.instance.uiImageDataList.getData("1");
            }
            else
            {
                
                acGrounp.GetChild(i).GetComponent<Image>().sprite = DataManager.instance.uiImageDataList.getData("2");
            }
        }

        for (int i = 0; i < mpGrounp.childCount; i++)
        {
            if (i < battleSystemData.playerUnit.curMP)
            {
                mpGrounp.GetChild(i).GetComponent<Image>().sprite = DataManager.instance.uiImageDataList.getData("3");
            }
            else
            {
                mpGrounp.GetChild(i).GetComponent<Image>().sprite = DataManager.instance.uiImageDataList.getData("4");
            }
        }
    }
}
