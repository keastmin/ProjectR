using System;
using UnityEngine;

public class EnemyCore : MonoBehaviour, IDamageable
{
    [SerializeField, Min(0f)] private float _maxHP = 100f;
    [SerializeField] private float _currentHP;

    [SerializeField] private EnemyAnimatorCallback _animatorCallback;

    private DamageData _lastDamageData;

    private EnemyRotator _rotator;
    private EnemyMover _mover;
    private EnemyAnimationEvent _animationEvent;
    private EnemyStateMachine _stateMachine;

    public float CurrentHP => _currentHP;
    public Animator Animator => _animatorCallback.Animator;
    public EnemyRotator Rotator => _rotator;
    public EnemyMover Mover => _mover;
    public EnemyAnimationEvent AnimationEvent => _animationEvent;
    public EnemyStateMachine StateMachine => _stateMachine;
    public DamageData LastDamageData => _lastDamageData;

    public event Action<DamageData> OnDamaged;

    private void Awake()
    {
        TryGetComponent(out _rotator);
        TryGetComponent(out _mover);
        TryGetComponent(out _animationEvent);

        _currentHP = _maxHP;

        _stateMachine = new EnemyStateMachine(this);
        _animatorCallback.OnAnimatorMoveAction += AnimatorUpdate;
    }

    private void Start()
    {
        _stateMachine.InitEnemyStateMachine(_stateMachine.IdleState);
    }

    private void Update()
    {
        StateMachine.UpdateTick();
    }

    private void FixedUpdate()
    {
        StateMachine.FixedTick();
    }

    private void LateUpdate()
    {
        StateMachine.LateTick();
    }

    private void AnimatorUpdate()
    {
        StateMachine.AnimatorTick();
    }

    public void TakeDamage(DamageData damageData)
    {
        if (damageData.DamageAmount <= 0f || _currentHP <= 0f)
            return;

        _currentHP = Mathf.Max(_currentHP - damageData.DamageAmount, 0f);
        _lastDamageData = damageData;
        OnDamaged?.Invoke(damageData);
    }
}
