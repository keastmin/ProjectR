using UnityEngine;

public class PlayerIdleState : PlayerStateBase
{
    public PlayerIdleState(PlayerCore player) : base(player)
    {

    }

    public override void Enter()
    {
        Core.Animator.SetTrigger("IsIdle");
    }

    public override void UpdateTick()
    {
        // 기본 공격 입력이 있다면 기본 공격 시작
        if (Core.InputCollector.IsInputAttack)
        {
            Core.StateMachine.Transition(Core.StateMachine.BasicAttack1State);
            return;
        }

        // 이동 입력이 있다면 달리기 시작
        if (Core.InputCollector.IsInputMove)
        {
            Core.StateMachine.Transition(Core.StateMachine.RunStartState);
            return;
        }
    }

    public override void FixedTick()
    {
        
    }

    public override void AnimatorTick()
    {
        
    }
}