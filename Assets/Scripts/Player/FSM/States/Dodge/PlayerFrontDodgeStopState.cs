using UnityEngine;

public class PlayerFrontDodgeStopState : PlayerStateBase
{
    private bool _isTransitionIdle = false;

    public PlayerFrontDodgeStopState(PlayerCore player) : base(player)
    {

    }

    public override void Enter()
    {
        Debug.Log("PlayerFrontDodgeStopState 진입");
        // 초기화
        _isTransitionIdle = false;

        // 이벤트 연결
        Core.AnimationEvent.OnKeepNext += SetTransitionIdle;

        // 애니메이션 시작
        Core.Animator.SetTrigger("IsFrontDodgeStop");
    }

    public override void UpdateTick()
    {
        // 회피 입력이 있다면 뒤로 회피로 전환
        if (Core.InputCollector.IsInputDodge)
        {
            Core.StateMachine.Transition(Core.StateMachine.BackDodgeState);
            return;
        }

        // 이동 입력이 있으면 달리기 시작으로 전환
        if (Core.InputCollector.IsInputMove)
        {
            Core.StateMachine.Transition(Core.StateMachine.RunStartState);
            return;
        }

        // Idle 상태 전환 플래그 활성화 시 Idle로 전환
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
        Core.AnimationEvent.OnKeepNext -= SetTransitionIdle;
    }

    private void SetTransitionIdle()
    {
        _isTransitionIdle = true;
    }
}
