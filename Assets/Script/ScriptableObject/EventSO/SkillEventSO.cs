using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event/SkillEventSO")]
public class SkillEventSO : ScriptableObject
{
    public UnityAction<string> onEventRaised;

    public void raiseEvent(string skillID)
    {
        onEventRaised?.Invoke(skillID);
    }
}
