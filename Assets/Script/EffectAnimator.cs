using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectAnimator : MonoBehaviour
{
    public float CalculateDuration() 
    {
        float maxDuration = 0f;

        Animator anim = GetComponent<Animator>();
        ParticleSystem[] allParticles = GetComponentsInChildren<ParticleSystem>();

        if (anim != null && anim.runtimeAnimatorController != null)
        {
            // 取得所有的動畫片段
            AnimationClip[] clips = anim.runtimeAnimatorController.animationClips;

            foreach (AnimationClip clip in clips)
            {
                // 更新最長動畫時間
                if (clip.length > maxDuration)
                {
                    maxDuration = clip.length;
                }
            }
        }


        foreach (ParticleSystem ps in allParticles)
        {
            //抓取粒子最長持續時間
            float psTotalTime = ps.main.duration + ps.main.startLifetime.constantMax;

            if (psTotalTime > maxDuration)
            {
                maxDuration = psTotalTime;
            }
        }

        return maxDuration > 0 ? maxDuration : 0.5f;
    }
}
