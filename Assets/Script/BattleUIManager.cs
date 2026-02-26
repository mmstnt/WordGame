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
    public VoidEventSO setUnitColorEvent;
    public VoidEventSO selectBackEvent;
    public VoidEventSO enterSelectEvent;
    public VoidEventSO WaitActionReactivateEvent;
    public IntEventSO recordLastButtonEvent;
    public StringEventSO switchSkillGroupEvent;

    [Header("資料")]
    public BattleSystemDataSO battleSystemData;

    [Header("UI")]
    public Transform actionGroup;
    public Transform skillGroup;

    [Header("組件")]
    public GameObject skillButtonGameObject;

    private Transform curUI;
    private int lastActionButtonIndex;
    private int lastSkillButtonIndex;

    private void Awake()
    {
        curUI = actionGroup;
    }

    private void OnEnable()
    {
        setUnitColorEvent.onEventRaised += setUnitColor;
        selectBackEvent.onEventRaised += onSelectBackEvent;
        enterSelectEvent.onEventRaised += onEnterSelect;
        WaitActionReactivateEvent.onEventRaised += onWaitActionReactivateEvent;
        recordLastButtonEvent.onEventRaised += onRecordLastButtonEvent;
        switchSkillGroupEvent.onEventRaised += onSwitchSkillGroupEvent;
    }

    private void OnDisable()
    {
        setUnitColorEvent.onEventRaised -= setUnitColor;
        selectBackEvent.onEventRaised -= onSelectBackEvent;
        enterSelectEvent.onEventRaised -= onEnterSelect;
        WaitActionReactivateEvent.onEventRaised -= onWaitActionReactivateEvent;
        recordLastButtonEvent.onEventRaised -= onRecordLastButtonEvent;
        switchSkillGroupEvent.onEventRaised -= onSwitchSkillGroupEvent;
    }

    public void onEnterSelect()
    {
        //選擇單位模式
        curUI.gameObject.SetActive(false);
    }

    public void onSelectBackEvent()
    {
        if (battleSystemData.battleState == BattleState.Select)
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

    public void onWaitActionReactivateEvent() 
    { 
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
        if (battleSystemData.battleState == BattleState.Select && battleSystemData.curSelectUnit != null)
        {
            battleSystemData.curSelectUnit.GetComponent<SpriteRenderer>().color = Color.white;
        }
    }
}
