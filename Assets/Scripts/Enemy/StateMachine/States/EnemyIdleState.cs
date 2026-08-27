using UnityEngine;

public class EnemyIdleState : EnemyStateBase
{
    private bool _isDamaged = false;

    public EnemyIdleState(EnemyCore enemy) : base(enemy)
    {

    }

    public override void Enter()
    {
        // 초기화
        _isDamaged = false;

        // 이벤트 연결
        Core.OnDamaged += SetDamaged;

        Core.Animator.SetTrigger("IsIdle");
    }

    public override void UpdateTick()
    {
        if (_isDamaged)
            return;

        if(Core.TargetTransform != null)
        {
            Core.StateMachine.Transition(Core.StateMachine.EngageState);
            return;
        }
    }

    public override void FixedTick()
    {
        Core.Mover.Move(Core.Rotator.FacingRotation * (AnimDeltaPos / Time.fixedDeltaTime));
        AnimDeltaPos = Vector3.zero;
    }

    public override void AnimatorTick()
    {
        AnimDeltaPos += Core.Animator.deltaPosition;
    }

    public override void Exit()
    {
        // 초기화
        _isDamaged = false;

        // 이벤트 해제
        Core.OnDamaged -= SetDamaged;

        Core.Animator.ResetTrigger("IsIdle");
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
}