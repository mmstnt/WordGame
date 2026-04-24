using UnityEngine;

[CreateAssetMenu(menuName = "Data/PlayerDataSO")]
public class PlayerDataSO : BaseUnitSO
{
    public string[] curMartial;
    public string[] curMagic;
    public string[] item;
    
    [Header("¾i¦¨¸ê°T")]
    public int developRound;
}
