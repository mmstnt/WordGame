using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event/AttributeImageEventSO")]
public class AttributeImageEventSO : ScriptableObject
{
    public UnityAction<AttributeImage> onEventRaised;

    public void raiseEvent(AttributeImage attributeImage)
    {
        onEventRaised?.Invoke(attributeImage);
    }
}
