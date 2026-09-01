using UnityEngine;

public abstract class EnemyCombatMoveSubState : EnemyStateBase
{
    protected EnemyEngageState Parent { get; }

    protected abstract string AnimatorTrigger { get; }
    
    protected EnemyCombatMoveSubState(EnemyCore core, EnemyEngageState parent) : base(core)
    {
        Parent = parent;
    }

    public override void Enter()
    {
        Core.Animator.SetTrigger(AnimatorTrigger);
    }

    public override void FixedTick()
    {
        Vector3 velocity = AnimDeltaPos / Time.fixedDeltaTime;
        Core.Mover.Move(Core.AdjustPositioningMovement(velocity));
        AnimDeltaPos = Vector3.zero;
    }

    public override void AnimatorTick()
    {
        AnimDeltaPos += Core.Animator.deltaPosition;
    }

    public override void Exit()
    {
        Core.Animator.ResetTrigger(AnimatorTrigger);
        base.Exit();
    }
}
