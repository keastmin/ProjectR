using UnityEngine;

public class PlayerRunStopLeftState : PlayerStateBase
{
    private Vector3 _animDeltaPos;

    private bool _isTransitionIdle = false;

    public PlayerRunStopLeftState(PlayerCore player) : base(player)
    {

    }

    public override void Enter()
    {
        Debug.Log("PlayerRunStopLeftState 진입");
        // 이벤트 연결
        Core.AnimationEvent.OnKeepNext += SetTransitionIdle;

        // 초기화
        _animDeltaPos = Vector3.zero;
        _isTransitionIdle = false;

        // 애니메이션 재생
        Core.Animator.SetTrigger("IsRunStopLeft");
    }

    public override void UpdateTick()
    {
        // 회피 입력이 있다면 회피 백회피 시작
        if (Core.InputCollector.IsInputDodge)
        {
            Core.StateMachine.Transition(Core.StateMachine.BackDodgeState);
            return;
        }

        // 기본 공격 입력이 있다면 기본 공격 상태로 전환
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

        // 이동 입력이 없다면 달리기 종료
        if (_isTransitionIdle)
        {
            Core.StateMachine.Transition(Core.StateMachine.IdleState);
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
        // 이벤트 해제
        Core.AnimationEvent.OnKeepNext -= SetTransitionIdle;

        // 초기화
        _animDeltaPos = Vector3.zero;
        _isTransitionIdle = false;
    }

    private void SetTransitionIdle()
    {
        _isTransitionIdle = true;
    }
}
