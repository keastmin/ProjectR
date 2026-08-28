using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class PlayerCore : MonoBehaviour, IHitStopParticipant
{
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Animator _animator;
    [SerializeField] private PlayerAnimatorController _animatorController;

    // 컴포넌트
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

    private readonly List<PlayableSpeedSnapshot> _hitStoppedPlayables = new();
    private bool _isHitStopped;
    private float _animatorSpeedBeforeHitStop = 1f;

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
    public bool IsHitStopped => _isHitStopped;

    private void Awake()
    {
        // 애니메이터 초기화
        _animatorController.OnAnimationTick += OnAnimationTickLoop; // OnAnimatorMove 틱에 작동하는 함수

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
        EndHitStop();
    }

    public void BeginHitStop()
    {
        if (_isHitStopped)
            return;

        _isHitStopped = true;
        _stateMachine.ClearAccumulatedMotion();
        _mover.SetHitStopped(true);

        _animatorSpeedBeforeHitStop = _animator.speed;
        _animator.speed = 0f;

        _hitStoppedPlayables.Clear();
        foreach (PlayableDirector director in _directorContainer.Directors.Values)
        {
            if (director == null || director.state != PlayState.Playing || !director.playableGraph.IsValid())
                continue;

            PlayableGraph graph = director.playableGraph;
            int rootPlayableCount = graph.GetRootPlayableCount();
            for (int i = 0; i < rootPlayableCount; i++)
            {
                Playable rootPlayable = graph.GetRootPlayable(i);
                _hitStoppedPlayables.Add(new PlayableSpeedSnapshot(rootPlayable, rootPlayable.GetSpeed()));
                rootPlayable.SetSpeed(0d);
            }
        }
    }

    public void EndHitStop()
    {
        if (!_isHitStopped)
            return;

        _isHitStopped = false;
        _animator.speed = _animatorSpeedBeforeHitStop;
        _mover.SetHitStopped(false);

        for (int i = 0; i < _hitStoppedPlayables.Count; i++)
        {
            PlayableSpeedSnapshot snapshot = _hitStoppedPlayables[i];
            if (snapshot.Playable.IsValid())
                snapshot.Playable.SetSpeed(snapshot.Speed);
        }

        _hitStoppedPlayables.Clear();
    }

    private readonly struct PlayableSpeedSnapshot
    {
        public readonly Playable Playable;
        public readonly double Speed;

        public PlayableSpeedSnapshot(Playable playable, double speed)
        {
            Playable = playable;
            Speed = speed;
        }
    }
}
