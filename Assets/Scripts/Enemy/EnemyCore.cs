using System;
using UnityEngine;

public class EnemyCore : MonoBehaviour, IDamageable, IHitStopParticipant
{
    [SerializeField, Min(0f)] private float _maxHP = 100f;
    [SerializeField] private float _currentHP;

    [SerializeField] private EnemyAnimatorCallback _animatorCallback;

    private DamageData _lastDamageData;

    private EnemyRotator _rotator;
    private EnemyMover _mover;
    private EnemyAnimationEvent _animationEvent;
    private EnemyStateMachine _stateMachine;
    private EnemyTargetDetector _targetDetector;
    private bool _isHitStopped;
    private float _animatorSpeedBeforeHitStop = 1f;

    public float CurrentHP => _currentHP;
    public Animator Animator => _animatorCallback.Animator;
    public EnemyRotator Rotator => _rotator;
    public EnemyMover Mover => _mover;
    public EnemyAnimationEvent AnimationEvent => _animationEvent;
    public EnemyStateMachine StateMachine => _stateMachine;
    public EnemyTargetDetector TargetDetector => _targetDetector;
    public DamageData LastDamageData => _lastDamageData;
    public bool IsHitStopped => _isHitStopped;
    public Transform TargetTransform => TargetDetector.TargetTransform;

    public event Action<DamageData> OnDamaged;

    private void Awake()
    {
        TryGetComponent(out _rotator);
        TryGetComponent(out _mover);
        TryGetComponent(out _animationEvent);
        TryGetComponent(out _targetDetector);

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
        if (_isHitStopped)
            return;

        StateMachine.UpdateTick();
    }

    private void FixedUpdate()
    {
        if (_isHitStopped)
            return;

        StateMachine.FixedTick();
    }

    private void LateUpdate()
    {
        if (_isHitStopped)
            return;

        StateMachine.LateTick();
    }

    private void AnimatorUpdate()
    {
        if (_isHitStopped)
            return;

        StateMachine.AnimatorTick();
    }

    private void OnDisable()
    {
        EndHitStop();
    }

    public bool TryTakeDamage(DamageData damageData)
    {
        if (damageData.DamageAmount <= 0f || _currentHP <= 0f)
            return false;

        _currentHP = Mathf.Max(_currentHP - damageData.DamageAmount, 0f);
        _lastDamageData = damageData;
        OnDamaged?.Invoke(damageData);
        return true;
    }

    public void PlayHitReaction(int stateHash)
    {
        Animator.Play(stateHash, 0, 0f);
        Animator.Update(0f);
    }

    public void BeginHitStop()
    {
        if (_isHitStopped)
            return;

        _isHitStopped = true;
        _stateMachine.ClearAccumulatedMotion();
        _mover.SetHitStopped(true);

        _animatorSpeedBeforeHitStop = Animator.speed;
        Animator.speed = 0f;
    }

    public void EndHitStop()
    {
        if (!_isHitStopped)
            return;

        _isHitStopped = false;
        Animator.speed = _animatorSpeedBeforeHitStop;
        _mover.SetHitStopped(false);
    }
}