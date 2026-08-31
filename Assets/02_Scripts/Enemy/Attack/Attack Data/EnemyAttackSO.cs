using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAttackSO", menuName = "Scriptable Objects/Enemy Attack SO")]
public class EnemyAttackSO : ScriptableObject
{
    public EnemyAttackID AttackID;
    [Min(0), Tooltip("60Hz 전투 프레임 기준 공격 전체의 히트스탑 길이입니다. 피격자는 자동으로 1프레임 더 정지합니다.")]
    public int HitStopFrame;
    public LayerMask DamagedLayer;
    public EnemyAttackHitboxInfo[] HitboxInfo;
    public AnimationClip AttackAnimationClip; // 공격을 하는 애니메이션 클립
    public float RootMotionMultiply; // 실제 이동에 관여하는 루트 모션에 곱하는 값
    public int SimulateStup; // 몇 번 시뮬레이션 할 것인지
    public string AttackFunctionName; // 애니메이션 클립 안에서 공격 함수 이름
    public float AttackAnimationTransitionTime; // 공격 대기 시간
    public int AttackOfNumber; // 애니메이션 클립 안에서 몇 번째 히트인지 -> 처음: 0
}
