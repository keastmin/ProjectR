using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class EnemyAttackTimingController : MonoBehaviour
{
    [Header("Participants")]
    [SerializeField] private List<EnemyCore> _enemies = new();

    [Header("Attack Rhythm")]
    [SerializeField] private Vector2 _initialAttackDelayRange = new(1.5f, 3.5f);
    [SerializeField] private Vector2 _repeatAttackDelayRange = new(3.5f, 6.5f);
    [SerializeField] private Vector2 _betweenAttackGapRange = new(0.35f, 1.1f);
    [SerializeField] private Vector2 _urgentAttackDelayRange = new(0f, 0.25f);

    private readonly Dictionary<EnemyCore, float> _nextAttackTimes = new();

    private EnemyCore _activeAttacker;
    private float _combatTime;
    private float _nextGlobalAttackTime;

    public EnemyCore ActiveAttacker => _activeAttacker;

    private void Awake()
    {
        RegisterConfiguredEnemies();
    }

    private void Update()
    {
        _combatTime += CombatTimeController.DeltaTime;

        if (_activeAttacker != null &&
            (!_activeAttacker.isActiveAndEnabled || _activeAttacker.CurrentHP <= 0f))
        {
            ReleaseAttack(_activeAttacker);
        }
    }

    private void OnDestroy()
    {
        foreach (EnemyCore enemy in _nextAttackTimes.Keys)
        {
            if (enemy != null)
                enemy.ClearAttackTimingController(this);
        }

        _nextAttackTimes.Clear();
        _activeAttacker = null;
    }

    public void Register(EnemyCore enemy)
    {
        if (enemy == null)
            return;

        if (!_enemies.Contains(enemy))
            _enemies.Add(enemy);

        if (!_nextAttackTimes.ContainsKey(enemy))
        {
            _nextAttackTimes.Add(
                enemy,
                _combatTime + RandomInRange(_initialAttackDelayRange));
        }

        enemy.SetAttackTimingController(this);
    }

    public bool TryBeginAttack(EnemyCore enemy)
    {
        if (!IsReadyToAttack(enemy))
            return false;

        _activeAttacker = enemy;
        return true;
    }

    public bool IsReadyToAttack(EnemyCore enemy)
    {
        if (enemy == null || !enemy.isActiveAndEnabled || enemy.CurrentHP <= 0f)
            return false;

        if (!_nextAttackTimes.ContainsKey(enemy))
            Register(enemy);

        return _activeAttacker == null &&
               _combatTime >= _nextGlobalAttackTime &&
               _combatTime >= _nextAttackTimes[enemy];
    }

    public void ReleaseAttack(EnemyCore enemy)
    {
        if (enemy == null || _activeAttacker != enemy)
            return;

        _activeAttacker = null;
        _nextGlobalAttackTime = _combatTime + RandomInRange(_betweenAttackGapRange);
        _nextAttackTimes[enemy] = _combatTime + RandomInRange(_repeatAttackDelayRange);
    }

    public void PrioritizeAttack(EnemyCore enemy)
    {
        if (enemy == null)
            return;

        if (!_nextAttackTimes.ContainsKey(enemy))
            Register(enemy);

        float urgentAttackTime = _combatTime + RandomInRange(_urgentAttackDelayRange);
        _nextAttackTimes[enemy] = Mathf.Min(_nextAttackTimes[enemy], urgentAttackTime);
    }

    private void RegisterConfiguredEnemies()
    {
        if (_enemies == null)
            _enemies = new List<EnemyCore>();

        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            EnemyCore enemy = _enemies[i];
            if (enemy == null)
            {
                _enemies.RemoveAt(i);
                continue;
            }

            Register(enemy);
        }
    }

    private void OnValidate()
    {
        ClampRange(ref _initialAttackDelayRange);
        ClampRange(ref _repeatAttackDelayRange);
        ClampRange(ref _betweenAttackGapRange);
        ClampRange(ref _urgentAttackDelayRange);
    }

    private static void ClampRange(ref Vector2 range)
    {
        range.x = Mathf.Max(0f, range.x);
        range.y = Mathf.Max(range.x, range.y);
    }

    private static float RandomInRange(Vector2 range)
    {
        return Random.Range(range.x, range.y);
    }
}
