using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.35f, 0.75f, 0.95f)]
[TrackBindingType(typeof(Animator))]
[TrackClipType(typeof(PlayerOpacityClip))]
public sealed class PlayerOpacityTrack : TrackAsset
{
    [SerializeField, Tooltip("플레이어의 URP Lit 투명 머티리얼. 빌드에도 페이드용 셰이더 변형을 포함합니다.")]
    private Material _fadeMaterial;

    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        var mixer = ScriptPlayable<PlayerOpacityMixer>.Create(graph, inputCount);
        mixer.GetBehaviour().Initialize(go.GetComponent<PlayableDirector>(), _fadeMaterial);
        return mixer;
    }

    protected override void OnCreateClip(TimelineClip clip)
    {
        clip.displayName = "Dash Fade";
        clip.easeInDuration = 3d / 60d;
        clip.easeOutDuration = 4d / 60d;
    }

    public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
    {
        // Let Timeline restore material references when exiting editor preview.
        var animator = director.GetGenericBinding(this) as Animator;
        if (animator != null)
        {
            foreach (Renderer renderer in animator.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer is not SkinnedMeshRenderer && renderer is not MeshRenderer)
                    continue;
                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                    driver.AddFromName(renderer, $"m_Materials.Array.data[{i}]");
            }
        }
        base.GatherProperties(director, driver);
    }
}
