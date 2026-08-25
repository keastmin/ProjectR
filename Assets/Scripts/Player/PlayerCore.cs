using UnityEngine;
using UnityEngine.Playables;

public class PlayerCore : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera;

    // 컴포넌트
    private Animator _animator;
    private TimelineDirectorContainer _directorContainer;
    private PlayerMover _mover;
    private PlayerRotator _rotator;
    private PlayerInputCollector _inputCollector;
    private PlayerAnimationEvent _animationEvent;
    private FootPositionDetector _footPositionDetector;
    private PlayerAttackInstanceContainer _attackInstanceContainer;
    private PlayerTargetDetector _targetDetector;

    // 기능
    private PlayerStateMachine _stateMachine;
    private DirectionCalculator _dirCalculator;

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
    public PlayerStateMachine StateMachine => _stateMachine;
    public DirectionCalculator DirCalculator => _dirCalculator;

    private void Awake()
    {
        TryGetComponent(out _animator);
        TryGetComponent(out _directorContainer);
        _directorContainer.InitTimelineDirectorContainer();
        TryGetComponent(out _mover);
        TryGetComponent(out _rotator);
        TryGetComponent(out _inputCollector);
        TryGetComponent(out _animationEvent);
        TryGetComponent(out _footPositionDetector);
        TryGetComponent(out _attackInstanceContainer);
        TryGetComponent(out _targetDetector);
        _stateMachine = new PlayerStateMachine(this);
        _dirCalculator = new DirectionCalculator();
    }

    private void Start()
    {
        StateMachine.InitStateMachine(StateMachine.IdleState);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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

    private void OnAnimatorMove()
    {
        StateMachine.AnimatorTick();
    }
}