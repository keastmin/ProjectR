using UnityEngine;

public class PlayerIdleState : PlayerStateBase
{
    private Vector3 _animDeltaPos = Vector3.zero;

    public PlayerIdleState(PlayerCore player) : base(player)
    {

    }

    public override void Enter()
    {
        Debug.Log("PlayerIdleState 진입");
        _animDeltaPos = Vector3.zero;
        Core.Animator.SetTrigger("IsIdle");
    }

    public override void UpdateTick()
    {
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
        Core.Mover.Move(Core.Rotator.FacingRotation * (_animDeltaPos / Time.fixedDeltaTime));
        _animDeltaPos = Vector3.zero;
    }

    public override void AnimatorTick()
    {
        _animDeltaPos += Core.Animator.deltaPosition;
    }

    public override void Exit()
    {
        Core.Animator.ResetTrigger("IsIdle");
        _animDeltaPos = Vector3.zero;
    }
}
