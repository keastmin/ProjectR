using System;
using UnityEngine;
using UnityEngine.Events;

public class PlayerCore : MonoBehaviour, IHitStopParticipant, IDamageable
{
    [SerializeField, Min(1f)] private float _maxHealth = 100f;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Animator _animator;
    [SerializeField] private PlayerAnimatorController _animatorController;

    [Header("Perfect Dodge")]
    [SerializeField, InspectorName("Slow Motion Settings")] private PerfectDodgeSettings _perfectDodge = new();
    [SerializeField] private UnityEvent _onPerfectDodgeStarted;
    [SerializeField] private UnityEvent _onPerfectDodgeEnded;
    [SerializeField] private UnityEvent _onDodgeAttackStarted;

    [Header("Skill")]
    [SerializeField] private float _skillUsageGauge = 70f;
    [SerializeField] private float _perfectDodgeSkillGauge = 5f;

    // 컴포넌트
    private TimelineDirectorContainer _directorContainer;
    private PlayerMover _mover;
    private PlayerRotator _rotator;
    private PlayerInputCollector _inputCollector;
    private PlayerAnimationEvent _animationEvent;
    private FootPositionDetector _footPositionDetector;
    private PlayerAttackInstanceContainer _attackInstanceContainer;
    private PlayerTargetDetector _targetDetector;
    private MeshTrailEffect _trailEffect;

    // 기능
    private PlayerStateMachine _stateMachine;
    private DirectionCalculator _dirCalculator;

    private bool _isHitStopped;
    private bool _isStartingDodgeAttack;
    private bool _isPerfectDodgeActive;
    private bool _isPerfectDodgeSlowMotionActive;
    private float _perfectDodgeStartDelayRemaining;
    private float _perfectDodgeDurationRemaining;
    private float _baseAnimatorSpeed;

    private float _currentHealth = 0f;

    // 스킬 게이지
    private float _currentSkillGauge = 0f;

    // 프로퍼티
    public Camera MainCamera => _mainCamera;
    public Animator Animator => _animator;
    public TimelineDirectorContainer DirectorContainer => _directorContainer;
    public PlayerMover Mover => _mover;
    public PlayerRotator Rotator => _rotator;
    public PlayerInputCollector InputCollector => _inputCollector;
    public PlayerAnimationEvent AnimationEvent => _animationEvent;
    public FootPositionDetector FootPosDetector => _footPositionDetector;
    public PlayerAttackInstanceContainer AttackInstanceContainer => _attackInstanceContainer;
    public PlayerTargetDetector TargetDetector => _targetDetector;
    public MeshTrailEffect TrailEffect => _trailEffect;
    public PlayerStateMachine StateMachine => _stateMachine;
    public DirectionCalculator DirCalculator => _dirCalculator;
    public bool IsHitStopped => _isHitStopped;
    public bool IsPerfectDodgeActive => _isPerfectDodgeActive;
    public bool IsPerfectDodgeWindowOpen { get; private set; }
    public bool IsInvulnerable => _isPerfectDodgeActive || _isStartingDodgeAttack || _stateMachine?.IsInvulnerable == true;
    public EnemyCore PerfectDodgeSource { get; private set; }
    public EnemyCore DodgeAttackTarget { get; private set; }
    public bool IsSkillEnable => (_currentSkillGauge >= _skillUsageGauge); // 스킬 사용 가능

    // 캐싱
    public DamageData LastDamageData { get; private set; } // 마지막에 피해를 입은 데이터

    // 이벤트
    public event Action<DamageData> OnDamaged; // 피해를 입었을 때 호출하는 이벤트
    public event Action<EnemyCore> OnPerfectDodgeStarted;
    public event Action OnPerfectDodgeEnded;
    public event Action<EnemyCore> OnDodgeAttackStarted;
    public Func<EnemyCore> OnPerfectDodgeCheck; // 완벽 회피라면 완벽회피를 하게 한 대상을 반환
    public event Action<float, float> OnHealthChange; // 체력 변화가 있을 때 호출되는 이벤트, <최대 체력, 현재 체력>
    public event Action<float, float> OnSkillGaugeChange; // 스킬 게이지 변화가 있을 때 호출되는 이벤트, <최대 게이지, 현재 게이지>

