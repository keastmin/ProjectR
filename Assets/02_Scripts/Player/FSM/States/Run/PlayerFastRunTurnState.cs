using UnityEngine;

public class PlayerFastRunTurnState : PlayerStateBase
{
    private bool _isTransitionFastRunLoop = false;

    public PlayerFastRunTurnState(PlayerCore player) : base(player)
    {

    }

    public override void Enter()
    {
        base.Enter();

        // 초기화
        _isTransitionFastRunLoop = false;

        // 이벤트 연결
        Core.AnimationEvent.OnAnimationEnd += SetTransitionFastRunLoop;

        // 애니메이션 재생
        Core.Animator.SetTrigger("IsFastRunTurn");
    }

    public override void UpdateTick()
    {
        if (IsDamaged)
            return;

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
        Vector3 dir = (Core.Rotator.FacingRotation * Core.Animator.deltaRotation) * Vector3.forward;
        Core.Rotator.RotateImmediately(dir);
    }

    public override void Exit()
    {
        // 초기화
        _isTransitionFastRunLoop = false;

        // 이벤트 해제
        Core.AnimationEvent.OnAnimationEnd -= SetTransitionFastRunLoop;

        base.Exit();
    }

    private void SetTransitionFastRunLoop()
    {
        _isTransitionFastRunLoop = true;
    }
}
