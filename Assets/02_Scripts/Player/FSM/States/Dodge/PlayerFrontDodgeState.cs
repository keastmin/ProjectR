using UnityEngine;

public class PlayerFrontDodgeState : PlayerDodgeState
{
    protected override string AnimationTrigger => "IsFrontDodge";

    private bool _isTransitionNextAction = false;

    public PlayerFrontDodgeState(PlayerCore player) : base(player)
    {

    }

    public override void Enter()
    {
        Debug.Log("PlayerFrontDodgeState 진입");
        // 초기화
        _isTransitionNextAction = false;

        // 이벤트 연결
        Core.AnimationEvent.OnAnimationEnd += SetTransitionNextAction;

        // 즉시 회전
        Vector3 targetDirection = Core.DirCalculator.GetTargetDirection(Core.InputCollector.MoveValue, Core.MainCamera.transform);
        Core.Rotator.RotateImmediately(targetDirection);

        base.Enter();
    }

    public override void UpdateTick()
    {
        if (IsDamaged)
            return;

        // 공격 입력이 있으면 달리기 공격으로 전환
        if (Core.InputCollector.IsInputAttack)
        {
            Core.StateMachine.Transition(Core.StateMachine.RunAttackState);
            return;
        }

        // 다음 상태 판별
        if (_isTransitionNextAction)
        {
            // 이동 입력이 있으면 달리기 유지, 아니면 정면 회피 종료
            if (Core.InputCollector.IsInputMove)
                Core.StateMachine.Transition(Core.StateMachine.FastRunLoopState);
            else
                Core.StateMachine.Transition(Core.StateMachine.FrontDodgeStopState);
            return;
        }
    }

    public override void Exit()
    {
        // 초기화
        _isTransitionNextAction = false;

        // 이벤트 해제
        Core.AnimationEvent.OnAnimationEnd -= SetTransitionNextAction;

        base.Exit();
    }

    private void SetTransitionNextAction()
    {
        _isTransitionNextAction = true;
    }
}
