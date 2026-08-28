using UnityEngine;

public class PlayerStateBase
{
    protected PlayerCore Core;
    protected Vector3 AnimDeltaPos = Vector3.zero;

    protected bool IsDamaged { get; private set; } = false;

    public PlayerStateBase(PlayerCore player)
    {
        Core = player;
    }

    public virtual void Enter()
    {
        // 초기화
        IsDamaged = false;

        // 이벤트 연결
        Core.OnDamaged += HandleDamaged;
    }

    public virtual void UpdateTick()
    {

    }

    public virtual void FixedTick()
    {

    }

    public virtual void LateTick()
    {

    }

    public virtual void AnimatorTick()
    {

    }

    public virtual void Exit()
    {
        // 초기화
        IsDamaged = false;

        // 이벤트 해제
        Core.OnDamaged -= HandleDamaged;
    }

    // 데미지 받았을 때 호출되는 함수
    private void HandleDamaged(DamageData damageData)
    {
        IsDamaged = true;

        HitDirectionType type = HitDirectionCalculator.GetHitDirection(damageData, Core.transform.position, Core.Rotator.FacingDirection);
        if (type == HitDirectionType.Front)
            Core.StateMachine.Transition(Core.StateMachine.HitFrontState);
        else if (type == HitDirectionType.Back)
            Core.StateMachine.Transition(Core.StateMachine.HitBackState);
    }

    public virtual void ClearAccumulatedMotion()
    {
        AnimDeltaPos = Vector3.zero;
    }

    protected void MoveRootMotionAlongFacingDirection()
    {
        Vector3 forward = Core.Rotator.FacingDirection;
        forward.y = 0f;
        forward.Normalize();

        float forwardDelta = Vector3.Dot(AnimDeltaPos, forward);

        Core.Mover.Move(forward * forwardDelta / Time.fixedDeltaTime);
        AnimDeltaPos = Vector3.zero;
    }
}
