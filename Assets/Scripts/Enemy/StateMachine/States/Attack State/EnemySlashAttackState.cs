using UnityEngine;

public class EnemySlashAttackState : EnemyAttackState
{
    protected override string AnimationTrigger => "IsAttack1";

    public EnemySlashAttackState(EnemyCore core) : base(core) { }
}