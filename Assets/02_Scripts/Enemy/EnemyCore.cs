using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyCore : MonoBehaviour, IDamageable, IHitStopParticipant
{
    [SerializeField, Min(0f)] private float _maxHP = 100f;
    [SerializeField] private float _currentHP;

    [SerializeField] private EnemyAnimatorCallback _animatorCallback;
    [SerializeField] private EnemyAttackSO[] _attackSOs;

    [SerializeField] private Collider[] _closeAttackNoticeBoxies;
    [SerializeField] private LayerMask _playerHurtboxLayer;

    private DamageData _lastDamageData;

    private EnemyRotator _rotator;
    private EnemyMover _mover;
    private EnemyAnimationEvent _animationEvent;
    private EnemyStateMachine _stateMachine;
    private EnemyTargetDetector _targetDetector;
    private EnemyHitboxPool _hitboxPool;
    private EnemyAttackSimulator _attackSimulator;
    private bool _isHitStopped;
    private float _animatorSpeedBeforeHitStop = 1f;

    public float CurrentHP => _currentHP;
    public Animator Animator => _animatorCallback.Animator;
    public EnemyRotator Rotator => _rotator;
    public EnemyMover Mover => _mover;
    public EnemyAnimationEvent AnimationEvent => _animationEvent;
    public EnemyStateMachine StateMachine => _stateMachine;
    public EnemyTargetDetector TargetDetector => _targetDetector;
    public EnemyHitboxPool HitboxPool => _hitboxPool;
    public EnemyAttackSimulator AttackSimulator => _attackSimulator;
    public DamageData LastDamageData => _lastDamageData;
    public bool IsHitStopped => _isHitStopped;
    public Transform TargetTransform => TargetDetector.TargetTransform;

    public Dictionary<EnemyAttackID, EnemyAttackSO> AttackDataDictionary;
    public Collider[] CloseAttackNotiveBoxies => _closeAttackNoticeBoxies;
    public event Action<DamageData> OnDamaged;

    private List<Collider> _attackRangeColliders = new();
    private Collider[] _playerDetectCollider = new Collider[10];

    private void Awake()
    {
        // 공격 데이터 저장
        AttackDataDictionary = new Dictionary<EnemyAttackID, EnemyAttackSO>();
        foreach (var so in _attackSOs)
            AttackDataDictionary.Add(so.AttackID, so);

        TryGetComponent(out _rotator);
        TryGetComponent(out _mover);
        TryGetComponent(out _animationEvent);
        TryGetComponent(out _targetDetector);
        TryGetComponent(out _hitboxPool);
        TryGetComponent(out _attackSimulator);

        _currentHP = _maxHP;

        _stateMachine = new EnemyStateMachine(this);
        _animatorCallback.OnAnimatorMoveAction += AnimatorUpdate;
        _animationEvent.OnAttack += HandleAttack;
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

    // 공격
    private void HandleAttack(EnemyAttackSO attackSO)
    {
        HitboxPool.SpacingHitboxes(attackSO);
    }

    public void SetAttackNoticeCollider(Collider[] colliders)
    {
        if (_attackRangeColliders == null)
            _attackRangeColliders = new List<Collider>();
        ClearAttackNoticeCollider();
        foreach(var col in colliders)
        {
            if (col != null)
                _attackRangeColliders.Add(col);
        }
    }

    public void ClearAttackNoticeCollider()
    {
        if (_attackRangeColliders == null)
            _attackRangeColliders = new List<Collider>();
        _attackRangeColliders.Clear();
    }

    public bool IsPlayerInEnemyAttackRange()
    {
        foreach(var col in _attackRangeColliders)
        {
            if(col != null)
            {
                if (col is BoxCollider box)
                {
                    Vector3 halfExtents = Vector3.Scale(box.size, Abs(box.transform.lossyScale)) * 0.5f;

                    int detectCount = Physics.OverlapBoxNonAlloc(
                                              box.transform.TransformPoint(box.center),
                                              halfExtents,
                                              _playerDetectCollider,
                                              box.transform.rotation,
                                              _playerHurtboxLayer,
                                              QueryTriggerInteraction.Collide);

                    if (detectCount > 0)
                        return true;
                }
            }
        }

        return false;
    }

    private Vector3 Abs(Vector3 value)
    {
        return new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
    }
}