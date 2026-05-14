using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager instance;

    [Header("資料清單")]
    public SpriteImageDataListSO characterImageDataList;
    public SpriteImageDataListSO backgroundImageDataList;
    public SpriteImageDataListSO uiImageDataList;
    public BattleDataListSO battleDataList;
    public UnitDataListSO unitDataList;
    public SkillDataListSO skillDataList;
    public DevelopEventDataListSO developEventDataList;
    public ProficiencyDataListSO proficiencyDataList;

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
