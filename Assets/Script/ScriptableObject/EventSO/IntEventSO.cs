using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event/IntEventSO")]
public class IntEventSO : ScriptableObject
{
    public UnityAction<int> onEventRaised;

    public void raiseEvent(int i)
    {
        onEventRaised?.Invoke(i);
    }
}
