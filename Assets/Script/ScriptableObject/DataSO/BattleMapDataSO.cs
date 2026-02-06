using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/BattleMapDataSO")]
public class BattleMapDataSO : ScriptableObject
{
    public Sprite image;
    public List<Vector2> unitSite;
}
