using UnityEngine;

public class PlayerBackDodgeState : PlayerDodgeState
{
    protected override string AnimationTrigger => "IsBackDodge";

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
        Core.AnimationEvent.OnAnimationEnd += SetTransitionIdle;

        base.Enter();
    }

    public override void UpdateTick()
    {
        if (IsDamaged)
            return;

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

    public override void Exit()
    {
        // 이벤트 해제
        Core.AnimationEvent.OnEnableOtherBehaviour -= SetEnableOtherBehaviour;
        Core.AnimationEvent.OnAnimationEnd -= SetTransitionIdle;

        // 초기화
        _isEnableOtherBehaviour = false;
        _isTransitionIdle = false;

        base.Exit();
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
