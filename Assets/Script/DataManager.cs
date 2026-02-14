using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager instance;

    [Header("資料清單")]
    public CharacterImageDataListSO characterImageDataList;
    public CharacterImageDataListSO backgroundImageDataList;
    public BattleDataListSO battleDataList;
    public UnitDataListSO unitDataList;
    public SkillDataListSO skillDataList;

    [Header("資料")]
    public PlayerDataSO playerData;
    public PlayerDataSO basePlayerData;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);

        newPlayerData();
    }

    public void newPlayerData() 
    {
        playerData = Instantiate(basePlayerData);
    }
}
