using System;
using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour
{
    public event Action OnAnimationTick;

    private void OnAnimatorMove()
    {
        OnAnimationTick?.Invoke();
    }
}