using System;
using UnityEngine;

[Serializable]
public struct EnemyAttackHitboxInfo
{
    public float DamageAmount;
    public EnemyAttackKnockBackType KnockBackType;
    public EnemyAttackHitboxType HitboxType;
    public Vector3 Center;
    public Vector3 Size;
    public Vector3 Radius;
}
