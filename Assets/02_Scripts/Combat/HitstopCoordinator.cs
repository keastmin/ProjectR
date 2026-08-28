using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-10000)]
public sealed class HitstopCoordinator : MonoBehaviour
{
    public const int CombatFrameRate = 60;

    private static HitstopCoordinator _instance;

    private readonly Dictionary<IHitStopParticipant, double> _releaseTimes = new();
    private readonly List<IHitStopParticipant> _releaseBuffer = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        EnsureInstance();
    }

    public static void Request(
        IHitStopParticipant attacker,
        IReadOnlyList<IHitStopParticipant> victims,
        int attackerFrameCount)
    {
        if (attackerFrameCount <= 0 || attacker == null || victims == null || victims.Count == 0)
            return;

        HitstopCoordinator coordinator = EnsureInstance();
        double now = Time.realtimeSinceStartupAsDouble;
        double frameDuration = 1d / CombatFrameRate;

        coordinator.Hold(attacker, now + attackerFrameCount * frameDuration);

        double victimReleaseTime = now + (attackerFrameCount + 1) * frameDuration;
        for (int i = 0; i < victims.Count; i++)
        {
            IHitStopParticipant victim = victims[i];
            if (victim != null)
                coordinator.Hold(victim, victimReleaseTime);
        }
    }

    private static HitstopCoordinator EnsureInstance()
    {
        if (_instance != null)
            return _instance;

        _instance = FindAnyObjectByType<HitstopCoordinator>();
        if (_instance != null)
            return _instance;

        GameObject coordinatorObject = new GameObject(nameof(HitstopCoordinator));
        DontDestroyOnLoad(coordinatorObject);
        _instance = coordinatorObject.AddComponent<HitstopCoordinator>();
        return _instance;
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (_releaseTimes.Count == 0)
            return;

        double now = Time.realtimeSinceStartupAsDouble;
        _releaseBuffer.Clear();

        foreach (KeyValuePair<IHitStopParticipant, double> pair in _releaseTimes)
        {
            if (pair.Value <= now || IsDestroyed(pair.Key))
                _releaseBuffer.Add(pair.Key);
        }

        for (int i = 0; i < _releaseBuffer.Count; i++)
        {
            IHitStopParticipant participant = _releaseBuffer[i];
            _releaseTimes.Remove(participant);

            if (!IsDestroyed(participant))
                participant.EndHitStop();
        }
    }

    private void OnDestroy()
    {
        if (_instance != this)
            return;

        foreach (IHitStopParticipant participant in _releaseTimes.Keys)
        {
            if (!IsDestroyed(participant))
                participant.EndHitStop();
        }

        _releaseTimes.Clear();
        _releaseBuffer.Clear();
        _instance = null;
    }

    private void Hold(IHitStopParticipant participant, double releaseTime)
    {
        if (IsDestroyed(participant))
            return;

        if (_releaseTimes.TryGetValue(participant, out double currentReleaseTime))
        {
            if (releaseTime > currentReleaseTime)
                _releaseTimes[participant] = releaseTime;

            return;
        }

        _releaseTimes.Add(participant, releaseTime);
        participant.BeginHitStop();
    }

    private static bool IsDestroyed(IHitStopParticipant participant)
    {
        return participant == null || participant is Object unityObject && unityObject == null;
    }
}
