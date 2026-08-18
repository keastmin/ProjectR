using UnityEngine;

public class PlayerStateBase
{
    protected PlayerCore Core;

    public PlayerStateBase(PlayerCore player)
    {
        Core = player;
    }

    public virtual void Enter()
    {

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

    }
}