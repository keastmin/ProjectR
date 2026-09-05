using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public sealed class PlayerOpacityClip : PlayableAsset, ITimelineClipAsset
{
    [Range(0f, 1f)]
    [Tooltip("클립 중앙의 불투명도. 0은 완전 투명, 1은 원래 모습입니다. 양쪽 Blend 길이로 사라지고 나타나는 시간을 조절합니다.")]
    public float Opacity;

    public ClipCaps clipCaps => ClipCaps.Blending;
    public override double duration => 13d / 60d;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<PlayerOpacityBehaviour>.Create(graph);
        playable.GetBehaviour().Opacity = Mathf.Clamp01(Opacity);
        return playable;
    }
}

public sealed class PlayerOpacityBehaviour : PlayableBehaviour
{
    public float Opacity;
}