    private void Awake()
    {
        _baseAnimatorSpeed = _animator.speed;

        // 애니메이터 초기화
        _animatorController.OnAnimationTick += OnAnimationTickLoop; // OnAnimatorMove 틱에 작동하는 함수

        // 체력 초기화
        _currentHealth = _maxHealth;

        // 스킬 게이지 초기화
        _currentSkillGauge = 0f;

        TryGetComponent(out _directorContainer);
        _directorContainer.InitTimelineDirectorContainer();
        TryGetComponent(out _mover);
        TryGetComponent(out _rotator);
        TryGetComponent(out _inputCollector);
        TryGetComponent(out _animationEvent);
        TryGetComponent(out _footPositionDetector);
        TryGetComponent(out _attackInstanceContainer);
        _attackInstanceContainer.OnAttackSkillGaugeAdditive += AddSkillGauge; // 공격 성공마다 스킬 게이지 증가 이벤트

        TryGetComponent(out _targetDetector);
        TryGetComponent(out _trailEffect);
        _stateMachine = new PlayerStateMachine(this);
        _dirCalculator = new DirectionCalculator();
        CombatVfxTime.RegisterHierarchy(gameObject);
    }

    private void OnEnable()
    {
        CombatTimeController.ScaleChanged += ApplyCombatSpeed;
        ApplyCombatSpeed(CombatTimeController.Scale);
    }

