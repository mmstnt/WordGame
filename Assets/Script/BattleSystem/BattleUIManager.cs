using System.Collections;
using UnityEngine;
using UnityEngine.Events;
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
    public VoidEventSO enterSelectUnitEvent;
    public VoidEventSO waitActionReactivateEvent;
    public VoidEventSO battleButtonUpdataEvent;
    public VoidEventSO battleResultEvent;
    public IntEventSO recordLastButtonEvent;
    public StringEventSO switchSkillGroupEvent;
    public StringEventSO nextRoundEvent;
    public SkillEffectEventSO skillEffectEvent;

    [Header("資料")]
    public BattleSystemDataSO battleSystemData;

    [Header("UI群組")]
    public Transform actionGroup;
    public Transform skillGroup;
    public Transform acGrounp;
    public Transform mpGrounp;
    public Transform preACGrounp;
    public Transform preMPGrounp;
    public Transform actionOrderGroup;
    public Transform effectGroup;

    [Header("組件")]
    public GameObject skillButtonGameObject;
    public GameObject pointGameObject;
    public GameObject characterSpeedBoxGameObject;

    private Transform curUI;
    private int lastActionButtonIndex;
    private int lastSkillButtonIndex;

    private void OnEnable()
    {
        battleUIInitializeEvent.onEventRaised += battleUIInitialize;
        setUnitColorEvent.onEventRaised += setUnitColor;
        selectBackEvent.onEventRaised += onSelectBackEvent;
        enterSelectUnitEvent.onEventRaised += onEnterSelectUnitEvent;
        waitActionReactivateEvent.onEventRaised += onWaitActionReactivateEvent;
        battleButtonUpdataEvent.onEventRaised += onBattleButtonUpdataEvent;
        battleResultEvent.onEventRaised += onBattleResultEvent;
        recordLastButtonEvent.onEventRaised += onRecordLastButtonEvent;
        switchSkillGroupEvent.onEventRaised += onSwitchSkillGroupEvent;
        nextRoundEvent.onEventRaised += onNextRoundEvent;
        skillEffectEvent.onEventRaised += onSkillEffectEvent;
    }

    private void OnDisable()
    {
        battleUIInitializeEvent.onEventRaised -= battleUIInitialize;
        setUnitColorEvent.onEventRaised -= setUnitColor;
        selectBackEvent.onEventRaised -= onSelectBackEvent;
        enterSelectUnitEvent.onEventRaised -= onEnterSelectUnitEvent;
        waitActionReactivateEvent.onEventRaised -= onWaitActionReactivateEvent;
        battleButtonUpdataEvent.onEventRaised -= onBattleButtonUpdataEvent;
        battleResultEvent.onEventRaised -= onBattleResultEvent;
        recordLastButtonEvent.onEventRaised -= onRecordLastButtonEvent;
        switchSkillGroupEvent.onEventRaised -= onSwitchSkillGroupEvent;
        nextRoundEvent.onEventRaised -= onNextRoundEvent;
        skillEffectEvent.onEventRaised -= onSkillEffectEvent;
    }

    public void battleUIInitialize() 
    {
        clearUIGrounp(acGrounp);
        clearUIGrounp(mpGrounp);

        curUI = actionGroup;
        //初始化玩家血條
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
    }

    public void onEnterSelectUnitEvent()
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

                onWaitActionReactivateEvent();
                break;
        }
    }

    public void onBattleButtonUpdataEvent()
    {
        updateUI();
    }

    public void onWaitActionReactivateEvent()
    {
        StartCoroutine(waitActionReactivate()); 
    }

    public IEnumerator waitActionReactivate()
    {
        if(battleSystemData.curActionUnit.unitData is PlayerDataSO) 
        {
            curUI.GetComponent<CanvasGroup>().interactable = false;
            curUI.gameObject.SetActive(true);

            yield return new WaitForEndOfFrame();

            curUI.GetComponent<CanvasGroup>().interactable = true;

            //判斷是哪個UI
            int index = 0;
            if (curUI == actionGroup)
                index = lastActionButtonIndex;
            else if (curUI == skillGroup)
                index = lastSkillButtonIndex;

            if (curUI.childCount > index)
            {
                //獲取上個按鈕
                EventSystem.current.SetSelectedGameObject(curUI.transform.GetChild(index).gameObject);
                GameObject curSkillButton = EventSystem.current?.currentSelectedGameObject;
            }
            else if (curUI.childCount > 0)
            {
                //獲取UI子物件第一個按鈕
                EventSystem.current.SetSelectedGameObject(curUI.transform.GetChild(0).gameObject);
            }
        }

        updateUI();
    }

    public void onSkillEffectEvent(SkillDataSO skill, Vector3 pos, UnityAction onComplete) 
    {
        StartCoroutine(SkillEffectCoroutine(skill, pos, onComplete));
    }

    public IEnumerator SkillEffectCoroutine(SkillDataSO skill, Vector3 pos, UnityAction onComplete)
    {
        //有動畫則先播放
        if (skill.skillAni != null)
        {
            GameObject skillEffect = Instantiate(skill.skillAni, pos, Quaternion.identity, effectGroup);

            //計算動畫播放時間
            float duration = skillEffect.GetComponent<EffectAnimator>().CalculateDuration();

            yield return new WaitForSeconds(duration);

            Destroy(skillEffect);
        }

        //讓戰鬥管理器解除等待
        onComplete?.Invoke();
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
        //創建技能按鈕
        for (int i = 0; i < skillList.Length; i++)
        {
            int index = i;
            int x = (i % 4) * 260;
            int y = (i / 4) * -130;
            Vector2 site = new Vector2(-390 + x, 120 + y);
            string skillID = skillList[i];

            GameObject skillButton = Instantiate(skillButtonGameObject, skillGroup);
            skillButton.GetComponent<RectTransform>().anchoredPosition = site;
            skillButton.GetComponent<ButtonUpdata>().buttonUpdataEvent = battleButtonUpdataEvent;
            skillButton.GetComponent<BattleSkillButton>().initialize(skillID);
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

    public void onBattleResultEvent()
    {
        curUI.gameObject.SetActive(false);

    }

    public void setUnitColor()
    {
        foreach (var unit in battleSystemData.enemyUnit)
        {
            if (unit != null) 
            unit.GetComponent<SpriteRenderer>().color = new Color(0.75f, 0.75f, 0.75f);
        }
        if (battleSystemData.battleState == BattleState.SelectUnit && battleSystemData.curSelectUnit != null)
        {
            battleSystemData.curSelectUnit.GetComponent<SpriteRenderer>().color = Color.white;
        }
    }

    public void updateUI() 
    {
        updataUIPoint(acGrounp, battleSystemData.playerUnit.curAC, "UI00001", "UI00002");
        updataUIPoint(mpGrounp, battleSystemData.playerUnit.curMP, "UI00004", "UI00005");

        preUI();
        actionOrderUI();
    }

    public void preUI() 
    {
        clearUIGrounp(preACGrounp);
        clearUIGrounp(preMPGrounp);

        GameObject curSkillButton = EventSystem.current?.currentSelectedGameObject;
        if (curSkillButton != null && curSkillButton.TryGetComponent<BattleSkillButton>(out BattleSkillButton curSelectSkill)) 
        {
            int needAC = DataManager.instance.skillDataList.getData(curSelectSkill.skillID).AC;
            int needMP = DataManager.instance.skillDataList.getData(curSelectSkill.skillID).MP;
            updataUIPrePoint(preACGrounp, battleSystemData.playerUnit.curAC, needAC, "UI00003");
            updataUIPrePoint(preMPGrounp, battleSystemData.playerUnit.curMP, needMP, "UI00006");
        }
    }

    public void actionOrderUI() 
    {
        clearUIGrounp(actionOrderGroup);

        for(int i= battleSystemData.preUnitSpeedList.Count - 1; i >= 0; i--) 
        {
            GameObject characterSpeedBox = Instantiate(characterSpeedBoxGameObject, actionOrderGroup);
            BaseUnitSO unitSO = battleSystemData.preUnitSpeedList[i].unitData;
            Sprite box = null, mask = null;

            //取得頭像
            switch (battleSystemData.preUnitSpeedList[i].faction) 
            {
                case BattleFaction.Player:

                    box = DataManager.instance.uiImageDataList.getData("UI00007");
                    mask = DataManager.instance.uiImageDataList.getData("UI00008");
                    break;
                case BattleFaction.Enemy:

                    box = DataManager.instance.uiImageDataList.getData("UI00009");
                    mask = DataManager.instance.uiImageDataList.getData("UI00010");
                    break;
            }
            Sprite headImage = battleSystemData.preUnitSpeedList[i].unitData.image;
            Color color = (i == 0) ? Color.white : new Color(0.5f, 0.5f, 0.5f);

            characterSpeedBox.GetComponent<CharacterSpeedBox>().initialize(box, mask, headImage, color);
        }
    }

    private void clearUIGrounp(Transform UIGroup) 
    {
        for (int i = UIGroup.childCount - 1; i >= 0; i--)
        {
            GameObject UIGameObject = UIGroup.GetChild(i).gameObject;
            UIGameObject.transform.SetParent(null);
            Destroy(UIGameObject);
        }
    }

    private void updataUIPoint(Transform UIGrounp, int curPoint, string pointImageID, string nullpointImageID) 
    {
        for (int i = 0; i < UIGrounp.childCount; i++)
        {
            if (i < curPoint)
            {
                UIGrounp.GetChild(i).GetComponent<Image>().sprite = DataManager.instance.uiImageDataList.getData(pointImageID);
            }
            else
            {
                UIGrounp.GetChild(i).GetComponent<Image>().sprite = DataManager.instance.uiImageDataList.getData(nullpointImageID);
            }
        }
    }

    private void updataUIPrePoint(Transform UIGrounp, int curPoint, int needPoint, string pointImageID)
    {
        for (int i = 0; i < needPoint; i++)
        {
            Image prePointImage = Instantiate(pointGameObject, UIGrounp).GetComponent<Image>();
            prePointImage.sprite = DataManager.instance.uiImageDataList.getData(pointImageID);
            if (curPoint < needPoint && i >= curPoint)  
            {
                prePointImage.color = new Color(0.5f, 0f, 0f);
            }
        }
    }
}
