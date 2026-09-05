using System;
using UnityEngine;
using UnityEngine.Playables;

// Temporary, unsaved lifecycle hook owned by the track mixer.
[ExecuteAlways, AddComponentMenu("")]
public sealed class PlayerOpacityLifetime : MonoBehaviour
{
    [NonSerialized] public PlayableDirector Director;
    [NonSerialized] public Action Release;

    private void OnDisable() => Release?.Invoke();

    private void LateUpdate()
    {
        if (Director == null || !Director.isActiveAndEnabled)
            Release?.Invoke();
    }
}
