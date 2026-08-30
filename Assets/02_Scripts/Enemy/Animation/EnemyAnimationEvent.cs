using System;
using Tiny;
using UnityEngine;

public class EnemyAnimationEvent : MonoBehaviour
{
    [SerializeField] private Trail _swordTrailEffect;
    [SerializeField] private Transform _attackNoticePoint;
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

    public void SwordTrailEffectActive(bool isActive)
    {
        _swordTrailEffect.enabled = isActive;
    }

    public void AttackNotice()
    {
        Transform spawnPoint = _attackNoticePoint != null ? _attackNoticePoint : transform;
        CombatEffectRequestBus.Request(CombatEffectID.AttackNoticeEffect, spawnPoint);
    }

    public void AttackWindowActive(bool isActive)
    {

    }
}