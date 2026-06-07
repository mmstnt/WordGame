using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.InputSystem.PlayerInput;

public class DevelopManager : MonoBehaviour
{
    [Header("廣播")]
    public SkillEffectEventSO globalFadeEvent;

    [Header("監聽")]
    public StringEventSO developActionEvent;
    public VoidEventSO developButtonUpdataEvent;

    [Header("組件")]
    public Transform developInterface;
    public Transform proficiencyInterface;
    public Transform actionPointGroup;
    public Transform developGroup;
    public Transform selectEventNeedPointGrounp;
    public Image background;
    public Image selectEventImage;
    public TMP_Text developRoundTextGameObject;
    public TMP_Text actionTextGameObject;
    public TMP_Text selectEventNameGameObject;
    public TMP_Text selectEventDescriptionGameObject;
    public TMP_Text selectEventNeedGameObject;
    public GameObject actionPointGameObjecct;
    public GameObject developButtonGameObjecct;

    public DevelopMapDataSO developMapDataSO;

    public Transform curInterface;
    private GameObject curDevelopButton;

    private void Awake()
    {
        initialize();
    }

    private void OnEnable()
    {
        developActionEvent.onEventRaised += onDevelopActionEvent;
        developButtonUpdataEvent.onEventRaised += onDevelopButtonUpdataEvent;
    }

    private void OnDisable()
    {
        developActionEvent.onEventRaised -= onDevelopActionEvent;
        developButtonUpdataEvent.onEventRaised -= onDevelopButtonUpdataEvent;
    }

    public void initialize() 
    {
        curInterface = developInterface;
        background.sprite = DataManager.instance.backgroundImageDataList.getData("B00001");
        switchInterface(developInterface);
        UIUpdata();
    }

    private void onDevelopActionEvent(string actionEvent)
    {
        string selectEventID = "";
        curDevelopButton = EventSystem.current?.currentSelectedGameObject;
        if (curDevelopButton != null && curDevelopButton.TryGetComponent<DevelopButton>(out DevelopButton curSelectButton))
        {
            selectEventID = curSelectButton.DevelopEventID;
            DevelopEventDataSO selectEvent = DataManager.instance.developEventDataList.getData(selectEventID);

            if (selectEvent == null || selectEvent.actionPoint > DataManager.instance.playerData.developActionPoint)
                return;

            DataManager.instance.playerData.developActionPoint -= selectEvent.actionPoint;
            UIUpdata();
        }

        StartCoroutine(developAction(actionEvent));
    }

    private IEnumerator developAction(string actionEvent)
    {
        bool isAniFinish = false;
        globalFadeEvent.raiseEvent(null, Vector2.zero, () =>
        {
            isAniFinish = true;
        }
        );

        yield return new WaitUntil(() => isAniFinish);

        switch (actionEvent)
        {
            case "Exercise":
                switchInterface(proficiencyInterface);
                background.sprite = DataManager.instance.backgroundImageDataList.getData("B00002");
                break;
            case "Martial":
                background.sprite = DataManager.instance.backgroundImageDataList.getData("B00003");
                break;
            case "Research":
                background.sprite = DataManager.instance.backgroundImageDataList.getData("B00004");
                break;
            case "Craft":
                break;
            case "Livelihood":
                break;
            case "Store":
                break;
            case "Socializing":
                break;
            case "Rest":
                roundEnd();
                break;
            case "Back":
                switchInterface(developInterface);
                break;
            default:
                GameEventManager.instance.enterDialog(actionEvent);
                Debug.Log(actionEvent);
                break;
        }
    }

