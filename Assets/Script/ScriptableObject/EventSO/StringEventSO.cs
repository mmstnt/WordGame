using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event/StringEventSO")]
public class StringEventSO : ScriptableObject
{
    public UnityAction<string> onEventRaised;

    public void raiseEvent(string str)
    {
        onEventRaised?.Invoke(str);
    }
}
