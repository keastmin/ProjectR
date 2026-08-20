using System;
using UnityEngine;

public class PlayerAnimationEvent : MonoBehaviour
{
    public event Action OnEnableNextBasicAttack;
    public event Action OnDisableNextBasicAttack;

    // 다음 기본 공격 가능 이벤트
    public void OnEnableNextBasicAttackActionInvoke()
    {
        OnEnableNextBasicAttack?.Invoke();
    }

    // 다음 기본 공격 불가능 이벤트
    public void OnDisableNextBasicAttackActionInvoke()
    {
        OnDisableNextBasicAttack?.Invoke();
    }
}