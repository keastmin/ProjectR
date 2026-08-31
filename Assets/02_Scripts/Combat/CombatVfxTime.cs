using UnityEngine;
using UnityEngine.VFX;

// 전투 VFX의 각 재생 컴포넌트에 등록합니다. Canvas/UI에는 자동 등록하지 않습니다.
[DisallowMultipleComponent]
public sealed class CombatVfxTime : MonoBehaviour
{
    private ParticleSystem _particle;
    private VisualEffect _visualEffect;
    private Tiny.Trail _trail;
    private float _particleBaseSpeed;
    private float _vfxBaseSpeed;
    private bool _timelineControlsParticle;
    private bool _initialized;

    public static void RegisterHierarchy(GameObject root, bool timelineControlsParticle = false)
    {
        foreach (ParticleSystem particle in root.GetComponentsInChildren<ParticleSystem>(true))
            Register(particle.gameObject, timelineControlsParticle);
        foreach (VisualEffect effect in root.GetComponentsInChildren<VisualEffect>(true))
            Register(effect.gameObject, timelineControlsParticle);
        foreach (Tiny.Trail trail in root.GetComponentsInChildren<Tiny.Trail>(true))
            Register(trail.gameObject, timelineControlsParticle);
    }

    private static void Register(GameObject target, bool timelineControlsParticle)
    {
        if (target.GetComponentInParent<Canvas>(true) != null)
            return;
        if (!target.TryGetComponent(out CombatVfxTime clock))
            clock = target.AddComponent<CombatVfxTime>();
        clock.Initialize();
        clock._timelineControlsParticle = timelineControlsParticle;
        clock.ApplyScale(CombatTimeController.Scale);
    }

    private void Awake() => Initialize();

    private void Initialize()
    {
        if (_initialized)
            return;
        _initialized = true;
        TryGetComponent(out _particle);
        TryGetComponent(out _visualEffect);
        TryGetComponent(out _trail);
        if (_particle != null)
            _particleBaseSpeed = _particle.main.simulationSpeed;
        if (_visualEffect != null)
            _vfxBaseSpeed = _visualEffect.playRate;
    }

    private void OnEnable()
    {
        Initialize();
        CombatTimeController.ScaleChanged += ApplyScale;
        ApplyScale(CombatTimeController.Scale);
    }

    private void OnDisable()
    {
        CombatTimeController.ScaleChanged -= ApplyScale;
        ApplyScale(1f);
    }

    public void ApplyScale(float scale)
    {
        if (_particle != null)
        {
            var main = _particle.main;
            main.simulationSpeed = _particleBaseSpeed * (_timelineControlsParticle ? 1f : scale);
        }
        if (_visualEffect != null)
            _visualEffect.playRate = _vfxBaseSpeed * scale;
        if (_trail != null)
            _trail.TimeScale = scale;
    }
}
