using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BattleUIManager : MonoBehaviour
{
    [Header("監聽")]
    public VoidEventSO enterSelectEvent;

    [Header("UI")]
    public Transform actionGroup;
    public Transform skillGroup;


    private Transform curUI;

    private void Awake()
    {
        curUI = actionGroup;
    }

    private void OnEnable()
    {
        enterSelectEvent.onEventRaised += onEnterSelect;
    }

    private void OnDisable()
    {
        enterSelectEvent.onEventRaised -= onEnterSelect;
    }

    public void onEnterSelect()
    {
        //選擇單位模式
        curUI.gameObject.SetActive(false);
        //battleState = BattleState.Select;
    }
}
