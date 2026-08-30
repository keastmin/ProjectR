using UnityEngine;

public class PlayerDodgeAttackEndState : PlayerDodgeAttackState
{
    protected override DirectorID AttackDirectorID => DirectorID.DodgeAttackStart;

    protected override string AnimationTrigger => "IsIdle";

    protected override PlayerStateBase NextState => throw new System.NotImplementedException();

    public PlayerDodgeAttackEndState(PlayerCore core) : base(core)
    {
    }
}
