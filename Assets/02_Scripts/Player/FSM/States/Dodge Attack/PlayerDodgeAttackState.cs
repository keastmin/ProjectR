using UnityEngine;
using UnityEngine.Playables;

public abstract class PlayerDodgeAttackState : PlayerStateBase
{
    protected abstract DirectorID AttackDirectorID { get; }
    protected abstract string AnimationTrigger { get; }
    protected abstract PlayerStateBase NextState{ get; }
    private PlayableDirector _director;
    private bool _isAnimationEnd = false;

    public PlayerDodgeAttackState(PlayerCore core) : base(core) 
    {
        _director = core.DirectorContainer.Directors[AttackDirectorID];
    }

    public override void Enter()
    {
        base.Enter();

        // 초기화
        _isAnimationEnd = false;

        // 이벤트 연결
        Core.AnimationEvent.OnAnimationEnd += HandleAnimationEnd;

        // 애니메이션 재생
        Core.Animator.SetTrigger(AnimationTrigger);

        // 타임라인 재생
        _director.time = 0f;
        _director.Play();
    }

    public override void UpdateTick()
    {
        if (_isAnimationEnd)
        {
            Core.StateMachine.Transition(NextState);
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
        // 초기화
        _isAnimationEnd = false;

        // 이벤트 해제
        Core.AnimationEvent.OnAnimationEnd -= HandleAnimationEnd;

        // 애니메이션 초기화
        Core.Animator.ResetTrigger(AnimationTrigger);

        // 타임라인 초기 중단
        if (_director.state == PlayState.Playing)
            _director.Stop();

        base.Exit();
    }

    private void HandleAnimationEnd()
    {
        _isAnimationEnd = true;
    }
}