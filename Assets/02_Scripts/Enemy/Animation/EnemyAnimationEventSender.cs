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

    public void OnSwordTrailEffectActive()
    {
        _animEvent.SwordTrailEffectActive(true);
    }

    public void OnSwordTrailEffectDeactive()
    {
        _animEvent.SwordTrailEffectActive(false);
    }

    public void OnAttackNotice()
    {
        _animEvent.AttackNotice();
    }

    public void OnAttackWindowOn()
    {
        _animEvent.AttackWindowActive(true);
    }

    public void OnAttackWindowOff()
    {
        _animEvent.AttackWindowActive(false);
    }
}
