using UnityEngine;

public abstract class PlayerDodgeState : PlayerStateBase
{
    protected abstract string AnimationTrigger { get; }

    public PlayerDodgeState(PlayerCore core) : base(core) { }

    public override void Enter()
    {
        // 애니메이션 재생
        Core.Animator.SetTrigger(AnimationTrigger);

        base.Enter();
    }

    public override void FixedTick()
    {
        if (IsDamaged)
            return;

        Core.Mover.Move(AnimDeltaPos / Time.fixedDeltaTime);
        AnimDeltaPos = Vector3.zero;
    }

    public override void AnimatorTick()
    {
        if (IsDamaged)
            return;

        AnimDeltaPos += Core.Animator.deltaPosition;
    }

    public override void Exit()
    {
        // 애니메이션 트리거 리셋
        Core.Animator.ResetTrigger(AnimationTrigger);

        base.Exit();
    }
}