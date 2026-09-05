using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimelineDirectorContainer : MonoBehaviour
{
    [SerializeField] private DirectorInfo[] _directorInfos;

    public readonly Dictionary<DirectorID, PlayableDirector> Directors = new();

    private readonly Dictionary<DirectorID, HitStopParticleMode> _particleModes = new();
    private readonly List<ParticleControlSetting> _particleControlSettings = new();
    private readonly Dictionary<Playable, double> _basePlayableSpeeds = new();
    private readonly List<Playable> _invalidPlayables = new();
    private float _combatSpeed = 1f;

    public void InitTimelineDirectorContainer()
    {
        foreach (var previous in Directors.Values)
            if (previous != null)
                previous.played -= OnDirectorPlayed;
        Directors.Clear();
        _particleModes.Clear();

        foreach (DirectorInfo info in _directorInfos)
        {
            Directors.Add(info.ID, info.Director);
            _particleModes.Add(info.ID, info.ParticleMode);
            if (info.Director != null)
                info.Director.played += OnDirectorPlayed;
        }
    }

    public void Play(DirectorID id)
    {
        if (!Directors.TryGetValue(id, out PlayableDirector director) || director == null)
        {
            Debug.LogWarning($"{id}에 해당하는 PlayableDirector가 없습니다.", this);
            return;
        }

        HitStopParticleMode particleMode = _particleModes[id];
        ConfigureParticleControl(director, particleMode);

        try
        {
            director.time = 0d;
            director.Play();
            ApplyDirectorSpeed(director);
        }
        finally
        {
            RestoreParticleControlSettings();
        }
    }

    private void ConfigureParticleControl(PlayableDirector director, HitStopParticleMode mode)
    {
        _particleControlSettings.Clear();

        if (director.playableAsset is not TimelineAsset timeline)
            return;

        bool timelineControlsParticles = mode == HitStopParticleMode.FreezeWithHitStop;
        foreach (TrackAsset rootTrack in timeline.GetRootTracks())
            ConfigureTrackParticleControl(rootTrack, timelineControlsParticles, director);
    }

    private void ConfigureTrackParticleControl(TrackAsset track, bool timelineControlsParticles, PlayableDirector director)
    {
        foreach (TimelineClip clip in track.GetClips())
        {
            if (clip.asset is not ControlPlayableAsset controlAsset)
                continue;

            _particleControlSettings.Add(new ParticleControlSetting(controlAsset, controlAsset.updateParticle));
            controlAsset.updateParticle = timelineControlsParticles;
            GameObject source = controlAsset.sourceGameObject.Resolve(director);
            if (source != null)
                CombatVfxTime.RegisterHierarchy(source, timelineControlsParticles,
                    GetComponentInParent<IHitStopParticipant>(), timelineControlsParticles);
        }

        foreach (TrackAsset childTrack in track.GetChildTracks())
            ConfigureTrackParticleControl(childTrack, timelineControlsParticles, director);
    }

    public void SetCombatSpeed(float speed)
    {
        _combatSpeed = speed;
        _invalidPlayables.Clear();
        foreach (var playable in _basePlayableSpeeds.Keys)
            if (!playable.IsValid())
                _invalidPlayables.Add(playable);
        foreach (var playable in _invalidPlayables)
            _basePlayableSpeeds.Remove(playable);
        foreach (var director in Directors.Values)
            ApplyDirectorSpeed(director);
    }

    private void OnDirectorPlayed(PlayableDirector director) => ApplyDirectorSpeed(director);

    private void ApplyDirectorSpeed(PlayableDirector director)
    {
        if (director == null || !director.playableGraph.IsValid())
            return;
        var graph = director.playableGraph;
        for (int i = 0; i < graph.GetRootPlayableCount(); i++)
        {
            var root = graph.GetRootPlayable(i);
            if (!_basePlayableSpeeds.TryGetValue(root, out double baseSpeed))
            {
                baseSpeed = root.GetSpeed();
                _basePlayableSpeeds.Add(root, baseSpeed);
            }
            root.SetSpeed(baseSpeed * _combatSpeed);
        }
    }

    private void OnDestroy()
    {
        foreach (var director in Directors.Values)
            if (director != null)
                director.played -= OnDirectorPlayed;
    }

    private void RestoreParticleControlSettings()
    {
        for (int i = 0; i < _particleControlSettings.Count; i++)
        {
            ParticleControlSetting setting = _particleControlSettings[i];
            setting.Asset.updateParticle = setting.OriginalValue;
        }

        _particleControlSettings.Clear();
    }

    private readonly struct ParticleControlSetting
    {
        public readonly ControlPlayableAsset Asset;
        public readonly bool OriginalValue;

        public ParticleControlSetting(ControlPlayableAsset asset, bool originalValue)
        {
            Asset = asset;
            OriginalValue = originalValue;
        }
    }
}
