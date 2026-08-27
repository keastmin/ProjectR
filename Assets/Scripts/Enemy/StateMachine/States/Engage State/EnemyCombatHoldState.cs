using UnityEngine;

public class EnemyCombatHoldState : EnemyCombatMoveSubState
{
    protected override string AnimatorTrigger => "IsIdle";

    public EnemyCombatHoldState(EnemyCore core, EnemyEngageState parent) : base(core, parent) { }
}
