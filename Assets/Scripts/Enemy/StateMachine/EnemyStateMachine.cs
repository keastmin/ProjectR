using UnityEngine;

public class EnemyStateMachine
{
    private EnemyStateBase _currentState;

    public EnemyStateMachine(EnemyCore enemy)
    {

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

    public void Transition(EnemyStateBase nextState)
    {
        _currentState?.Exit();
        _currentState = nextState;
        _currentState?.Enter();
    }
}