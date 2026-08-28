using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAttackSO", menuName = "Scriptable Objects/Enemy Attack SO")]
public class EnemyAttackSO : ScriptableObject
{
    public EnemyAttackID AttackID;
    [Min(0), Tooltip("60Hz 전투 프레임 기준 공격 전체의 히트스탑 길이입니다. 피격자는 자동으로 1프레임 더 정지합니다.")]
    public int HitStopFrame;
    public LayerMask DamagedLayer;
    public EnemyAttackHitboxInfo[] HitboxInfo;
}
