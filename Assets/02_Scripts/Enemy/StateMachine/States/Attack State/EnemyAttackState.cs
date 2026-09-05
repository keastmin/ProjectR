using UnityEngine;

public abstract class EnemyAttackState : EnemyStateBase
{
    protected abstract string AnimationTrigger { get; }

    private bool _isTransitionIdle = false;
    private bool _isDamaged = false;

    public EnemyAttackState(EnemyCore core) : base(core)
    {

    }

    public override void Enter()
    {
        // 초기화
        _isTransitionIdle = false;
        _isDamaged = false;

        // 이벤트 연결
        Core.AnimationEvent.OnAnimationEnd += SetTransitionIdle;
        Core.OnDamaged += SetDamaged;

        // 애니메이션 재생
        Core.Animator.SetTrigger(AnimationTrigger);
    }

    public override void UpdateTick()
    {
        base.UpdateTick();

        if (_isDamaged)
            return;

        if (_isTransitionIdle)
        {
            EnemyStateBase nextState =
                Core.TargetTransform != null
                    ? Core.StateMachine.EngageState
                    : Core.StateMachine.IdleState;

            Core.StateMachine.Transition(nextState);
            return;
        }
    }

    public override void FixedTick()
    {
        AnimDeltaPos.y = 0f;
        Core.Mover.Move(AnimDeltaPos / Time.fixedDeltaTime);
        AnimDeltaPos = Vector3.zero;
    }

    public override void AnimatorTick()
    {
        AnimDeltaPos += Core.Animator.deltaPosition;
    }

    public override void Exit()
    {
        // 초기화
        _isTransitionIdle = false;
        _isDamaged = false;

        // 이벤트 해제
        Core.AnimationEvent.OnAnimationEnd -= SetTransitionIdle;
        Core.OnDamaged -= SetDamaged;

        // 플래그 리셋
        Core.Animator.ResetTrigger(AnimationTrigger);
        Core.EndAttackTargeting();
        Core.ReleaseAttackPermission();
    }

    private void SetTransitionIdle(AnimationEvent animationEvent)
    {
        _isTransitionIdle = true;
    }

    private void SetDamaged(DamageData data)
    {
        if (!Core.ShouldEnterHitReaction(data))
            return;

        _isDamaged = true;

        HitDirectionType type = HitDirectionCalculator.GetHitDirection(data, Core.transform.position, Core.Rotator.FacingDirection);
        if (type == HitDirectionType.Front)
            Core.StateMachine.Transition(Core.StateMachine.FrontHitState);
        else if (type == HitDirectionType.Back)
            Core.StateMachine.Transition(Core.StateMachine.BackHitState);
    }
}
