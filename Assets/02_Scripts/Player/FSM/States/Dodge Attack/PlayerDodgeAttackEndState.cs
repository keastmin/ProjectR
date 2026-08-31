using UnityEngine;

public class PlayerDodgeAttackEndState : PlayerDodgeAttackState
{
    protected override DirectorID AttackDirectorID => DirectorID.DodgeAttackEnd;

    protected override string AnimationTrigger => "IsDodgeAttack";

    protected override PlayerStateBase NextState => Core.StateMachine.IdleState;

    private bool _isEnableOtherBehaviour = false;

    public PlayerDodgeAttackEndState(PlayerCore core) : base(core)
    {
    }

    public override void Enter()
    {
        // 초기화
        _isEnableOtherBehaviour = false;

        // 이벤트 연결
        Core.AnimationEvent.OnEnableOtherBehaviour += SetEnableOtherBehaviour;

        base.Enter();
    }

    public override void UpdateTick()
    {
        // 다른 행동 가능 플래그가 활성화 되면 다른 행동으로 전환
        if (_isEnableOtherBehaviour)
        {
            // 회피 전환
            if (Core.InputCollector.IsInputDodge)
            {
                if (Core.InputCollector.IsInputMove)
                    Core.StateMachine.Transition(Core.StateMachine.FrontDodgeState);
                else
                    Core.StateMachine.Transition(Core.StateMachine.BackDodgeState);
                return;
            }

            // 기본 공격 전환
            if (Core.InputCollector.IsInputAttack)
            {
                Core.StateMachine.Transition(Core.StateMachine.BasicAttack1State);
                return;
            }

            // 달리기 전환
            if (Core.InputCollector.IsInputMove)
            {
                Core.StateMachine.Transition(Core.StateMachine.RunStartState);
                return;
            }
        }

        base.UpdateTick();
    }

    public override void Exit()
    {
        // 초기화
        _isEnableOtherBehaviour = false;

        // 이벤트 해제
        Core.AnimationEvent.OnEnableOtherBehaviour -= SetEnableOtherBehaviour;

        base.Exit();
    }

    private void SetEnableOtherBehaviour()
    {
        _isEnableOtherBehaviour = true;
    }
}
