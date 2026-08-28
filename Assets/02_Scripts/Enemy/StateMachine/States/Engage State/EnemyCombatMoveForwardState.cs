using UnityEngine;

public class EnemyCombatMoveForwardState : EnemyCombatMoveSubState
{
    protected override string AnimatorTrigger => "IsMoveForward";

    public EnemyCombatMoveForwardState(EnemyCore core, EnemyEngageState parent) : base(core, parent) { }
}
