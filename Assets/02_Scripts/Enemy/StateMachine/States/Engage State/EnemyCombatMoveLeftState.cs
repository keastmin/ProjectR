using UnityEngine;

public class EnemyCombatMoveLeftState : EnemyCombatMoveSubState
{
    protected override string AnimatorTrigger => "IsMoveLeft";

    public EnemyCombatMoveLeftState(EnemyCore core, EnemyEngageState parent) : base(core, parent) { }
}
