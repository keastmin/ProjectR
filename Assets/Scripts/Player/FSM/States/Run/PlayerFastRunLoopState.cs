using UnityEngine;

public class PlayerFastRunLoopState : PlayerStateBase
{
    public PlayerFastRunLoopState(PlayerCore player) : base(player)
    {

    }

    public override void Enter()
    {
        Debug.Log("PlayerFastRunLoopState 진입");

        // 애니메이션 재생
        Core.Animator.SetTrigger("IsFastRunLoop");
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

        // 기본 공격 입력이 있으면 기본 공격으로 전환
        if (Core.InputCollector.IsInputAttack)
        {
            Core.StateMachine.Transition(Core.StateMachine.BasicAttack1State);
            return;
        }

        // 이동 입력이 없으면 빠른 달리기 종료
        if (!Core.InputCollector.IsInputMove)
        {
            Core.StateMachine.Transition(Core.StateMachine.FastRunStopState);
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

    }

    private void Rotation()
    {
        Vector3 targetDirection = Core.DirCalculator.GetTargetDirection(Core.InputCollector.MoveValue, Core.MainCamera.transform);
        Core.Rotator.RotateToward(targetDirection);
    }
}
