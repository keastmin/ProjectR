using UnityEngine;

public class EnemyIdleState : EnemyStateBase
{
    public EnemyIdleState(EnemyCore enemy) : base(enemy)
    {

    }

    public override void Enter()
    {
        Core.Animator.SetTrigger("IsIdle");
    }

    public override void FixedTick()
    {
        
    }

    public override void Exit()
    {
        Core.Animator.ResetTrigger("IsIdle");
    }
}