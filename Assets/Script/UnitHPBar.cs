using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitHPBar : MonoBehaviour
{
    [SerializeField]
    public Vector2 offset;

    private Slider selfBar;

    private void Awake()
    {
        selfBar = GetComponent<Slider>();
    }

    public void initialize(Unit unitTarget)
    {
        transform.position = unitTarget.transform.position + Vector3.up * offset.y + Vector3.right * offset.x;
        changeHP(unitTarget);
    }

    public void changeHP(Unit unitTarget)
    {
            selfBar.value = unitTarget.curHP * 1.0f / unitTarget.maxHP;
    }
}
