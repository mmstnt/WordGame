using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattleSkillButton : MonoBehaviour
{
    public string skillID;

    public void Awake()
    {
        if (!string.IsNullOrEmpty(skillID)) 
        {
            initialize(skillID);
        }
    }

    public void initialize(string buttonSkillID) 
    {
        skillID = buttonSkillID;
        transform.GetComponentInChildren<TMP_Text>().text = DataManager.instance.skillDataList.getData(skillID).skillName;
    }
}
