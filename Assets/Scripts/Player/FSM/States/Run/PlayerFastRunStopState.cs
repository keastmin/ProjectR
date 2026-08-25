using UnityEngine;

public class PlayerFastRunStopState : PlayerStateBase
{
    private bool _isTransitionIdle = false;

    public PlayerFastRunStopState(PlayerCore player) : base(player)
    {

    }

    public override void Enter()
    {
        Debug.Log("PlayerFastRunStopState 진입");
        // 초기화
        _isTransitionIdle = false;

        // 이벤트 연결
        Core.AnimationEvent.OnTransitionIdle += SetTransitionIdle;

        // 애니메이션 재생
        Core.Animator.SetTrigger("IsFastRunStop");
    }

    public override void UpdateTick()
    {
        // 회피 입력이 있으면 뒤로 회피로 전환
        if (Core.InputCollector.IsInputDodge)
        {
            Core.StateMachine.Transition(Core.StateMachine.BackDodgeState);
            return;
        }

        // 기본 공격 입력이 있으면 기본 공격으로 전환
        if (Core.InputCollector.IsInputAttack)
        {
            Core.StateMachine.Transition(Core.StateMachine.BasicAttack1State);
            return;
        }

        // 이동 입력이 있으면 달리기 시작으로 전환
        if (Core.InputCollector.IsInputMove)
        {
            Core.StateMachine.Transition(Core.StateMachine.RunStartState);
            return;
        }

        // Idle 전환 플래그가 활성화 되면 Idle로 전환
        if (_isTransitionIdle)
        {
            Core.StateMachine.Transition(Core.StateMachine.IdleState);
            return;
        }
    }

    public override void FixedTick()
    {
        Core.Mover.Move(Core.Rotator.FacingRotation * (AnimDeltaPos / Time.fixedDeltaTime));
        AnimDeltaPos = Vector3.zero;
    }

    public override void AnimatorTick()
    {
        AnimDeltaPos += Core.Animator.deltaPosition;
    }

    public override void Exit()
    {
        // 초기화
        _isTransitionIdle = false;

        // 이벤트 해제
        Core.AnimationEvent.OnTransitionIdle -= SetTransitionIdle;
    }

    private void SetTransitionIdle()
    {
        _isTransitionIdle = true;
    }
}
