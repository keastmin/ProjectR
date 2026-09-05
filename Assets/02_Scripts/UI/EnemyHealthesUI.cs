using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealthesUI : MonoBehaviour
{
    [SerializeField] private EnemyCore[] _enemies;
    [SerializeField] private Vector2 _offset = new Vector2(50f, 50f);
    [SerializeField] private PlayerCore _player;
    [SerializeField] private RectTransform _canvasRectTransform;
    [SerializeField] private Camera _mainCam;
    [SerializeField] private EnemyHealthBar _enemyHealthBarPrefab;
    [SerializeField, Min(0f)] private float _disableDistance = 5f; // 이 거리보다 멀면 숨김 대기 시작
    [SerializeField, Min(0f)] private float _disableTime = 3f; // 멀어진 뒤 숨기기 시작할 때까지의 시간
    [SerializeField, Range(0f, 1f)] private float _normalVisibleAlpha = 0.5f; // 피격되지 않은 체력바의 투명도
    [SerializeField, Min(0f)] private float _disableFadeInDuration = 0.2f; // 투명해지는 시간
    [SerializeField, Min(0f)] private float _disableFadeOutDuration = 0.2f; // 다시 보이는 시간
    [SerializeField, Min(0f)] private float _damagedVisibleDuration = 3f; // 플레이어에게 피격된 뒤 표시를 유지하는 시간
    [SerializeField, Min(0f)] private float _deadFadeOutDuration = 0.5f;

    private sealed class HealthBarState
    {
        public EnemyHealthBar HealthBar;
        public Action<DamageData> DamagedHandler;
        public Action DeadHandler;
        public float OutOfRangeTime;
        public float DamagedVisibleTime;
        public bool IsDead;
    }

    private Dictionary<EnemyCore, HealthBarState> _enemyHealthes;
    private readonly List<EnemyCore> _healthBarsToRemove = new();

    private void Awake()
    {
        _enemyHealthes = new Dictionary<EnemyCore, HealthBarState>();
        foreach (var enemy in _enemies)
        {
            if (enemy == null || _enemyHealthes.ContainsKey(enemy))
                continue;

            EnemyHealthBar healthBar = Instantiate(_enemyHealthBarPrefab, transform);
            healthBar.InitializeHealthBar(_mainCam, _canvasRectTransform, enemy, _offset);
            healthBar.SetAlpha(_normalVisibleAlpha, 0f);

            var state = new HealthBarState
            {
                HealthBar = healthBar
            };
            state.DamagedHandler = damageData => HandleEnemyDamaged(state, damageData);
            state.DeadHandler = () => HandleEnemyDead(state);
            enemy.OnDamaged += state.DamagedHandler;
            enemy.OnDead += state.DeadHandler;
            _enemyHealthes.Add(enemy, state);
        }
    }

    private void Update()
    {
        if (_player == null)
            return;

        float deltaTime = Time.unscaledDeltaTime;
        float disableDistanceSqr = _disableDistance * _disableDistance;
        Vector3 playerPosition = _player.transform.position;

        foreach (KeyValuePair<EnemyCore, HealthBarState> pair in _enemyHealthes)
        {
            EnemyCore enemy = pair.Key;
            HealthBarState state = pair.Value;
            if (enemy == null || state.HealthBar == null)
                continue;

            if (state.IsDead)
            {
                if (state.HealthBar.Alpha <= 0f)
                    _healthBarsToRemove.Add(enemy);

                continue;
            }

            state.DamagedVisibleTime = Mathf.Max(0f, state.DamagedVisibleTime - deltaTime);

            bool isInRange = (enemy.transform.position - playerPosition).sqrMagnitude <= disableDistanceSqr;
            if (isInRange)
                state.OutOfRangeTime = 0f;
            else
                state.OutOfRangeTime += deltaTime;

            bool wasDamagedRecently = state.DamagedVisibleTime > 0f;
            bool shouldBeVisible = isInRange ||
                                   wasDamagedRecently ||
                                   state.OutOfRangeTime < _disableTime;

            float targetAlpha = wasDamagedRecently
                ? 1f
                : shouldBeVisible ? _normalVisibleAlpha : 0f;
            float fadeDuration = targetAlpha > state.HealthBar.Alpha
                ? _disableFadeOutDuration
                : _disableFadeInDuration;
            state.HealthBar.SetAlpha(targetAlpha, fadeDuration);
        }

        RemoveFadedHealthBars();
    }

    private void OnDestroy()
    {
        if (_enemyHealthes == null)
            return;

        foreach (KeyValuePair<EnemyCore, HealthBarState> pair in _enemyHealthes)
        {
            if (pair.Key != null)
            {
                pair.Key.OnDamaged -= pair.Value.DamagedHandler;
                pair.Key.OnDead -= pair.Value.DeadHandler;
            }
        }
    }

    private void HandleEnemyDamaged(HealthBarState state, DamageData damageData)
    {
        if (!WasDamagedByPlayer(damageData))
            return;

        state.DamagedVisibleTime = _damagedVisibleDuration;
        state.HealthBar.SetAlpha(1f, 0f);
    }

    private void HandleEnemyDead(HealthBarState state)
    {
        if (state.IsDead || state.HealthBar == null)
            return;

        state.IsDead = true;
        state.HealthBar.SetAlpha(0f, _deadFadeOutDuration);
    }

    private void RemoveFadedHealthBars()
    {
        foreach (EnemyCore enemy in _healthBarsToRemove)
        {
            if (!_enemyHealthes.TryGetValue(enemy, out HealthBarState state))
                continue;

            enemy.OnDamaged -= state.DamagedHandler;
            enemy.OnDead -= state.DeadHandler;
            Destroy(state.HealthBar.gameObject);
            _enemyHealthes.Remove(enemy);
        }

        _healthBarsToRemove.Clear();
    }

    private bool WasDamagedByPlayer(DamageData damageData)
    {
        if (_player == null || damageData.Sender == null)
            return false;

        PlayerCore damageSourcePlayer = damageData.Sender.GetComponentInParent<PlayerCore>();
        return damageSourcePlayer == _player || damageData.Sender.transform == _player.transform;
    }

    private void OnValidate()
    {
        _disableDistance = Mathf.Max(0f, _disableDistance);
        _disableTime = Mathf.Max(0f, _disableTime);
        _normalVisibleAlpha = Mathf.Clamp01(_normalVisibleAlpha);
        _disableFadeInDuration = Mathf.Max(0f, _disableFadeInDuration);
        _disableFadeOutDuration = Mathf.Max(0f, _disableFadeOutDuration);
        _damagedVisibleDuration = Mathf.Max(0f, _damagedVisibleDuration);
        _deadFadeOutDuration = Mathf.Max(0f, _deadFadeOutDuration);
    }
}
