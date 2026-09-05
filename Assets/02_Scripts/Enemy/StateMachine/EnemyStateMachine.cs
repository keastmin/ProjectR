using UnityEngine;

public class EnemyStateMachine
{
    public EnemyIdleState IdleState;
    public EnemyEngageState EngageState;
    public EnemyCloseAttackState CloseAttackState;
    public EnemyFrontHitState FrontHitState;
    public EnemyBackHitState BackHitState;
    public EnemyCloseAttackNoticeState CloseAttackNoticeState;
    public EnemyDeadState DeadState;

    private EnemyStateBase _currentState;

    private EnemyCore _core;

    public EnemyStateMachine(EnemyCore enemy)
    {
        _core = enemy;
        IdleState = new EnemyIdleState(enemy);
        EngageState = new EnemyEngageState(enemy);
        CloseAttackState = new EnemyCloseAttackState(enemy);
        FrontHitState = new EnemyFrontHitState(enemy);
        BackHitState = new EnemyBackHitState(enemy);
        CloseAttackNoticeState = new EnemyCloseAttackNoticeState(enemy);
        DeadState = new EnemyDeadState(enemy);
    }

    public void InitEnemyStateMachine(EnemyStateBase initState)
    {
        _currentState = initState;
        _currentState?.Enter();
    }

    public void UpdateTick()
    {
        _currentState?.UpdateTick();
    }

    public void FixedTick()
    {
        _currentState?.FixedTick();
    }

    public void LateTick()
    {
        _currentState?.LateTick();
    }

    public void AnimatorTick()
    {
        _currentState?.AnimatorTick();
    }

    public void ClearAccumulatedMotion()
    {
        _currentState?.ClearAccumulatedMotion();
    }

    public void Transition(EnemyStateBase nextState)
    {
        _currentState?.Exit();
        // A pause preserves pending motion; cancelling a state does not.
        _currentState?.ClearAccumulatedMotion();
        _currentState = nextState;
        _currentState?.Enter();
    }
}
