using System.Xml;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public abstract class EnemyStateBase
{
    protected EnemyCore Core;
    protected Vector3 AnimDeltaPos = Vector3.zero;

    protected EnemyStateBase(EnemyCore enemy)
    {
        Core = enemy;
    }

    public virtual void Enter()
    {

    }

    public virtual void UpdateTick()
    {
        if (Core.IsDead)
        {
            Core.StateMachine.Transition(Core.StateMachine.DeadState);
            return;
        }
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

    public virtual void ClearAccumulatedMotion()
    {
        AnimDeltaPos = Vector3.zero;
    }
}
