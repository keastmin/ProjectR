using System;
using UnityEngine;

[Serializable]
public sealed class PerfectDodgeSettings
{
    [Tooltip("완벽 회피 중 전투 재생 배율. 0은 종료 애니메이션 이벤트가 실행되지 않으므로 허용하지 않습니다.")]
    [Range(0.01f, 1f)] public float SlowScale = 0.2f;

    [Tooltip("완벽 회피 판정 후 슬로우와 회피 반격 입력 구간을 여는 데까지의 지연 (실제 초). 이 동안도 무적은 즉시 적용됩니다.")]
    [Min(0f)] public float StartDelay = 0.07f;

    [Tooltip("슬로우와 회피 반격 입력 구간의 최대 유지 시간 (실제 초). 애니메이션 종료 이벤트가 먼저 오면 그때 종료합니다.")]
    [Min(0.01f)] public float MaxDuration = 1.65f;

    [Tooltip("슬로우 진입 시간 (실제 초). 0이면 즉시 적용합니다.")]
    [Min(0f)] public float FadeInDuration = 0.08f;
    public AnimationCurve FadeInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Tooltip("완벽 회피 종료 후 정상 속도로 복귀하는 시간 (실제 초). 반격 시에는 무시하고 즉시 해제합니다.")]
    [Min(0f)] public float FadeOutDuration = 0.15f;
    public AnimationCurve FadeOutCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
}
