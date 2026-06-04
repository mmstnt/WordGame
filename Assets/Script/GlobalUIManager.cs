using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;
using static UnityEditor.PlayerSettings;

public class GlobalUIManager : MonoBehaviour
{
    [Header("∫ ≈•")]
    public SkillEffectEventSO globalFadeEvent;


    [Header("≤’•Û")]
    public GameObject globalFadeAin;

    private void OnEnable()
    {
        globalFadeEvent.onEventRaised += onGlobalFadeEvent;
    }

    private void OnDisable()
    {
        globalFadeEvent.onEventRaised -= onGlobalFadeEvent;
    }

    private void onGlobalFadeEvent(SkillDataSO skill, Vector3 pos, UnityAction onComplete)
    {
        StartCoroutine(globalFade(onComplete));
    }

    private IEnumerator globalFade(UnityAction onComplete)
    {
        float duration = globalFadeAin.GetComponent<EffectAnimator>().CalculateDuration() / 2;
        globalFadeAin.GetComponent<Animator>().Play("GlobalFade", -1, 0);

        yield return new WaitForSeconds(duration);

        onComplete?.Invoke();
    }
}
