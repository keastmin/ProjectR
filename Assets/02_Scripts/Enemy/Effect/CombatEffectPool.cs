using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

[DefaultExecutionOrder(-1000)]
[DisallowMultipleComponent]
public sealed class CombatEffectPool : MonoBehaviour
{
    private const float DefaultPlaybackDuration = 5f;

    [SerializeField] private CombatEffectPoolInfo[] _effectPoolInfo;
    [SerializeField] private Transform _container;

    private readonly Dictionary<CombatEffectID, EffectPool> _effectPools = new();
    private readonly List<PooledEffect> _activeEffects = new();
    private bool _isInitialized;

    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        Initialize();
        CombatEffectRequestBus.Requested += HandleRequest;
    }

    private void OnDisable()
    {
        CombatEffectRequestBus.Requested -= HandleRequest;

        for (int i = _activeEffects.Count - 1; i >= 0; i--)
            ReturnToPool(_activeEffects[i]);
    }

    private void LateUpdate()
    {
        float now = Time.time;
        for (int i = _activeEffects.Count - 1; i >= 0; i--)
        {
            PooledEffect effect = _activeEffects[i];
            effect.UpdateFollowTransform();

            if (now >= effect.ReturnTime)
                ReturnToPool(effect);
        }
    }

    private void Initialize()
    {
        if (_isInitialized)
            return;

        _isInitialized = true;

        if (_container == null)
            _container = transform;

        if (_effectPoolInfo == null)
            return;

        foreach (CombatEffectPoolInfo info in _effectPoolInfo)
        {
            if (info.Prefab == null)
            {
                Debug.LogWarning($"{info.ID} 이펙트 프리팹이 지정되지 않았습니다.", this);
                continue;
            }

            if (_effectPools.ContainsKey(info.ID))
            {
                Debug.LogWarning($"중복된 이펙트 타입은 등록할 수 없습니다: {info.ID}", this);
                continue;
            }

            int maxSize = Mathf.Max(1, info.MaxSize, info.PrewarmCount);
            EffectPool pool = new EffectPool(info, maxSize);
            _effectPools.Add(info.ID, pool);

            for (int i = 0; i < info.PrewarmCount; i++)
            {
                PooledEffect effect = CreateEffect(pool);
                if (effect == null)
                    break;

                pool.Available.Push(effect);
            }
        }
    }

    private void HandleRequest(CombatEffectRequest request)
    {
        if (!_effectPools.TryGetValue(request.EffectType, out EffectPool pool))
        {
            Debug.LogWarning($"등록되지 않은 이펙트가 요청되었습니다: {request.EffectType}", this);
            return;
        }

        PooledEffect effect = Acquire(pool);
        if (effect == null)
            return;

        effect.Play(
            request.Position,
            request.Rotation,
            request.FollowTarget,
            GetPlaybackDuration(pool.Info));
        pool.Active.Add(effect);
        _activeEffects.Add(effect);
    }

    private PooledEffect Acquire(EffectPool pool)
    {
        if (pool.Available.Count > 0)
            return pool.Available.Pop();

        if (pool.TotalCount < pool.MaxSize)
            return CreateEffect(pool);

        // 풀의 상한에 도달하면 요청을 버리지 않고 가장 오래 재생된 인스턴스를 재사용합니다.
        PooledEffect oldest = null;
        for (int i = 0; i < pool.Active.Count; i++)
        {
            PooledEffect candidate = pool.Active[i];
            if (oldest == null || candidate.StartTime < oldest.StartTime)
                oldest = candidate;
        }

        if (oldest == null)
            return null;

        ReturnToPool(oldest);
        return pool.Available.Pop();
    }

    private PooledEffect CreateEffect(EffectPool pool)
    {
        GameObject instance = Instantiate(pool.Info.Prefab, _container);
        instance.name = $"{pool.Info.Prefab.name} (Pooled)";

        PooledEffect effect = new PooledEffect(instance, pool);
        if (!effect.HasPlayableComponent)
        {
            Debug.LogWarning(
                $"{pool.Info.ID} 프리팹에 ParticleSystem 또는 VisualEffect가 없습니다: {pool.Info.Prefab.name}",
                this);
        }

        effect.StopAndHide();
        pool.TotalCount++;
        return effect;
    }

    private void ReturnToPool(PooledEffect effect)
    {
        effect.StopAndHide();
        effect.Owner.Active.Remove(effect);
        _activeEffects.Remove(effect);
        effect.Owner.Available.Push(effect);
    }

    private static float GetPlaybackDuration(CombatEffectPoolInfo info)
    {
        return info.PlaybackDuration > 0f ? info.PlaybackDuration : DefaultPlaybackDuration;
    }

    private sealed class EffectPool
    {
        public readonly CombatEffectPoolInfo Info;
        public readonly int MaxSize;
        public readonly Stack<PooledEffect> Available = new();
        public readonly List<PooledEffect> Active = new();
        public int TotalCount;

        public EffectPool(CombatEffectPoolInfo info, int maxSize)
        {
            Info = info;
            MaxSize = maxSize;
        }
    }

    private sealed class PooledEffect
    {
        private readonly GameObject _gameObject;
        private readonly Transform _transform;
        private readonly ParticleSystem[] _particleSystems;
        private readonly VisualEffect[] _visualEffects;
        private Transform _followTarget;

        public readonly EffectPool Owner;
        public float StartTime { get; private set; }
        public float ReturnTime { get; private set; }
        public bool HasPlayableComponent => _particleSystems.Length > 0 || _visualEffects.Length > 0;

        public PooledEffect(GameObject gameObject, EffectPool owner)
        {
            _gameObject = gameObject;
            _transform = gameObject.transform;
            Owner = owner;
            _particleSystems = gameObject.GetComponentsInChildren<ParticleSystem>(true);
            _visualEffects = gameObject.GetComponentsInChildren<VisualEffect>(true);
        }

        public void Play(
            Vector3 position,
            Quaternion rotation,
            Transform followTarget,
            float duration)
        {
            _transform.SetPositionAndRotation(position, rotation);
            _followTarget = followTarget;
            _gameObject.SetActive(true);

            // 이전 재생 상태를 완전히 제거한 뒤 새 요청으로 시작합니다.
            foreach (ParticleSystem particleSystem in _particleSystems)
            {
                particleSystem.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                particleSystem.Play(false);
            }

            foreach (VisualEffect visualEffect in _visualEffects)
            {
                visualEffect.Stop();
                visualEffect.Reinit();
                visualEffect.Play();
            }

            StartTime = Time.time;
            ReturnTime = StartTime + duration;
        }

        public void UpdateFollowTransform()
        {
            if (_followTarget == null)
                return;

            _transform.SetPositionAndRotation(_followTarget.position, _followTarget.rotation);
        }

        public void StopAndHide()
        {
            _followTarget = null;

            foreach (ParticleSystem particleSystem in _particleSystems)
                particleSystem.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);

            foreach (VisualEffect visualEffect in _visualEffects)
                visualEffect.Stop();

            _gameObject.SetActive(false);
        }
    }
}
