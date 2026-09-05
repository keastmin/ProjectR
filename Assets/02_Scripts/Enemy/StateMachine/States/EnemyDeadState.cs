using UnityEngine;

public class EnemyDeadState : EnemyStateBase
{
    public EnemyDeadState(EnemyCore core) : base(core) { }

    public override void Enter()
    {
        // 애니메이션 재생
        Core.Animator.SetTrigger("IsDead");

        Core.DeadStart();
    }

    public override void UpdateTick()
    {
        
    }

    public override void FixedTick()
    {
        Core.Mover.Move(AnimDeltaPos / Time.fixedDeltaTime);
        AnimDeltaPos = Vector3.zero;
    }

    public override void AnimatorTick()
    {
        AnimDeltaPos += Core.Animator.deltaPosition;
    }

    public override void Exit()
    {
        
    }
}