using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyAttackNoticeState : EnemyStateBase
{
    public EnemyAttackNoticeState(EnemyCore core) : base(core)
    {

    }

    public override void Enter()
    {
        // 공격 알림 VFX 재생
        Core.AnimationEvent.AttackNoticeVFX();

        // 애니메이션 재생
        Core.Animator.SetTrigger("IsIdle");
    }

    public override void Exit()
    {
        // 애니메이션 초기화
        Core.Animator.ResetTrigger("IsIdle");
    }
}