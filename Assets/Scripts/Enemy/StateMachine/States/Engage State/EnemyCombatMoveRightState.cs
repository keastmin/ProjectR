using UnityEngine;

public class EnemyCombatMoveRightState : EnemyCombatMoveSubState
{
    protected override string AnimatorTrigger => "IsMoveRight";

    public EnemyCombatMoveRightState(EnemyCore core, EnemyEngageState parent) : base(core, parent) { }
}
