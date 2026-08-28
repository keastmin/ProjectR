using UnityEngine;

public class PlayerFastRunLoopState : PlayerStateBase
{
    private const float OppositeDirectionDotThreshold = -0.8f;

    private Vector3 _runDirection = Vector3.forward;

    public PlayerFastRunLoopState(PlayerCore player) : base(player)
    {

    }

    public override void Enter()
    {
        base.Enter();

        Debug.Log("PlayerFastRunLoopState 진입");

        // 현재 실제 이동에 사용되는 방향을 달리기 방향으로 저장
        UpdateRunDirection();

        // 애니메이션 재생
        Core.Animator.SetTrigger("IsFastRunLoop");
    }

    public override void UpdateTick()
    {
        if (IsDamaged)
            return;

        // 회피 입력이 있으면 정면 회피로 전환
        if (Core.InputCollector.IsInputDodge)
        {
            Core.StateMachine.Transition(Core.StateMachine.FrontDodgeState);
            return;
        }

        // 공격 입력이 있으면 달리기 공격으로 전환
        if (Core.InputCollector.IsInputAttack)
        {
            Core.StateMachine.Transition(Core.StateMachine.RunAttackState);
            return;
        }

        // 한 프레임도 끊기지 않고 반대 방향 입력이 들어오면 빠른 달리기 회전으로 전환
        if (TryTransitionToFastRunTurn())
            return;

        // 이동 입력이 없으면 빠른 달리기 종료
        if (!Core.InputCollector.IsInputMove)
        {
            Core.StateMachine.Transition(Core.StateMachine.FastRunStopState);
            return;
        }

        // 회전 후 실제 이동 방향을 다음 프레임의 반대 방향 판정에 사용
        Rotation();
        UpdateRunDirection();
    }

    public override void FixedTick()
    {
        if (IsDamaged)
            return;

        Core.Mover.Move(AnimDeltaPos / Time.fixedDeltaTime);
        AnimDeltaPos = Vector3.zero;
    }

    public override void AnimatorTick()
    {
        if (IsDamaged)
            return;

        AnimDeltaPos += Core.Animator.deltaPosition;
    }

    public override void Exit()
    {
        base.Exit();
    }

    private void Rotation()
    {
        Vector3 targetDirection = Core.DirCalculator.GetTargetDirection(Core.InputCollector.MoveValue, Core.MainCamera.transform);
        Core.Rotator.RotateToward(targetDirection);
    }

    private bool TryTransitionToFastRunTurn()
    {
        if (!Core.InputCollector.IsInputMove)
            return false;

        Vector3 inputDirection = Core.DirCalculator.GetTargetDirection(
            Core.InputCollector.MoveValue,
            Core.MainCamera.transform);

        bool isOppositeDirection = Vector3.Dot(_runDirection, inputDirection) <= OppositeDirectionDotThreshold;
        if (!isOppositeDirection)
            return false;

        Core.StateMachine.Transition(Core.StateMachine.FastRunTurnState);
        return true;
    }

    private void UpdateRunDirection()
    {
        _runDirection = Core.Rotator.FacingRotation * Vector3.forward;
    }
}
