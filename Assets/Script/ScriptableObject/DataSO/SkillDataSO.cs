using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/SkillDataSO")]
public class SkillDataSO : ScriptableObject
{
    public string skillName;
    public int damageDiceCount;
    public int damageDiceSide;
    public int damageModifier;

    public int AC;
    public int MP;

    public string[] effect;
}
