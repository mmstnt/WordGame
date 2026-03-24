using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BattleButton : MonoBehaviour, ISelectHandler, IPointerEnterHandler
{
    [Header("¼s¼½")]
    public VoidEventSO battleButtonUpdataEvent;

    public void OnSelect(BaseEventData eventData)
    {
        if (battleButtonUpdataEvent != null) 
        {
            battleButtonUpdataEvent.raiseEvent();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (battleButtonUpdataEvent != null) 
        {
            EventSystem.current.SetSelectedGameObject(this.gameObject);
        }
    }
}
