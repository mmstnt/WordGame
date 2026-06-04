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

    private Dictionary<int, Dictionary<string, string[]>> developEventDic = new Dictionary<int, Dictionary<string, string[]>>();

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

    private void initialize() 
    {
        Dictionary<int, List<DevelopEventDataSO>> timeEvent = new Dictionary<int, List<DevelopEventDataSO>>();
        Dictionary<string, DevelopEventDataSO> placeEvent = new Dictionary<string, DevelopEventDataSO>();

        foreach(var developEvent in developEventDataList.dataList) 
        {
            if (timeEvent.ContainsKey(developEvent.data.round)) 
            {
                timeEvent[developEvent.data.round].Add(developEvent.data);
            }
            else 
            {
                timeEvent[developEvent.data.round] = new List<DevelopEventDataSO> { developEvent.data };
            }
            //developEventDic[developEvent.data.round] = ;
        }


    }
}
