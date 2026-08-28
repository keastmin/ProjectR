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

    public void InitTimelineDirectorContainer()
    {
        Directors.Clear();
        _particleModes.Clear();

        foreach (DirectorInfo info in _directorInfos)
        {
            Directors.Add(info.ID, info.Director);
            _particleModes.Add(info.ID, info.ParticleMode);
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
            ConfigureTrackParticleControl(rootTrack, timelineControlsParticles);
    }

    private void ConfigureTrackParticleControl(TrackAsset track, bool timelineControlsParticles)
    {
        foreach (TimelineClip clip in track.GetClips())
        {
            if (clip.asset is not ControlPlayableAsset controlAsset)
                continue;

            _particleControlSettings.Add(new ParticleControlSetting(controlAsset, controlAsset.updateParticle));
            controlAsset.updateParticle = timelineControlsParticles;
        }

        foreach (TrackAsset childTrack in track.GetChildTracks())
            ConfigureTrackParticleControl(childTrack, timelineControlsParticles);
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
