using UnityEngine;

[CreateAssetMenu(menuName = "Data/UnitDataSO")]
public class UnitDataSO : ScriptableObject
{
    public Sprite image;

    public int strength;
    public int dexterity;
    public int constitution;
    public int intelligence;
    public int wisdom;
    public int charisma;
}
