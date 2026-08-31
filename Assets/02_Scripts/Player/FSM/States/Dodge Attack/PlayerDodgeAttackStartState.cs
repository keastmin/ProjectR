using UnityEngine;

public class PlayerDodgeAttackStartState : PlayerDodgeAttackState
{
    protected override DirectorID AttackDirectorID => DirectorID.DodgeAttackStart;

    protected override string AnimationTrigger => "IsDodgeAttack";

    protected override PlayerStateBase NextState => Core.StateMachine.DodgeAttackLoopState;

    public PlayerDodgeAttackStartState(PlayerCore core) : base(core)
    {
    }

    public override void Enter()
    {
        EnemyCore target = Core.DodgeAttackTarget;
        if (target != null)
        {
            Vector3 direction = target.transform.position - Core.transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > Mathf.Epsilon)
                Core.Rotator.RotateImmediately(direction);
        }

        base.Enter();
    }

    public override void UpdateTick()
    {
        // 자신을 Perfect Dodge로 진입시킨 주체의 공격을 한 대상의 Hurtbox를 향해서
        // 이 상태가 끝나기 전에 Motion Warping 완료가 되어야 함
        MotionWarpToEnemyHurtbox();

        base.UpdateTick();
    }

    private void MotionWarpToEnemyHurtbox()
    {

    }
}
