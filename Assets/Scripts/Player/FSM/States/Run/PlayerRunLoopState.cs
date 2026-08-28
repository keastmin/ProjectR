using UnityEngine;

public class PlayerRunLoopState : PlayerStateBase
{
    private float _fastRunTransitionTime = 2f;
    private float _currentStateTime = 0f;

    public PlayerRunLoopState(PlayerCore player) : base(player)
    {

    }

    public override void Enter()
    {
        Debug.Log("PlayerRunLoopState 진입");
        Debug.Log("달리기 루프");

        // 초기화
        _currentStateTime = 0f;

        // 애니메이션 재생
        Core.Animator.SetTrigger("IsRunLoop");
    }

    public override void UpdateTick()
    {
        // 상태 시간 누적
        _currentStateTime += Time.deltaTime;

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

        // 상태 시간이 빠른 달리기 전환 시간 이상이면 빠른 달리기로 전환
        if(_currentStateTime >= _fastRunTransitionTime)
        {
            Core.StateMachine.Transition(Core.StateMachine.FastRunLoopState);
            return;
        }

        // 이동 입력이 없다면 달리기 종료
        if (!Core.InputCollector.IsInputMove)
        {
            FrontFoot currentFrontFoot = Core.FootPosDetector.GetCurrentFrontFoot();
            if (currentFrontFoot == FrontFoot.LeftFoot)
                Core.StateMachine.Transition(Core.StateMachine.RunStopLeftState);
            else
                Core.StateMachine.Transition(Core.StateMachine.RunStopRightState);
            return;
        }
    }

    public override void FixedTick()
    {
        Core.Mover.Move(AnimDeltaPos / Time.fixedDeltaTime);
        AnimDeltaPos = Vector3.zero;
    }

    public override void AnimatorTick()
    {
        AnimDeltaPos += Core.Animator.deltaPosition;
    }

    public override void Exit()
    {
        // 초기화
        _currentStateTime = 0f;
    }

    private void Rotation()
    {
        Vector3 targetDirection = Core.DirCalculator.GetTargetDirection(Core.InputCollector.MoveValue, Core.MainCamera.transform);
        Core.Rotator.RotateToward(targetDirection);
    }
}
