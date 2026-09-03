using System;
using UnityEngine;

[Serializable]
public struct EnemyAttackHitboxInfo
{
    public float DamageAmount;
    public EnemyAttackKnockBackType KnockBackType;
    public EnemyAttackHitboxType HitboxType;
    [Tooltip("Sphere 타입의 레거시 반지름입니다. Size가 0일 때만 사용됩니다.")]
    public Vector3 Radius;
    [Tooltip("Hitbox Transform 아래 Hitbox 오브젝트의 로컬 Scale입니다.")]
    public Vector3 Size;
    [Tooltip("적 로컬 원점에서 Hitbox 오브젝트까지의 로컬 위치입니다.")]
    public Vector3 Offset;
    [Tooltip("공격 예고 판정에만 Size에 더해지는 추가 범위입니다.")]
    public Vector3 AdditiveNoticeRange;
}
