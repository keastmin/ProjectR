using UnityEngine;

public class PlayerBackDodgeState : PlayerStateBase
{
    private bool _isEnableOtherBehaviour = false;
    private bool _isTransitionIdle = false;

    private Vector3 _animDeltaPos = Vector3.zero;

    public PlayerBackDodgeState(PlayerCore player) : base(player)
    {

    }

    public override void Enter()
    {
        Debug.Log("PlayerBackDodgeState 진입");
        // 초기화
        _isEnableOtherBehaviour = false;
        _isTransitionIdle = false;
        _animDeltaPos = Vector3.zero;

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
            if (Core.InputCollector.IsInputDodge)
            {
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

        if (_isTransitionIdle)
        {
            Core.StateMachine.Transition(Core.StateMachine.IdleState);
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
        // 이벤트 해제
        Core.AnimationEvent.OnEnableOtherBehaviour -= SetEnableOtherBehaviour;
        Core.AnimationEvent.OnTransitionIdle -= SetTransitionIdle;

        // 초기화
        _isEnableOtherBehaviour = false;
        _isTransitionIdle = false;
        _animDeltaPos = Vector3.zero;
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
