using UnityEngine;

public class PlayerDodgeAttackLoopState : PlayerDodgeAttackState
{
    protected override DirectorID AttackDirectorID => DirectorID.DodgeAttackStart;

    protected override string AnimationTrigger => "IsDodgeAttack";

    protected override PlayerStateBase NextState => throw new System.NotImplementedException();

    public PlayerDodgeAttackLoopState(PlayerCore core) : base(core)
    {
    }
}
