using System;
using UnityEngine;

public class EnemyCore : MonoBehaviour, IDamageable
{
    [SerializeField, Min(0f)] private float _maxHP = 100f;
    [SerializeField] private float _currentHP;

    [SerializeField] private EnemyAnimatorCallback _animatorCallback;

    public float CurrentHP => _currentHP;
    public Animator Animator => _animatorCallback.Animator;
    public Rigidbody Rigidbody => _rigidbody;

    private Rigidbody _rigidbody;

    public event Action<DamageData> OnDamaged;

    private void Awake()
    {
        _currentHP = _maxHP;
        TryGetComponent(out _rigidbody);
    }

    private void FixedUpdate()
    {
        _rigidbody.linearVelocity = Vector3.zero;
    }

    public void TakeDamage(DamageData damageData)
    {
        if (damageData.DamageAmount <= 0f || _currentHP <= 0f)
            return;

        _currentHP = Mathf.Max(_currentHP - damageData.DamageAmount, 0f);
        OnDamaged?.Invoke(damageData);
    }
}
