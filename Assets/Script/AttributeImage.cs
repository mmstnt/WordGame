using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AttributeImage : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("¼s¼½")]
    public AttributeImageEventSO showAttributeSourceUIEvent;
    public VoidEventSO closeAttributeSourceUIEvent;

    [Header("°Ñ¼Æ")]
    public Attribute attributeType;

    private bool isCurAttribute = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        isCurAttribute = true;
        showAttributeSourceUIEvent.onEventRaised(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isCurAttribute) return;

        closeAttributeSourceUIEvent.raiseEvent();
        isCurAttribute = false;
    }
}