    public void UIUpdata() 
    {
        curDevelopButton = EventSystem.current?.currentSelectedGameObject;
        int round = DataManager.instance.playerData.developRound;
        int actionPoint = DataManager.instance.playerData.developActionPoint;
        int needActionPoint = 0;

        string developRoundText = $"啟明 {(round + 35) / 36 + 21}年 {((round + 2) / 3 % 12 == 0 ? 12 : (round + 2) / 3 % 12)}月 - {(round % 3 == 1 ? "初" : round % 3 == 2 ? "中" : "末")}";
        
        developRoundTextGameObject.text = developRoundText;
        actionTextGameObject.text = $"剩餘體力:{actionPoint}/5";

        selectEventImage.sprite = null;
        selectEventImage.color = Color.clear;
        selectEventNameGameObject.text = "";
        selectEventDescriptionGameObject.text = "";
        selectEventNeedGameObject.text = "";
        needActionPoint = 0;

        if (curDevelopButton != null && curDevelopButton.TryGetComponent<DevelopButton>(out DevelopButton curSelectButton))
        {
            DevelopEventDataSO selectEvent = DataManager.instance.developEventDataList.getData(curSelectButton.DevelopEventID);
            if(selectEvent != null) 
            {
                selectEventImage.sprite = selectEvent.image;
                selectEventImage.color = Color.white;
                selectEventNameGameObject.text = selectEvent.developEventName;
                selectEventDescriptionGameObject.text = selectEvent.description;
                selectEventNeedGameObject.text = "需求";
                needActionPoint = selectEvent.actionPoint;
            }
        }

        updataNeedPoint(needActionPoint);
        updataUIPoint(actionPointGroup, actionPoint, needActionPoint, "UI00011","UI00012","UI00013");
    }

    public void roundEnd() 
    {
        DataManager.instance.playerData.developRound += 1;
        DataManager.instance.playerData.developActionPoint = 5;
        UIUpdata();
    }

    public void onDevelopButtonUpdataEvent() 
    {
        UIUpdata();
    }

    private void switchInterface(Transform newInterface) 
    {
        curInterface.gameObject.SetActive(false);
        curInterface = newInterface;

        if (curInterface == developInterface) 
        {
            createDevelopButton();
        }

        newInterface.gameObject.SetActive(true);
    }

    private void updataUIPoint(Transform UIGrounp, int curPoint, int needPoint, string pointImageID, string nullPointImageID, string prePointImageID)
    {
        for (int i = 0; i < UIGrounp.childCount; i++)
        {
            Image point = UIGrounp.GetChild(i).GetComponent<Image>();
            if (i < curPoint)
            {
                if (i < (curPoint - needPoint))  
                {
                    point.sprite = DataManager.instance.uiImageDataList.getData(pointImageID);
                    point.color = Color.white;
                }
                else 
                {
                    point.sprite = DataManager.instance.uiImageDataList.getData(prePointImageID);
                    point.color = Color.white;
                }
            }
            else
            {
                point.sprite = DataManager.instance.uiImageDataList.getData(nullPointImageID);
                if (i >= needPoint) 
                {
                    point.color = Color.white;
                }
                else 
                {
                    point.color = new Color(1, 0.25f, 0.25f);
                }
            }
        }
    }

    private void updataNeedPoint(int point) 
    {
        clearUIGrounp(selectEventNeedPointGrounp);
        for (int i = 0; i < point; i++) 
        {
            Instantiate(actionPointGameObjecct, selectEventNeedPointGrounp);
        }
    }

    private void createDevelopButton() 
    {
        clearUIGrounp(developGroup);

        for (int i = 0; i < developMapDataSO.mapSiteList.Count; i++) 
        {
            GameObject developButton = Instantiate(developButtonGameObjecct, developGroup);
            developButton.GetComponent<RectTransform>().anchoredPosition = developMapDataSO.mapSiteList[i].site;
            developButton.GetComponent<DevelopButton>().initialize("DE00009");
            
            developButton.GetComponent<Button>().onClick.AddListener
            (
                delegate
                {
                    developActionEvent.onEventRaised("B");
                }
            );
        }
    }

    private void clearUIGrounp(Transform UIGrounp)
    {
        for (int i = UIGrounp.childCount - 1; i >= 0; i--)
        {
            GameObject UIGameObject = UIGrounp.GetChild(i).gameObject;
            UIGameObject.transform.SetParent(null);
            Destroy(UIGameObject);
        }
    }
}
