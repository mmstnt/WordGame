using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectAnimator : MonoBehaviour
{
    public float CalculateDuration() 
    {
        Animator anim = GetComponent<Animator>();
        float maxDuration = 0f;

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

        return maxDuration > 0 ? maxDuration : 0.5f;
    }
}
