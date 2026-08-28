using System;
using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    public virtual void Configure(EnemyAttackHitboxInfo hitboxInfo)
    {
        throw new NotSupportedException($"{GetType().Name} does not support this hitbox configuration.");
    }

    public virtual Collider[] DetectTargets(LayerMask targetLayers)
    {
        throw new NotSupportedException($"{GetType().Name} does not support hit detection.");
    }

    protected static Vector3 Abs(Vector3 value)
    {
        return new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
    }
}
