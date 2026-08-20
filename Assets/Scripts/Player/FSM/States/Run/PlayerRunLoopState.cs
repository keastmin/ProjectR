using UnityEngine;

public class PlayerRunLoopState : PlayerStateBase
{
    private Vector3 _animDeltaPos;

    public PlayerRunLoopState(PlayerCore player) : base(player)
    {

    }

    public override void Enter()
    {
        Debug.Log("달리기 유지");
        _animDeltaPos = Vector3.zero;
    }

    public override void UpdateTick()
    {
        // 회전
        Rotation();

        // 기본 공격 입력이 있다면 기본 공격 상태로 전환
        if (Core.InputCollector.IsInputAttack)
        {
            Core.StateMachine.Transition(Core.StateMachine.BasicAttack1State, "IsBasicAttack");
            return;
        }

        // 이동 입력이 없다면 달리기 종료
        if (!Core.InputCollector.IsInputMove)
        {
            Core.StateMachine.Transition(Core.StateMachine.RunStopState, "IsRunStop");
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
        _animDeltaPos = Vector3.zero;
    }

    private void Rotation()
    {
        Vector3 targetDirection = Core.DirCalculator.GetTargetDirection(Core.InputCollector.MoveValue, Core.MainCamera.transform);
        Core.Rotator.RotateToward(targetDirection);
    }
}
