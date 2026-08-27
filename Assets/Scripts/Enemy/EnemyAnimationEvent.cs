using System;
using UnityEngine;

public class EnemyAnimationEvent : MonoBehaviour
{
    public event Action<AnimationEvent> OnAnimationEnd;

    public void AnimationEndActionInvoke(AnimationEvent animationEvent)
    {
        OnAnimationEnd?.Invoke(animationEvent);
    }
}
