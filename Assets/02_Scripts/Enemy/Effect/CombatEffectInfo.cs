using System;
using UnityEngine;

[Serializable]
public struct CombatEffectPoolInfo
{
    [Tooltip("요청할 때 사용할 이펙트 타입입니다.")]
    public CombatEffectID ID;

    [Tooltip("ParticleSystem 또는 VisualEffect를 하나 이상 포함한 프리팹입니다.")]
    public GameObject Prefab;

    [Min(0), Tooltip("씬 시작 시 미리 생성할 개수입니다.")]
    public int PrewarmCount;

    [Min(1), Tooltip("동시에 사용할 수 있는 최대 개수입니다. 모두 사용 중이면 가장 오래된 이펙트를 재사용합니다.")]
    public int MaxSize;

    [Min(0.01f), Tooltip("재생 후 Stop/Clear하고 풀로 돌려보낼 때까지의 시간(초)입니다.")]
    public float PlaybackDuration;
}

/// <summary>
/// 이펙트 사용자와 풀 구현 사이에서 전달되는 값 객체입니다.
/// </summary>
public readonly struct CombatEffectRequest
{
    public CombatEffectID EffectType { get; }
    public Vector3 Position { get; }
    public Quaternion Rotation { get; }
    public Transform FollowTarget { get; }

    public CombatEffectRequest(
        CombatEffectID effectType,
        Vector3 position,
        Quaternion rotation,
        Transform followTarget = null)
    {
        EffectType = effectType;
        Position = position;
        Rotation = rotation;
        FollowTarget = followTarget;
    }
}

/// <summary>
/// 호출 측은 이 요청 창구만 알고, 요청을 처리하는 풀이 무엇인지는 알지 못합니다.
/// </summary>
public static class CombatEffectRequestBus
{
    public static event Action<CombatEffectRequest> Requested;

    public static void Request(CombatEffectID effectType, Vector3 position, Quaternion rotation)
    {
        Requested?.Invoke(new CombatEffectRequest(effectType, position, rotation));
    }

    /// <summary>
    /// 대상의 현재 위치와 회전에서 재생하고, 풀에 반환될 때까지 대상을 따라갑니다.
    /// </summary>
    public static void Request(CombatEffectID effectType, Transform followTarget)
    {
        if (followTarget == null)
        {
            Debug.LogWarning($"{effectType} 이펙트의 추적 대상이 null입니다.");
            return;
        }

        Requested?.Invoke(new CombatEffectRequest(
            effectType,
            followTarget.position,
            followTarget.rotation,
            followTarget));
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ResetSubscribers()
    {
        Requested = null;
    }
}
