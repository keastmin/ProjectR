using UnityEngine;
using UnityEngine.Playables;

public class PlayerRunAttackState : PlayerStateBase
{
    private PlayableDirector _director;
    private bool _isTransitionIdle = false;
    private bool _isEnableOtherBehaviour = false;

    public PlayerRunAttackState(PlayerCore player) : base(player)
    {
        _director = player.DirectorContainer.Directors[DirectorID.RunAttack];
    }

    public override void Enter()
    {
        // 초기화
        _isTransitionIdle = false;
        _isEnableOtherBehaviour = false;

        // 이벤트 연결
        Core.AnimationEvent.OnEnableOtherBehaviour += SetEnableOtherBehaviour;
        Core.AnimationEvent.OnAnimationEnd += SetTransitionIdle;

        // 즉시 회전
        Vector3 targetDirection = Core.TargetDetector.NearestEnemyCollider == null ?
            Core.DirCalculator.GetTargetDirection(Core.InputCollector.MoveValue, Core.MainCamera.transform) :
            Core.TargetDetector.NearestEnemyDirection;
        Core.Rotator.RotateImmediately(targetDirection);

        // 애니메이션 재생
        Core.Animator.SetTrigger("IsRunAttack");

        // 타임라인 재생
        Core.DirectorContainer.Play(DirectorID.RunAttack);
    }

    public override void UpdateTick()
    {
        // 회피 입력이 있다면 회피 상태로 전환
        if (Core.InputCollector.IsInputDodge)
        {
            if (Core.InputCollector.IsInputMove)
                Core.StateMachine.Transition(Core.StateMachine.FrontDodgeState);
            else
                Core.StateMachine.Transition(Core.StateMachine.BackDodgeState);
            return;
        }

        // 다른 행동 가능 플래그가 활성화 되면 공격이나 이동으로 전환
        if (_isEnableOtherBehaviour)
        {
            if (Core.InputCollector.IsInputAttack)
            {
                Core.StateMachine.Transition(Core.StateMachine.BasicAttack1State);
                return;
            }

            if(Core.InputCollector.IsInputMove)
            {
                Core.StateMachine.Transition(Core.StateMachine.RunStartState);
                return;
            }
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
        MoveRootMotionAlongFacingDirection();
    }

    public override void AnimatorTick()
    {
        AnimDeltaPos += Core.Animator.deltaPosition;
    }

    public override void Exit()
    {
        // 타임라인 종료
        if (_director.state == PlayState.Playing)
            _director.Stop();

        // 초기화
        _isTransitionIdle = false;
        _isEnableOtherBehaviour = false;

        // 이벤트 해제
        Core.AnimationEvent.OnEnableOtherBehaviour -= SetEnableOtherBehaviour;
        Core.AnimationEvent.OnAnimationEnd -= SetTransitionIdle;
    }

    private void SetTransitionIdle()
    {
        _isTransitionIdle = true;
    }

    private void SetEnableOtherBehaviour()
    {
        _isEnableOtherBehaviour = true;
    }
}
