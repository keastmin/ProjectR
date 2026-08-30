using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyAnimatorCallback : MonoBehaviour
{
    private Animator _animator;

    public event Action OnAnimatorMoveAction;
    public Animator Animator => _animator;

    private void Awake()
    {
        TryGetComponent(out _animator);
    }

    private void OnAnimatorMove()
    {
        OnAnimatorMoveAction?.Invoke();
    }
}
