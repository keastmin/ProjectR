using UnityEngine;

public abstract class PlayerHitState : PlayerStateBase
{
    private bool _isTransitionIdle = false;

    protected abstract string AnimationTrigger { get; }

    public PlayerHitState(PlayerCore core) : base(core)
    {

    }

    public override void Enter()
    {
        // 초기화
        _isTransitionIdle = false;

        // 이벤트 연결
        Core.AnimationEvent.OnAnimationEnd += SetTransitionIdle;

        // 애니메이션 재생
        Core.Animator.SetTrigger(AnimationTrigger);

        base.Enter();
    }

    public override void UpdateTick()
    {
        if (IsDamaged)
            return;

        // Idle 전환 트리거 활성화 시 Idle로 전환
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

        // 바라보는 방향의 Z축으로만 이동
        MoveRootMotionAlongFacingDirection();
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

        // 이벤트 해제
        Core.AnimationEvent.OnAnimationEnd -= SetTransitionIdle;

        // 애니메이션 트리거 초기화
        Core.Animator.ResetTrigger(AnimationTrigger);

        base.Exit();
    }

    protected void RotateImmediatelyForHitReaction(bool faceAttacker)
    {
        GameObject sender = Core.LastDamageData.Sender;
        if (sender == null)
            return;

        Vector3 directionToAttacker = sender.transform.position - Core.transform.position;
        Core.Rotator.RotateImmediately(faceAttacker ? directionToAttacker : -directionToAttacker);
    }

    private void SetTransitionIdle()
    {
        _isTransitionIdle = true;
    }
}
