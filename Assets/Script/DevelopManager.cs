using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DevelopManager : MonoBehaviour
{
    [Header("監聽")]
    public StringEventSO developActionEvent;

    public TMP_Text developRoundTextGameObject;

    private void Awake()
    {
        UIUpdata();
    }

    private void OnEnable()
    {
        developActionEvent.onEventRaised += onDevelopActionEvent;
    }

    private void OnDisable()
    {
        developActionEvent.onEventRaised -= onDevelopActionEvent;
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
                Debug.Log(actionEvent);
                break;
        }
    }

    public void UIUpdata() 
    {
        int round = DataManager.instance.playerData.developRound;
        string developRoundText = $"啟明 {(round + 35) / 36 + 21}年 {((round + 2) / 3 % 12 == 0 ? 12 : (round + 2) / 3 % 12)}月 - {(round % 3 == 1 ? "初" : round % 3 == 2 ? "中" : "末")}";
        developRoundTextGameObject.text = developRoundText;
    }
}
