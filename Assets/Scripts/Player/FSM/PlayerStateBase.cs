using UnityEngine;

public class PlayerStateBase
{
    protected PlayerCore Core;
    protected Vector3 AnimDeltaPos = Vector3.zero;

    public PlayerStateBase(PlayerCore player)
    {
        Core = player;
    }

    public virtual void Enter()
    {
        Debug.Log("PlayerStateBase 진입");
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

    public void ClearAccumulatedMotion()
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

    public virtual void Exit()
    {

    }
}
