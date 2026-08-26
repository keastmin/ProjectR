using System;
using UnityEngine;

public class PlayerAnimationEvent : MonoBehaviour
{
    public event Action OnEnableNextBasicAttack;
    public event Action OnDisableNextBasicAttack;
    public event Action OnDisableQuickTurn;
    public event Action OnEnableOtherBehaviour;
    public event Action OnKeepNext; // 공용 다음 상태로 이어서 가는 이벤트
    public event Action OnTransitionIdle;
    public event Action OnFrontDodgeStop;
    public event Action OnTransitionFastRunLoop;
    public event Action OnRunAttackEnableOtherBehaviour; // Run Attack 다음 행동 가능 이벤트
    public event Action OnRunAttackEnd; // Run Attack 종료 이벤트
    public event Action OnFastRunTurnEnd; // Fast Run Turn 종료 이벤트

    // 다음 기본 공격 가능 이벤트 발동
    public void OnEnableNextBasicAttackActionInvoke()
    {
        OnEnableNextBasicAttack?.Invoke();
    }

    // 다음 기본 공격 불가능 이벤트 발동
    public void OnDisableNextBasicAttackActionInvoke()
    {
        OnDisableNextBasicAttack?.Invoke();
    }

    // 빠른 회전 중단 이벤트 발동
    public void OnDisableQuickTurnActionvInvoke()
    {
        OnDisableQuickTurn?.Invoke();
    }

    // 다른 행동 가능 이벤트 발동
    public void OnEnableOtherBehaviourActionInvoke()
    {
        OnEnableOtherBehaviour?.Invoke();
    }

    // Idle로 전환 이벤트 발동
    public void OnTransitionIdleActionInvoke()
    {
        OnTransitionIdle?.Invoke();
    }

    // 정면 회피 종료 이벤트 발동
    public void OnFrontDodgeStopActionInvoke()
    {
        OnFrontDodgeStop?.Invoke();
    }

    // 빠른 달리기 전환 이벤트 발동
    public void OnTransitionFastRunLoopActionInvoke()
    {
        OnTransitionFastRunLoop?.Invoke();
    }

    // 다음 상태로 넘어가는 이벤트 발동
    public void OnKeepNextActionInvoke()
    {
        OnKeepNext?.Invoke();
    }

    // Run Attack에서 다음 행동이 가능한 이벤트 발동
    public void OnRunAttackEnableOtherBehaviourActionInvoke()
    {
        OnRunAttackEnableOtherBehaviour?.Invoke();
    }

    // Run Attack이 종료되는 이벤트 발동
    public void OnRunAttackEndActionInvoke()
    {
        OnRunAttackEnd?.Invoke();
    }

    // Fast Run Turn이 종료되는 이벤트 발동
    public void OnFastRunTurnEndActionInvoke()
    {
        OnFastRunTurnEnd?.Invoke();
    }
}