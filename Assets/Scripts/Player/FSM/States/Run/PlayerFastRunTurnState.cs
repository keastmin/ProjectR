using UnityEngine;

public class PlayerFastRunTurnState : PlayerStateBase
{
    private bool _isTransitionFastRunLoop = false;

    public PlayerFastRunTurnState(PlayerCore player) : base(player)
    {

    }

    public override void Enter()
    {
        // 초기화
        _isTransitionFastRunLoop = false;

        // 이벤트 연결
        Core.AnimationEvent.OnFastRunTurnEnd += SetTransitionFastRunLoop;

        // 애니메이션 재생
        Core.Animator.SetTrigger("IsFastRunTurn");
    }

    public override void UpdateTick()
    {
        // 회피 입력이 있으면 회피 상태로 전환
        if (Core.InputCollector.IsInputDodge)
        {
            if (Core.InputCollector.IsInputMove)
                Core.StateMachine.Transition(Core.StateMachine.FrontDodgeState);
            else
                Core.StateMachine.Transition(Core.StateMachine.BackDodgeState);
            return;
        }

        // 공격 입력이 있으면 Run Attack으로 전환
        if (Core.InputCollector.IsInputAttack)
        {
            Core.StateMachine.Transition(Core.StateMachine.RunAttackState);
            return;
        }

        // Fast Run Loop 전환 플래그가 활성화 되면 Fast Run Loop로 전환
        if (_isTransitionFastRunLoop)
        {
            if (Core.InputCollector.IsInputMove)
                Core.StateMachine.Transition(Core.StateMachine.FastRunLoopState);
            else
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
        Vector3 dir = (Core.Rotator.FacingRotation * Core.Animator.deltaRotation) * Vector3.forward;
        Core.Rotator.RotateImmediately(dir);
    }

    public override void Exit()
    {
        // 초기화
        _isTransitionFastRunLoop = false;

        // 이벤트 해제
        Core.AnimationEvent.OnFastRunTurnEnd -= SetTransitionFastRunLoop;
    }

    private void SetTransitionFastRunLoop()
    {
        _isTransitionFastRunLoop = true;
    }
}