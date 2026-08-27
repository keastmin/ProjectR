using UnityEngine;

public class PlayerStateMachine
{
    public PlayerIdleState IdleState;
    public PlayerRunStartState RunStartState;
    public PlayerRunLoopState RunLoopState;
    public PlayerRunStopLeftState RunStopLeftState;
    public PlayerRunStopRightState RunStopRightState;
    public PlayerFastRunLoopState FastRunLoopState;
    public PlayerFastRunStopState FastRunStopState;
    public PlayerBasicAttack1State BasicAttack1State;
    public PlayerBasicAttack2State BasicAttack2State;
    public PlayerBasicAttack3State BasicAttack3State;
    public PlayerBasicAttack4State BasicAttack4State;
    public PlayerFrontDodgeState FrontDodgeState;
    public PlayerFrontDodgeStopState FrontDodgeStopState;
    public PlayerBackDodgeState BackDodgeState;
    public PlayerRunAttackState RunAttackState;
    public PlayerFastRunTurnState FastRunTurnState;

    private PlayerStateBase _currentState;
    private Animator _playerAnimator;

    public PlayerStateMachine(PlayerCore player)
    {
        _playerAnimator = player.Animator;
        IdleState = new PlayerIdleState(player);
        RunStartState = new PlayerRunStartState(player);
        RunLoopState = new PlayerRunLoopState(player);
        RunStopLeftState = new PlayerRunStopLeftState(player);
        RunStopRightState = new PlayerRunStopRightState(player);
        FastRunLoopState = new PlayerFastRunLoopState(player);
        FastRunStopState = new PlayerFastRunStopState(player);
        BasicAttack1State = new PlayerBasicAttack1State(player);
        BasicAttack2State = new PlayerBasicAttack2State(player);
        BasicAttack3State = new PlayerBasicAttack3State(player);
        BasicAttack4State = new PlayerBasicAttack4State(player);
        FrontDodgeState = new PlayerFrontDodgeState(player);
        FrontDodgeStopState = new PlayerFrontDodgeStopState(player);
        BackDodgeState = new PlayerBackDodgeState(player);
        RunAttackState = new PlayerRunAttackState(player);
        FastRunTurnState = new PlayerFastRunTurnState(player);
    }

    public void InitStateMachine(PlayerStateBase initState)
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

    public void Transition(PlayerStateBase nextState, string paramName)
    {
        _playerAnimator.SetTrigger(paramName);
        Transition(nextState);
    }

    public void Transition(PlayerStateBase nextState, string paramName, bool value)
    {
        _playerAnimator.SetBool(paramName, value);
        Transition(nextState);
    }

    public void Transition(PlayerStateBase nextState)
    {
        _currentState?.Exit();
        _currentState = nextState;
        _currentState?.Enter();
    }
}
