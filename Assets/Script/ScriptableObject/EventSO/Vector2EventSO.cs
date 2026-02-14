using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event/Vector2EventSO")]
public class Vector2EventSO : ScriptableObject
{
    public UnityAction<Vector2> onEventRaised;

    public void raiseEvent(Vector2 vector2)
    {
        onEventRaised?.Invoke(vector2);
    }
}
