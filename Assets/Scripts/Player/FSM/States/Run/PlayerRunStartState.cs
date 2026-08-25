using UnityEngine;

public class PlayerRunStartState : PlayerStateBase
{
    private Vector3 _animDeltaPos;

    private bool _isQuickTurn = true;
    private bool _isTransitionRunLoop = false;

    public PlayerRunStartState(PlayerCore player) : base(player)
    {

    }

    public override void Enter()
    {
        Debug.Log("PlayerRunStartState 진입");
        Debug.Log("달리기 시작");
        
        // 초기화
        _isQuickTurn = true;
        _isTransitionRunLoop = false;

        // 이벤트 연결
        Core.AnimationEvent.OnDisableQuickTurn += HandleQuickTurnEvent;
        Core.AnimationEvent.OnKeepNext += SetTransitionRunLoop;

        Core.Animator.SetTrigger("IsRunStart");
        _animDeltaPos = Vector3.zero;

    }

    public override void UpdateTick()
    {
        // 회전
        Rotation();

        // 회피 입력이 있으면 정면 회피로 전환
        if (Core.InputCollector.IsInputDodge)
        {
            Core.StateMachine.Transition(Core.StateMachine.FrontDodgeState);
            return;
        }

        // 기본 공격 입력이 있다면 기본 공격 상태로 전환
        if (Core.InputCollector.IsInputAttack)
        {
            Core.StateMachine.Transition(Core.StateMachine.BasicAttack1State);
            return;
        }

        // 이동 입력이 없다면 달리기 종료
        if (!Core.InputCollector.IsInputMove)
        {
            Core.StateMachine.Transition(Core.StateMachine.IdleState);
        }

        // 애니메이션 종료까지 입력이 있다면 달리기 유지
        if (_isTransitionRunLoop)
        {
            Core.StateMachine.Transition(Core.StateMachine.RunLoopState);
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
        // 초기화
        _isQuickTurn = true;
        _isTransitionRunLoop = false;
        _animDeltaPos = Vector3.zero;

        // 이벤트 해제
        Core.AnimationEvent.OnDisableQuickTurn -= HandleQuickTurnEvent;
        Core.AnimationEvent.OnKeepNext -= SetTransitionRunLoop;
    }

    private void Rotation()
    {
        Vector3 targetDirection = Core.DirCalculator.GetTargetDirection(Core.InputCollector.MoveValue, Core.MainCamera.transform);
        if (!_isQuickTurn)
            Core.Rotator.RotateToward(targetDirection);
        else
            Core.Rotator.RotateToward(targetDirection, 360f * 4);
    }

    private void HandleQuickTurnEvent()
    {
        _isQuickTurn = false;
    }

    private void SetTransitionRunLoop()
    {
        _isTransitionRunLoop = true;
    }
}
