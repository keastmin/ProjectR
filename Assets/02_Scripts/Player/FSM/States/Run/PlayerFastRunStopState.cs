using UnityEngine;

public class PlayerFastRunStopState : PlayerStateBase
{
    private const float FastRunTurnInputWindow = 0.15f;
    private const float OppositeDirectionDotThreshold = -0.8f;

    private bool _isTransitionIdle = false;
    private float _currentStateTime = 0f;
    private Vector3 _runDirection = Vector3.forward;

    public PlayerFastRunStopState(PlayerCore player) : base(player)
    {

    }

    public override void Enter()
    {
        base.Enter();

        Debug.Log("PlayerFastRunStopState 진입");
        // 초기화
        _isTransitionIdle = false;
        _currentStateTime = 0f;

        // 빠른 달리기를 멈추기 직전에 이동하던 방향을 저장
        _runDirection = Core.Rotator.FacingRotation * Vector3.forward;

        // 이벤트 연결
        Core.AnimationEvent.OnAnimationEnd += SetTransitionIdle;

        // 애니메이션 재생
        Core.Animator.SetTrigger("IsFastRunStop");
    }

    public override void UpdateTick()
    {
        if (IsDamaged)
            return;

        _currentStateTime += CombatTimeController.DeltaTime;

        // 회피 입력이 있으면 뒤로 회피로 전환
        if (Core.InputCollector.IsInputDodge)
        {
            Core.StateMachine.Transition(Core.StateMachine.BackDodgeState);
            return;
        }

        if (Core.InputCollector.IsInputSkill && Core.IsSkillEnable)
        {
            Core.StateMachine.Transition(Core.StateMachine.SkillState);
            return;
        }

        // 기본 공격 입력이 있으면 기본 공격으로 전환
        if (Core.InputCollector.IsInputAttack)
        {
            Core.StateMachine.Transition(Core.StateMachine.BasicAttack1State);
            return;
        }

        // 정지 직후 일정 시간 안에 반대 방향 입력이 들어오면 빠른 달리기 회전으로 전환
        if (TryTransitionToFastRunTurn())
            return;

        // 이동 입력이 있으면 달리기 시작으로 전환
        if (Core.InputCollector.IsInputMove)
        {
            Core.StateMachine.Transition(Core.StateMachine.RunStartState);
            return;
        }

        // Idle 전환 플래그가 활성화 되면 Idle로 전환
        if (_isTransitionIdle)
        {
            Core.StateMachine.Transition(Core.StateMachine.IdleState);
            return;
        }
    }

    public override void FixedTick()
    {
        if (IsDamaged)
            return;

        Core.Mover.Move(AnimDeltaPos / Time.fixedDeltaTime);
        AnimDeltaPos = Vector3.zero;
    }

    public override void AnimatorTick()
    {
        if (IsDamaged)
            return;

        AnimDeltaPos += Core.Animator.deltaPosition;
    }

    public override void Exit()
    {
        // 초기화
        _isTransitionIdle = false;
        _currentStateTime = 0f;

        // 이벤트 해제
        Core.AnimationEvent.OnAnimationEnd -= SetTransitionIdle;

        base.Exit();
    }

    private void SetTransitionIdle()
    {
        _isTransitionIdle = true;
    }

    private bool TryTransitionToFastRunTurn()
    {
        if (_currentStateTime > FastRunTurnInputWindow || !Core.InputCollector.IsInputMove)
            return false;

        Vector3 inputDirection = Core.DirCalculator.GetTargetDirection(
            Core.InputCollector.MoveValue,
            Core.MainCamera.transform);

        bool isOppositeDirection = Vector3.Dot(_runDirection, inputDirection) <= OppositeDirectionDotThreshold;
        if (!isOppositeDirection)
            return false;

        Core.StateMachine.Transition(Core.StateMachine.FastRunTurnState);
        return true;
    }
}
