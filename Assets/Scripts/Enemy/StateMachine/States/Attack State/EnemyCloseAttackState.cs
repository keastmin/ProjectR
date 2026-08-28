using UnityEngine;

public class EnemyCloseAttackState : EnemyAttackState
{
    protected override string AnimationTrigger => "IsCloseAttack";

    public EnemyCloseAttackState(EnemyCore core) : base(core) { }

    public override void AnimatorTick()
    {
        AnimDeltaPos += (Core.Animator.deltaPosition * 1.7f);
    }
}