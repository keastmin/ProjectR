using UnityEngine;

public class EnemyStateMachine
{
    public EnemyIdleState IdleState;
    public EnemyEngageState EngageState;
    public EnemyCloseAttackState CloseAttackState;
    public EnemyFrontHitState FrontHitState;
    public EnemyBackHitState BackHitState;

    private EnemyStateBase _currentState;

    public EnemyStateMachine(EnemyCore enemy)
    {
        IdleState = new EnemyIdleState(enemy);
        EngageState = new EnemyEngageState(enemy);
        CloseAttackState = new EnemyCloseAttackState(enemy);
        FrontHitState = new EnemyFrontHitState(enemy);
        BackHitState = new EnemyBackHitState(enemy);
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
        _currentState = nextState;
        _currentState?.Enter();
    }
}