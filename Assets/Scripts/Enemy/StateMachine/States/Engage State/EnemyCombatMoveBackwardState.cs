using UnityEngine;

public class EnemyCombatMoveBackwardState : EnemyCombatMoveSubState
{
    protected override string AnimatorTrigger => "IsMoveBackward";

    public EnemyCombatMoveBackwardState(EnemyCore core, EnemyEngageState parent) : base(core, parent) { }
}
