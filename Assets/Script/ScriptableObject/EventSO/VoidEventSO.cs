using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event/VoidEventSO")]
public class VoidEventSO : ScriptableObject
{
    public UnityAction onEventRaised;

    public void raiseEvent()
    {
        onEventRaised?.Invoke();
    }
}
