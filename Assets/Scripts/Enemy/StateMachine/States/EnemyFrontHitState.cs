using UnityEngine;

public class EnemyFrontHitState : EnemyStateBase
{
    private static readonly int HitStateHash = Animator.StringToHash("Base Layer.Hit.Hit Front");

    private bool _isTransitionIdle = false;
    private bool _isDamaged = false;
    private DamageData _damageData;

    public EnemyFrontHitState(EnemyCore enemy) : base(enemy)
    {

    }

    public override void Enter()
    {
        // 초기화
        _isTransitionIdle = false;
        _isDamaged = false;

        // 데미지 데이터 캐싱
        _damageData = Core.LastDamageData;

        // 즉시 회전
        Vector3 dir = _damageData.Sender.transform.position - Core.transform.position;
        Core.Rotator.RotateImmediately(dir);

        // 이벤트 연결
        Core.OnDamaged += SetDamaged;
        Core.AnimationEvent.OnFrontHitEnd += SetTransitionIdle;

        Core.PlayHitReaction(HitStateHash);
    }

    public override void UpdateTick()
    {
        if (_isDamaged)
            return;

        // Idle 전환 플래그가 활성화되면 Idle로 전환
        if (_isTransitionIdle)
        {
            Core.StateMachine.Transition(Core.StateMachine.IdleState);
            return;
        }
    }

    public override void FixedTick()
    {
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
        Core.OnDamaged -= SetDamaged;
        Core.AnimationEvent.OnFrontHitEnd -= SetTransitionIdle;
    }

    private void SetDamaged(DamageData data)
    {
        _isDamaged = true;
        HitDirectionType type = HitDirectionCalculator.GetHitDirection(data, Core.transform.position, Core.Rotator.FacingDirection);
        if (type == HitDirectionType.Front)
            Core.StateMachine.Transition(Core.StateMachine.FrontHitState);
        else if (type == HitDirectionType.Back)
            Core.StateMachine.Transition(Core.StateMachine.BackHitState);
    }

    private void SetTransitionIdle()
    {
        _isTransitionIdle = true;
    }
}
