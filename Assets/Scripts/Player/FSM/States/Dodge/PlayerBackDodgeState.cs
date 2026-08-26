using UnityEngine;

public class PlayerBackDodgeState : PlayerStateBase
{
    private bool _isEnableOtherBehaviour = false;
    private bool _isTransitionIdle = false;

    public PlayerBackDodgeState(PlayerCore player) : base(player)
    {

    }

    public override void Enter()
    {
        Debug.Log("PlayerBackDodgeState 진입");
        // 초기화
        _isEnableOtherBehaviour = false;
        _isTransitionIdle = false;

        // 이벤트 연결
        Core.AnimationEvent.OnEnableOtherBehaviour += SetEnableOtherBehaviour;
        Core.AnimationEvent.OnTransitionIdle += SetTransitionIdle;

        // 애니메이션 재생
        Core.Animator.SetTrigger("IsBackDodge");
    }

    public override void UpdateTick()
    {
        if (_isEnableOtherBehaviour)
        {
            // 회피 입력이 있다면 회피
            if (Core.InputCollector.IsInputDodge)
            {
                if (Core.InputCollector.IsInputMove)
                    Core.StateMachine.Transition(Core.StateMachine.FrontDodgeState);
                else
                    Core.StateMachine.Transition(Core.StateMachine.BackDodgeState);
                return;
            }

            if (Core.InputCollector.IsInputAttack)
            {
                Core.StateMachine.Transition(Core.StateMachine.BasicAttack1State);
                return;
            }

            if (Core.InputCollector.IsInputMove)
            {
                Core.StateMachine.Transition(Core.StateMachine.RunStartState);
                return;
            }
        }
        else
        {
            // 공격 입력이 있으면 달리기 공격으로 전환
            if (Core.InputCollector.IsInputAttack)
            {
                Core.StateMachine.Transition(Core.StateMachine.RunAttackState);
                return;
            }
        }

        if (_isTransitionIdle)
        {
            Core.StateMachine.Transition(Core.StateMachine.IdleState);
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
        // 이벤트 해제
        Core.AnimationEvent.OnEnableOtherBehaviour -= SetEnableOtherBehaviour;
        Core.AnimationEvent.OnTransitionIdle -= SetTransitionIdle;

        // 초기화
        _isEnableOtherBehaviour = false;
        _isTransitionIdle = false;
    }

    private void SetEnableOtherBehaviour()
    {
        _isEnableOtherBehaviour = true;
    }

    private void SetTransitionIdle()
    {
        _isTransitionIdle = true;
    }
}
