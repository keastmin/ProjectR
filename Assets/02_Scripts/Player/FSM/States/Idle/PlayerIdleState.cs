using UnityEngine;

public class PlayerIdleState : PlayerStateBase
{
    public PlayerIdleState(PlayerCore player) : base(player)
    {

    }

    public override void Enter()
    {
        Debug.Log("PlayerIdleState 진입");
        Core.Animator.SetTrigger("IsIdle");

        base.Enter();
    }

    public override void UpdateTick()
    {
        if (IsDamaged)
            return;

        // 회피 입력이 있다면 회피 백회피 시작
        if (Core.InputCollector.IsInputDodge)
        {
            Core.StateMachine.Transition(Core.StateMachine.BackDodgeState);
            return;
        }

        // 기본 공격 입력이 있다면 기본 공격 시작
        if (Core.InputCollector.IsInputAttack)
        {
            Core.StateMachine.Transition(Core.StateMachine.BasicAttack1State);
            return;
        }

        // 이동 입력이 있다면 달리기 시작
        if (Core.InputCollector.IsInputMove)
        {
            Core.StateMachine.Transition(Core.StateMachine.RunStartState);
            return;
        }
    }

    public override void FixedTick()
    {
        if (IsDamaged)
            return;

        Core.Mover.Move(AnimDeltaPos / Time.fixedDeltaTime);
        AnimDeltaPos = Vector3.zero;
    }

    public override void AnimatorTick()
    {
        if (IsDamaged)
            return;

        AnimDeltaPos += Core.Animator.deltaPosition;
    }

    public override void Exit()
    {
        Core.Animator.ResetTrigger("IsIdle");

        base.Exit();
    }
}
