using System;
using UnityEngine;

public class EnemyAnimationEvent : MonoBehaviour
{
    public event Action<AnimationEvent> OnAnimationEnd;
    public event Action<EnemyAttackSO> OnAttack;

    public void AnimationEndActionInvoke(AnimationEvent animationEvent)
    {
        OnAnimationEnd?.Invoke(animationEvent);
    }

    public void AttackActionInvoke(EnemyAttackSO enemyAttackSO)
    {
        OnAttack?.Invoke(enemyAttackSO);
    }
}