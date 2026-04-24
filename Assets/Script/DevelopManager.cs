using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DevelopManager : MonoBehaviour
{
    public TMP_Text developRoundTextGameObject;

    private void Awake()
    {
        UIUpdata();
    }

    public void UIUpdata() 
    {
        int round = DataManager.instance.playerData.developRound;
        string developRoundText = $"啟明 {(round + 35) / 36 + 21}年 {((round + 2) / 3 % 12 == 0 ? 12 : (round + 2) / 3 % 12)}月 - {(round % 3 == 1 ? "初" : round % 3 == 2 ? "中" : "末")}";
        developRoundTextGameObject.text = developRoundText;
    }
}
