using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DevelopButton : MonoBehaviour
{
    public string DevelopEventID;

    public void Awake()
    {
        if (!string.IsNullOrEmpty(DevelopEventID))
        {
            initialize(DevelopEventID);
        }
    }

    public void initialize(string buttonSkillID)
    {
        DevelopEventID = buttonSkillID;
    }
}
