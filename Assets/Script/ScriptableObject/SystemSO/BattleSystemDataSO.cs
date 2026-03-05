using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "System/BattleSystemDataSO")]
public class BattleSystemDataSO : ScriptableObject
{
    [Header("玩家戰鬥資料")]
    public PlayerDataSO playerBattleData;

    [Header("戰鬥狀態")]
    public BattleState battleState;
    public Unit curSelectUnit;
    public int curRound;

    [Header("單位")]
    public Unit[] enemyUnit;
    public Unit[] playerUnit;
    public List<unitActionPoint> unitSpeedList;
    public List<Unit> preUnitSpeedList;

    [System.Serializable]
    public class unitActionPoint
    {
        public Unit unit;
        public float actionPoint;
    }

    public void initialize()
    {
        playerBattleData = null;
        battleState = BattleState.Ready;
        curRound = 1;
        enemyUnit = null;
        playerUnit = null;
        unitSpeedList = new List<unitActionPoint>();
    }
}
