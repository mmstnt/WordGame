using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event/SkillEffectEventSO")]
public class SkillEffectEventSO : ScriptableObject
{
    public UnityAction<SkillDataSO, Vector3, UnityAction> onEventRaised;

    public void raiseEvent(SkillDataSO skill, Vector3 pos, UnityAction onComplete)
    {
        onEventRaised?.Invoke(skill, pos, onComplete);
    }
}
