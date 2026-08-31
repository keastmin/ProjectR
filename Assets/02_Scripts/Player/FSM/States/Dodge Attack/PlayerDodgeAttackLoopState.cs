using UnityEngine;

public class PlayerDodgeAttackLoopState : PlayerDodgeAttackState
{
    protected override DirectorID AttackDirectorID => DirectorID.DodgeAttackLoop;

    protected override string AnimationTrigger => "IsDodgeAttack";

    protected override PlayerStateBase NextState => Core.StateMachine.DodgeAttackEndState;

    public PlayerDodgeAttackLoopState(PlayerCore core) : base(core)
    {
    }
}
