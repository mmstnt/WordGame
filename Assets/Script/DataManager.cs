using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager instance;

    [Header("¸ê®Æ")]
    public CharacterImageDataListSO characterImageDataList;
    public BattleDataListSO battleDataList;
    public UnitDataListSO unitDataList;
    public SkillDataListSO skillDataList;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }
}