    private void Start()
    {
        StateMachine.InitStateMachine(StateMachine.IdleState);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        TickPerfectDodge(Time.unscaledDeltaTime);

        // 입력은 슬로우 배율과 무관하게 먼저 처리합니다. 반격은 기존 히트스탑을 해제하지 않습니다.
        if (IsPerfectDodgeWindowOpen && _inputCollector.IsInputAttack && TryBeginDodgeAttack())
            return;

        // 지연 구간의 공격 입력이 일반 공격으로 새지 않게 합니다.
        if (_isPerfectDodgeActive && !IsPerfectDodgeWindowOpen && _inputCollector.IsInputAttack)
            return;

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

    private void OnAnimationTickLoop()
    {
        if (_isHitStopped)
            return;

        StateMachine.AnimatorTick();
    }

    private void OnDisable()
    {
        EndPerfectDodge(true);
        DodgeAttackTarget = null;
        _targetDetector?.ClearBasicAttackTarget();
        EndHitStop();
        CombatTimeController.ScaleChanged -= ApplyCombatSpeed;
        ApplyCombatSpeed(1f);
    }

    public void BeginHitStop()
    {
        if (_isHitStopped)
            return;

        _isHitStopped = true;
        _stateMachine.ClearAccumulatedMotion();
        _mover.SetHitStopped(true);

        ApplyCombatSpeed(CombatTimeController.Scale);
    }

    public void EndHitStop()
    {
        if (!_isHitStopped)
            return;

        _isHitStopped = false;
        _mover.SetHitStopped(false);
        ApplyCombatSpeed(CombatTimeController.Scale);
    }

    private void ApplyCombatSpeed(float scale)
    {
        float effectiveScale = _isHitStopped ? 0f : scale;
        _animator.speed = _baseAnimatorSpeed * effectiveScale;
        _directorContainer.SetCombatSpeed(effectiveScale);
    }

    // 피해를 입음
    public bool TryTakeDamage(DamageData damageData)
    {
        // 피해 기록, 피격 이벤트, 슬로우 해제보다 먼저 거부합니다.
        // 호출자도 false를 받아 이 타격의 히트스탑을 발생시키지 않습니다.
        if (IsInvulnerable)
            return false;

        EndPerfectDodge(true);
        LastDamageData = damageData;
        OnDamaged?.Invoke(damageData);

        // 체력 감소
        _currentHealth = Mathf.Clamp(_currentHealth - damageData.DamageAmount, 0f, _maxHealth);
        OnHealthChange?.Invoke(_maxHealth, _currentHealth);

        return true;
    }

    // 완벽 회피인지 검사
    public bool IsPerfectDodge(out EnemyCore enemyCore)
    {
        enemyCore = null;
        enemyCore = OnPerfectDodgeCheck?.Invoke();
        return enemyCore != null;
    }

    public void BeginPerfectDodge(EnemyCore source)
    {
        if (source == null || !source.isActiveAndEnabled || source.CurrentHP <= 0f)
            return;

        PerfectDodgeSource = source;
        _isPerfectDodgeActive = true;
        _perfectDodgeStartDelayRemaining = Mathf.Max(0f, _perfectDodge.StartDelay);
        _perfectDodgeDurationRemaining = 0f;
        AddSkillGauge(_perfectDodgeSkillGauge); // 완벽 회피 성공시 스킬 게이지 증가

        if (_perfectDodgeStartDelayRemaining <= 0f)
            OpenPerfectDodgeWindow();
    }

    private void TickPerfectDodge(float realDeltaTime)
    {
        if (!_isPerfectDodgeActive)
            return;

        if (PerfectDodgeSource == null || !PerfectDodgeSource.isActiveAndEnabled || PerfectDodgeSource.CurrentHP <= 0f)
        {
            EndPerfectDodge(true);
            return;
        }

        float remainingDelta = Mathf.Max(0f, realDeltaTime);
        if (!_isPerfectDodgeSlowMotionActive)
        {
            float delayStep = Mathf.Min(_perfectDodgeStartDelayRemaining, remainingDelta);
            _perfectDodgeStartDelayRemaining -= delayStep;
            remainingDelta -= delayStep;
            if (_perfectDodgeStartDelayRemaining > 0f)
                return;

            OpenPerfectDodgeWindow();
        }

        _perfectDodgeDurationRemaining -= remainingDelta;
        if (_perfectDodgeDurationRemaining <= 0f)
            EndPerfectDodge();
    }

    private void OpenPerfectDodgeWindow()
    {
        if (!_isPerfectDodgeActive || _isPerfectDodgeSlowMotionActive)
            return;

        _isPerfectDodgeSlowMotionActive = true;
        IsPerfectDodgeWindowOpen = true;
        _perfectDodgeDurationRemaining = Mathf.Max(0.01f, _perfectDodge.MaxDuration);
        CombatTimeController.Begin(this, _perfectDodge);
        OnPerfectDodgeStarted?.Invoke(PerfectDodgeSource);
        _onPerfectDodgeStarted?.Invoke();
        TrailEffect.ActiveDodgeEffect();
    }

    public void EndPerfectDodge(bool immediate = false)
    {
        bool wasSlowMotionActive = _isPerfectDodgeSlowMotionActive;
        _isPerfectDodgeActive = false;
        _isPerfectDodgeSlowMotionActive = false;
        IsPerfectDodgeWindowOpen = false;
        PerfectDodgeSource = null;
        _perfectDodgeStartDelayRemaining = 0f;
        _perfectDodgeDurationRemaining = 0f;
        if (wasSlowMotionActive)
            CombatTimeController.End(this, _perfectDodge, immediate);
        if (!wasSlowMotionActive)
            return;
        OnPerfectDodgeEnded?.Invoke();
        _onPerfectDodgeEnded?.Invoke();
    }

    public bool TryBeginDodgeAttack()
    {
        if (!IsPerfectDodgeWindowOpen)
            return false;
        EnemyCore target = PerfectDodgeSource;
        if (target == null || !target.isActiveAndEnabled || target.CurrentHP <= 0f)
        {
            EndPerfectDodge(true);
            return false;
        }

        // Exit가 완벽 회피 데이터를 정리하기 전에 반격 대상부터 넘겨받습니다.
        DodgeAttackTarget = target;
        // 종료 이벤트를 알리는 순간에도 회피 → 반격 사이에 무적 공백을 만들지 않습니다.
        _isStartingDodgeAttack = true;
        try
        {
            EndPerfectDodge(true);
            StateMachine.Transition(StateMachine.DodgeAttackStartState);
        }
        finally
        {
            _isStartingDodgeAttack = false;
        }
        OnDodgeAttackStarted?.Invoke(target);
        _onDodgeAttackStarted?.Invoke();
        return true;
    }

    public void ClearDodgeAttackTarget() => DodgeAttackTarget = null;

    // 스킬 게이지 더하기
    public void AddSkillGauge(float additive)
    {
        _currentSkillGauge = Mathf.Min(_currentSkillGauge + additive, 100f);
        OnSkillGaugeChange?.Invoke(100f, _currentSkillGauge);
    }

    // 스킬 게이지 사용
    public void UseSkillGauge()
    {
        _currentSkillGauge = Mathf.Max(_currentSkillGauge - _skillUsageGauge, 0f);
        OnSkillGaugeChange?.Invoke(100f, _currentSkillGauge);
    }
}
