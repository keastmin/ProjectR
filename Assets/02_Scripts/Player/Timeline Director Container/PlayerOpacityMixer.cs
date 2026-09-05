using UnityEngine;
using UnityEngine.Playables;

public sealed class PlayerOpacityMixer : PlayableBehaviour
{
    private PlayableDirector _director;
    private Material _fadeMaterial;
    private Animator _binding;
    private PlayerOpacityMaterials _materials;
    private PlayerOpacityLifetime _lifetime;

    public void Initialize(PlayableDirector director, Material fadeMaterial)
    {
        _director = director;
        _fadeMaterial = fadeMaterial;
        if (_director != null)
        {
            _director.stopped += OnDirectorStopped;
            // A paused/manual graph does not receive OnGraphStop again when its
            // owner is disabled. Observe the GameObject lifetime independently.
            _lifetime = _director.gameObject.AddComponent<PlayerOpacityLifetime>();
            _lifetime.hideFlags = HideFlags.HideAndDontSave;
            _lifetime.Director = _director;
            _lifetime.Release = ReleaseMaterials;
        }
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        var animator = playerData as Animator;
        if (_binding != animator)
        {
            ReleaseMaterials();
            _binding = animator;
        }
        if (_binding == null)
            return;

        // Unoccupied track time is fully opaque. Timeline's native blend weights
        // provide the fade envelope, including user-edited blend curves.
        float transparency = 0f;
        float totalWeight = 0f;
        for (int i = 0; i < playable.GetInputCount(); i++)
        {
            float weight = playable.GetInputWeight(i);
            if (weight <= 0f)
                continue;
            var input = (ScriptPlayable<PlayerOpacityBehaviour>)playable.GetInput(i);
            transparency += (1f - input.GetBehaviour().Opacity) * weight;
            totalWeight += weight;
        }
        float opacity = Mathf.Clamp01(1f - transparency / Mathf.Max(1f, totalWeight));
        if (opacity < 1f)
            _materials ??= new PlayerOpacityMaterials(_binding, _fadeMaterial);
        _materials?.SetOpacity(opacity);
    }

    private void OnDirectorStopped(PlayableDirector director) => ReleaseMaterials();

    public override void OnGraphStop(Playable playable)
    {
        // Pause/scrub holds the current opacity. Disabling the owner restores it.
        if (_director == null || !_director.isActiveAndEnabled)
            ReleaseMaterials();
    }

    public override void OnPlayableDestroy(Playable playable)
    {
        if (_director != null)
            _director.stopped -= OnDirectorStopped;
        ReleaseMaterials();
        if (_lifetime != null)
        {
            _lifetime.Release = null;
            if (Application.isPlaying)
                Object.Destroy(_lifetime);
            else
                Object.DestroyImmediate(_lifetime);
        }
    }

    private void ReleaseMaterials()
    {
        _materials?.Dispose();
        _materials = null;
    }
}
