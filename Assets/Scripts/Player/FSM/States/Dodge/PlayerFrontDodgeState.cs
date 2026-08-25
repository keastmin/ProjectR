using UnityEngine;

public class PlayerFrontDodgeState : PlayerStateBase
{
    private bool _isTransitionFrontDodgeStop = false;
    private bool _isTransitionFastRunLoop = false;

    private Vector3 _animDeltaPos = Vector3.zero;

    public PlayerFrontDodgeState(PlayerCore player) : base(player)
    {

    }

    public override void Enter()
    {
        Debug.Log("PlayerFrontDodgeState 진입");
        // 초기화
        _isTransitionFrontDodgeStop = false;
        _isTransitionFastRunLoop = false;
        _animDeltaPos = Vector3.zero;

        // 이벤트 연결
        Core.AnimationEvent.OnFrontDodgeStop += SetTransitionFrontDodgeStop;
        Core.AnimationEvent.OnTransitionFastRunLoop += SetTransitionFastRunLoop;

        // 애니메이션 시작
        Core.Animator.SetTrigger("IsFrontDodge");

        // 즉시 회전
        Vector3 targetDirection = Core.DirCalculator.GetTargetDirection(Core.InputCollector.MoveValue, Core.MainCamera.transform);
        Core.Rotator.RotateImmediately(targetDirection);
    }

    public override void UpdateTick()
    {
        // 회피 종료 플래그 타이밍에 이동 입력이 없으면 정면 회피 멈춤으로 전환
        if (_isTransitionFrontDodgeStop && !Core.InputCollector.IsInputMove)
        {
            Core.StateMachine.Transition(Core.StateMachine.FrontDodgeStopState);
            return;
        }

        // 빠른 달리기 플래그 활성화 시 빠른 달리기로 전환
        if (_isTransitionFastRunLoop)
        {
            Core.StateMachine.Transition(Core.StateMachine.FastRunLoopState);
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
        _isTransitionFrontDodgeStop = false;
        _isTransitionFastRunLoop = false;
        _animDeltaPos = Vector3.zero;

        // 이벤트 해제
        Core.AnimationEvent.OnFrontDodgeStop -= SetTransitionFrontDodgeStop;
        Core.AnimationEvent.OnTransitionFastRunLoop -= SetTransitionFastRunLoop;
    }

    private void SetTransitionFrontDodgeStop()
    {
        _isTransitionFrontDodgeStop = true;
    }

    private void SetTransitionFastRunLoop()
    {
        _isTransitionFastRunLoop = true;
    }
}
