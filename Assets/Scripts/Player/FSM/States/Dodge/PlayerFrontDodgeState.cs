using UnityEngine;

public class PlayerFrontDodgeState : PlayerStateBase
{
    private bool _isTransitionFrontDodgeStop = false;
    private bool _isTransitionFastRunLoop = false;

    public PlayerFrontDodgeState(PlayerCore player) : base(player)
    {

    }

    public override void Enter()
    {
        Debug.Log("PlayerFrontDodgeState 진입");
        // 초기화
        _isTransitionFrontDodgeStop = false;
        _isTransitionFastRunLoop = false;

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
        // 공격 입력이 있으면 달리기 공격으로 전환
        if (Core.InputCollector.IsInputAttack)
        {
            Core.StateMachine.Transition(Core.StateMachine.RunAttackState);
            return;
        }

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
        _isTransitionFrontDodgeStop = false;
        _isTransitionFastRunLoop = false;

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
