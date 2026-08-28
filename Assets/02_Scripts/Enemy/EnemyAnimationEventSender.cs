using System;
using UnityEngine;
using UnityEngine.Scripting;

public class EnemyAnimationEventSender : MonoBehaviour
{
    [SerializeField] private EnemyAnimationEvent _animEvent;

    public void OnAnimationEnd(AnimationEvent animationEvent)
    {
        _animEvent.AnimationEndActionInvoke(animationEvent);
    }

    public void OnAttack(EnemyAttackSO enemyAttackSO)
    {
        _animEvent.AttackActionInvoke(enemyAttackSO);
    }
}
