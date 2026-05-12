using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DevelopManager : MonoBehaviour
{
    [Header("監聽")]
    public StringEventSO developActionEvent;
    public VoidEventSO developButtonUpdataEvent;

    [Header("組件")]
    public Transform actionPointGrounp;
    public Transform selectEventNeedPointGrounp;
    public Image selectEventImage;
    public TMP_Text developRoundTextGameObject;
    public TMP_Text actionTextGameObject;
    public TMP_Text selectEventNameGameObject;
    public TMP_Text selectEventDescriptionGameObject;
    public TMP_Text selectEventNeedGameObject;
    public GameObject actionPointGameObjecct;

    private void Awake()
    {
        UIUpdata();
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

    private void onDevelopActionEvent(string actionEvent)
    {
        switch (actionEvent) 
        {
            case "Exercise":
                Debug.Log(actionEvent);
                break;
            case "Martial":
                Debug.Log(actionEvent);
                break;
            case "Research":
                Debug.Log(actionEvent);
                break;
            case "Craft":
                Debug.Log(actionEvent);
                break;
            case "Livelihood":
                Debug.Log(actionEvent);
                break;
            case "Store":
                Debug.Log(actionEvent);
                break;
            case "Socializing":
                Debug.Log(actionEvent);
                break;
            case "Rest":
                roundEnd();
                Debug.Log(actionEvent);
                break;
        }
    }

    public void UIUpdata() 
    {
        GameObject curDevelopButton = EventSystem.current?.currentSelectedGameObject;
        int round = DataManager.instance.playerData.developRound;
        int actionPoint = DataManager.instance.playerData.developActionPoint;
        int needActionPoint = 0;

        string developRoundText = $"啟明 {(round + 35) / 36 + 21}年 {((round + 2) / 3 % 12 == 0 ? 12 : (round + 2) / 3 % 12)}月 - {(round % 3 == 1 ? "初" : round % 3 == 2 ? "中" : "末")}";
        
        developRoundTextGameObject.text = developRoundText;
        actionTextGameObject.text = $"剩餘體力:{actionPoint}/5";

        if (curDevelopButton != null && curDevelopButton.TryGetComponent<DevelopButton>(out DevelopButton curSelectButton))
        {
            DevelopEventDataSO selectEvent = DataManager.instance.developEventDataList.getData(curSelectButton.DevelopEventID);
            selectEventImage.sprite = selectEvent.image;
            selectEventImage.color = Color.white;
            selectEventNameGameObject.text = selectEvent.developEventName;
            selectEventDescriptionGameObject.text = selectEvent.description;
            selectEventNeedGameObject.text = "需求";
            needActionPoint = selectEvent.actionPoint;
        }
        else 
        {
            selectEventImage.sprite = null;
            selectEventImage.color = Color.clear;
            selectEventNameGameObject.text = "";
            selectEventDescriptionGameObject.text = "";
            selectEventNeedGameObject.text = "";
            needActionPoint = 0;
        }

        updataNeedPoint(needActionPoint);
        updataUIPoint(actionPointGrounp, actionPoint, needActionPoint, "UI00011","UI00012","UI00013");
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
