using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/PlayerDataSO")]
public class PlayerDataSO : ScriptableObject
{
    public Sprite image;

    public int strength;
    public int dexterity;
    public int constitution;
    public int intelligence;
    public int wisdom;
    public int charisma;

    public string[] curMartial;
    public string[] curMagic;
    public string[] item;
}
