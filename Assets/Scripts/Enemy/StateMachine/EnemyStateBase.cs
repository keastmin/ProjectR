using UnityEngine;

public class EnemyStateBase
{
    protected EnemyCore Core;
    protected Vector3 AnimDeltaPos = Vector3.zero;

    public EnemyStateBase(EnemyCore enemy)
    {
        Core = enemy;
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

    public void ClearAccumulatedMotion()
    {
        AnimDeltaPos = Vector3.zero;
    }

    public virtual void Exit()
    {

    }
}
