using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAttackSO", menuName = "Scriptable Objects/Enemy Attack SO")]
public class EnemyAttackSO : ScriptableObject
{
    public EnemyAttackID AttackID;
    [Min(0), Tooltip("60Hz 전투 프레임 기준 공격 전체의 히트스탑 길이입니다. 피격자는 자동으로 1프레임 더 정지합니다.")]
    public int HitStopFrame;
    public LayerMask DamagedLayer;
    public EnemyAttackHitboxInfo[] HitboxInfo;
    [Min(0f), Tooltip("공격 예고 후 공격 애니메이션으로 전환하기까지의 시간입니다.")]
    public float AttackAnimationTransitionTime;
    [Min(0f), Tooltip("공격 시작 위치에서 허용되는 최대 모션 워핑 거리입니다.")]
    public float AttackWarpDistance;
    [Min(0f), Tooltip("공격 위치가 플레이어를 추적할 때 허용되는 초당 최대 회전 각도입니다.")]
    public float AttackRotationAnglePerSecond;

    /// <summary>
    /// Maximum planar distance from the attacker origin at which this attack can
    /// touch a target. AttackWarpDistance moves the attacker root; the hitbox's
    /// forward offset and extent provide the remaining reach.
    /// </summary>
    public float GetMaximumAttackReach()
    {
        float forwardHitboxReach = 0f;

        if (HitboxInfo != null)
        {
            foreach (EnemyAttackHitboxInfo hitbox in HitboxInfo)
            {
                Vector3 size = new(
                    Mathf.Abs(hitbox.Size.x),
                    Mathf.Abs(hitbox.Size.y),
                    Mathf.Abs(hitbox.Size.z));

                float forwardExtent = hitbox.HitboxType == EnemyAttackHitboxType.Sphere
                    ? Mathf.Max(size.x, size.y, size.z) * 0.5f
                    : size.z * 0.5f;

                if (forwardExtent <= 0f && hitbox.HitboxType == EnemyAttackHitboxType.Sphere)
                    forwardExtent = Mathf.Max(0f, hitbox.Radius.x);

                forwardHitboxReach = Mathf.Max(
                    forwardHitboxReach,
                    hitbox.Offset.z + forwardExtent);
            }
        }

        return Mathf.Max(0f, AttackWarpDistance) + Mathf.Max(0f, forwardHitboxReach);
    }
}
