using System.Collections.Generic;
using UnityEngine;

public enum EnemyPositioningAction
{
    Hold,
    Advance,
    Retreat,
    StrafeLeft,
    StrafeRight
}

/// <summary>
/// Coordinates enemies around a shared target without assigning hard formation slots.
/// Each enemy receives a drifting comfort radius and orbit preference, while separation
/// and attack-lane priorities temporarily override those preferences when necessary.
/// </summary>
[DefaultExecutionOrder(-90)]
public class EnemyPositioningController : MonoBehaviour
{
    [Header("Participants")]
    [SerializeField] private List<EnemyCore> _enemies = new();
    [SerializeField] private bool _autoDiscoverEnemies = true;

    [Header("Soft Combat Envelope")]
    [SerializeField, Min(0f)] private float _minimumCombatRadius = 2.1f;
    [SerializeField, Min(0f)] private float _maximumCombatRadius = 4.4f;
    [SerializeField, Min(0f)] private float _radialDeadZone = 0.65f;
    [SerializeField, Range(0f, 2f)] private float _orbitInfluence = 0.72f;
    [SerializeField] private Vector2 _preferenceChangeInterval = new(2.4f, 5.5f);
    [SerializeField] private Vector2 _actionHoldInterval = new(0.35f, 0.9f);

    [Header("Neighbour Avoidance")]
    [SerializeField, Min(0.1f)] private float _personalSpaceRadius = 1.35f;
    [SerializeField, Range(0f, 4f)] private float _separationInfluence = 2.2f;
    [SerializeField, Min(0f)] private float _motionLookAhead = 1.25f;
    [SerializeField, Range(0f, 1f)] private float _motionSteeringStrength = 0.72f;

    [Header("Attack Lane")]
    [SerializeField, Min(0f)] private float _minimumAttackDistance = 1.15f;
    [SerializeField, Min(0f)] private float _maximumAttackDistance = 4.25f;
    [SerializeField, Min(0.1f)] private float _attackLaneHalfWidth = 0.9f;
    [SerializeField, Min(0f)] private float _attackLaneEndPadding = 0.55f;
    [SerializeField, Min(0f)] private float _yieldDuration = 0.9f;
    [SerializeField, Min(0f)] private float _laneRequestMemory = 1.15f;

    [Header("Debug")]
    [SerializeField] private bool _drawDebugGizmos;

    private sealed class AgentState
    {
        public float ComfortRadius;
        public float TargetComfortRadius;
        public float OrbitSign;
        public float NextPreferenceTime;
        public float LastPreferenceSampleTime;
        public float NextActionTime;
        public float YieldUntil;
        public Vector3 YieldDirection;
        public EnemyPositioningAction Action;
        public Vector3 SmoothedAvoidanceVelocity;
    }

    private static readonly List<EnemyPositioningController> ActiveControllers = new();

    private readonly Dictionary<EnemyCore, AgentState> _agentStates = new();
    private readonly List<EnemyCore> _cleanupBuffer = new();

    private EnemyCore _committedAttacker;
    private EnemyCore _laneRequester;
    private float _laneRequestUntil;
    private float _combatClock;

    public EnemyCore CommittedAttacker => _committedAttacker;

    private void OnEnable()
    {
        if (!ActiveControllers.Contains(this))
            ActiveControllers.Add(this);

        RegisterConfiguredEnemies();

        if (_autoDiscoverEnemies)
            RegisterDiscoveredEnemies();
    }

    private void Update()
    {
        _combatClock += CombatTimeController.DeltaTime;
        CleanupInvalidEnemies();

        if (_committedAttacker != null && !IsAvailable(_committedAttacker))
            NotifyAttackEnded(_committedAttacker);

        if (_laneRequester != null &&
            (_combatClock >= _laneRequestUntil || !IsAvailable(_laneRequester)))
        {
            _laneRequester = null;
        }
    }

    private void OnDisable()
    {
        ActiveControllers.Remove(this);

        foreach (EnemyCore enemy in _agentStates.Keys)
        {
            if (enemy != null)
                enemy.ClearPositioningController(this);
        }

        _agentStates.Clear();
        _committedAttacker = null;
        _laneRequester = null;
    }

    public static EnemyPositioningController FindFor(EnemyCore enemy)
    {
        if (enemy == null)
            return null;

        EnemyPositioningController closest = null;
        float closestSqrDistance = float.PositiveInfinity;

        for (int i = ActiveControllers.Count - 1; i >= 0; i--)
        {
            EnemyPositioningController controller = ActiveControllers[i];
            if (controller == null || !controller.isActiveAndEnabled)
            {
                ActiveControllers.RemoveAt(i);
                continue;
            }

            float sqrDistance = (controller.transform.position - enemy.transform.position).sqrMagnitude;
            if (sqrDistance >= closestSqrDistance)
                continue;

            closest = controller;
            closestSqrDistance = sqrDistance;
        }

        return closest;
    }

    public void Register(EnemyCore enemy)
    {
        if (enemy == null)
            return;

        if (!_enemies.Contains(enemy))
            _enemies.Add(enemy);

        if (!_agentStates.ContainsKey(enemy))
            _agentStates.Add(enemy, CreateAgentState(enemy));

        enemy.SetPositioningController(this);
    }

    public void Unregister(EnemyCore enemy)
    {
        if (enemy == null)
            return;

        if (_committedAttacker == enemy)
            _committedAttacker = null;

        if (_laneRequester == enemy)
            _laneRequester = null;

        _agentStates.Remove(enemy);
        enemy.ClearPositioningController(this);
    }

    public EnemyPositioningAction GetRecommendedAction(EnemyCore enemy)
    {
        if (!TryGetActiveState(enemy, out AgentState state) || enemy.TargetTransform == null)
            return EnemyPositioningAction.Hold;

        if (_combatClock < state.NextActionTime)
            return state.Action;

        RefreshPreferences(enemy, state);

        Vector3 steering = CalculateSteering(enemy, state);
        state.Action = ResolveAction(enemy, steering, state.Action);
        state.NextActionTime = _combatClock + RandomInRange(_actionHoldInterval);
        return state.Action;
    }

    public bool CanBeginAttack(EnemyCore enemy)
    {
        if (!TryGetActiveState(enemy, out _) || enemy.TargetTransform == null)
            return false;

        if (_committedAttacker != null)
            return _committedAttacker == enemy;

        Vector3 toTarget = Flatten(enemy.TargetTransform.position - enemy.transform.position);
        float distance = toTarget.magnitude;
        if (distance < _minimumAttackDistance || distance > _maximumAttackDistance)
        {
            TryClaimLaneRequest(enemy);
            return false;
        }

        EnemyCore blocker = FindFirstLaneBlocker(enemy, enemy.TargetTransform.position);
        if (blocker == null)
            return true;

        if (TryClaimLaneRequest(enemy))
            AskEnemyToYield(blocker, enemy, enemy.TargetTransform.position);

        return false;
    }

    public void NotifyAttackStarted(EnemyCore enemy)
    {
        if (!TryGetActiveState(enemy, out _))
            return;

        _committedAttacker = enemy;
        _laneRequester = null;
        AskLaneBlockersToYield(enemy, enemy.TargetTransform != null
            ? enemy.TargetTransform.position
            : enemy.transform.position + enemy.Rotator.FacingDirection * _maximumAttackDistance);
    }

    public void NotifyAttackEnded(EnemyCore enemy)
    {
        if (_committedAttacker == enemy)
            _committedAttacker = null;
    }

    public Vector3 AdjustMovement(EnemyCore enemy, Vector3 desiredVelocity)
    {
        if (!TryGetActiveState(enemy, out AgentState state))
            return desiredVelocity;

        Vector3 flatVelocity = Flatten(desiredVelocity);
        float speed = flatVelocity.magnitude;
        if (speed <= 0.01f)
        {
            state.SmoothedAvoidanceVelocity = Vector3.zero;
            return desiredVelocity;
        }

        Vector3 moveDirection = flatVelocity / speed;
        Vector3 avoidance = Vector3.zero;
        float brake = 0f;

        foreach (KeyValuePair<EnemyCore, AgentState> pair in _agentStates)
        {
            EnemyCore other = pair.Key;
            if (other == enemy || !IsAvailable(other))
                continue;

            Vector3 toOther = Flatten(other.transform.position - enemy.transform.position);
            float forwardDistance = Vector3.Dot(toOther, moveDirection);
            if (forwardDistance <= 0f || forwardDistance > _motionLookAhead + _personalSpaceRadius)
                continue;

            Vector3 lateralOffset = toOther - moveDirection * forwardDistance;
            float lateralDistance = lateralOffset.magnitude;
            if (lateralDistance >= _personalSpaceRadius)
                continue;

            float urgency = (1f - lateralDistance / _personalSpaceRadius) *
                            (1f - Mathf.Clamp01(forwardDistance / (_motionLookAhead + _personalSpaceRadius)));

            bool enemyHasPriority = enemy == _committedAttacker;
            bool otherHasPriority = other == _committedAttacker;
            float priorityMultiplier = enemyHasPriority ? 0.35f : otherHasPriority ? 1.35f : 1f;

            Vector3 side = lateralDistance > 0.05f
                ? -lateralOffset.normalized
                : GetPassingSide(moveDirection);

            avoidance += side * urgency * speed * priorityMultiplier;
            brake = Mathf.Max(brake, urgency * (enemyHasPriority ? 0.2f : 0.65f));
        }

        Vector3 targetAvoidance = Vector3.ClampMagnitude(avoidance, speed);
        state.SmoothedAvoidanceVelocity = Vector3.MoveTowards(
            state.SmoothedAvoidanceVelocity,
            targetAvoidance,
            speed * 8f * Time.fixedDeltaTime);

        Vector3 steeredVelocity = flatVelocity * (1f - brake) + state.SmoothedAvoidanceVelocity;
        steeredVelocity = Vector3.ClampMagnitude(steeredVelocity, speed);

        Vector3 result = Vector3.Lerp(flatVelocity, steeredVelocity, _motionSteeringStrength);
        result.y = desiredVelocity.y;
        return result;
    }

    private Vector3 CalculateSteering(EnemyCore enemy, AgentState state)
    {
        Vector3 enemyPosition = enemy.transform.position;
        Vector3 targetPosition = enemy.TargetTransform.position;
        Vector3 toTarget = Flatten(targetPosition - enemyPosition);
        float distance = toTarget.magnitude;
        if (distance <= 0.001f)
            return Vector3.zero;

        Vector3 forward = toTarget / distance;
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        float radialError = distance - state.ComfortRadius;
        Vector3 steering = Vector3.zero;

        if (Mathf.Abs(radialError) > _radialDeadZone)
            steering += forward * Mathf.Sign(radialError) * Mathf.Min(Mathf.Abs(radialError), 2f);

        float orbitPulse = 0.65f + 0.35f * Mathf.Sin(_combatClock * 1.7f + StableId(enemy) * 0.013f);
        steering += right * state.OrbitSign * _orbitInfluence * orbitPulse;

        foreach (EnemyCore other in _agentStates.Keys)
        {
            if (other == enemy || !IsAvailable(other))
                continue;

            Vector3 away = Flatten(enemyPosition - other.transform.position);
            float neighbourDistance = away.magnitude;
            if (neighbourDistance <= 0.001f || neighbourDistance >= _personalSpaceRadius)
                continue;

            float closeness = 1f - neighbourDistance / _personalSpaceRadius;
            steering += away / neighbourDistance * closeness * _separationInfluence;
        }

        EnemyCore priorityEnemy = _committedAttacker != null ? _committedAttacker : _laneRequester;
        if (priorityEnemy != null && priorityEnemy != enemy &&
            IsBlockingLane(enemy, priorityEnemy, targetPosition))
        {
            AskEnemyToYield(enemy, priorityEnemy, targetPosition);
        }

        if (_laneRequester == enemy)
        {
            if (distance > _maximumAttackDistance - 0.15f)
                steering += forward * 2.35f;
            else if (distance < _minimumAttackDistance + 0.1f)
                steering -= forward * 1.8f;

            EnemyCore blocker = FindFirstLaneBlocker(enemy, targetPosition);
            if (blocker != null)
            {
                Vector3 toBlocker = Flatten(blocker.transform.position - enemyPosition);
                float sideSign = Mathf.Sign(Vector3.Dot(toBlocker, right));
                if (Mathf.Approximately(sideSign, 0f))
                    sideSign = state.OrbitSign;

                steering += right * -sideSign * 1.45f;
            }
        }

        if (_combatClock < state.YieldUntil)
        {
            Vector3 awayFromTarget = -forward;
            steering += state.YieldDirection * 2.6f + awayFromTarget * 0.55f;
        }

        return steering;
    }

    private bool TryClaimLaneRequest(EnemyCore enemy)
    {
        if (_laneRequester != null && _laneRequester != enemy && _combatClock < _laneRequestUntil)
            return false;

        _laneRequester = enemy;
        _laneRequestUntil = _combatClock + _laneRequestMemory;
        return true;
    }

    private EnemyPositioningAction ResolveAction(
        EnemyCore enemy,
        Vector3 steering,
        EnemyPositioningAction previousAction)
    {
        if (steering.sqrMagnitude < 0.16f)
            return EnemyPositioningAction.Hold;

        Vector3 toTarget = Flatten(enemy.TargetTransform.position - enemy.transform.position).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, toTarget);
        float forwardAmount = Vector3.Dot(steering, toTarget);
        float sideAmount = Vector3.Dot(steering, right);

        const float actionHysteresis = 1.18f;
        bool wasRadial = previousAction == EnemyPositioningAction.Advance ||
                         previousAction == EnemyPositioningAction.Retreat;

        if (Mathf.Abs(forwardAmount) > Mathf.Abs(sideAmount) * (wasRadial ? 1f / actionHysteresis : actionHysteresis))
            return forwardAmount >= 0f ? EnemyPositioningAction.Advance : EnemyPositioningAction.Retreat;

        return sideAmount >= 0f ? EnemyPositioningAction.StrafeRight : EnemyPositioningAction.StrafeLeft;
    }

    private EnemyCore FindFirstLaneBlocker(EnemyCore attacker, Vector3 targetPosition)
    {
        EnemyCore nearestBlocker = null;
        float nearestProgress = float.PositiveInfinity;

        foreach (EnemyCore other in _agentStates.Keys)
        {
            if (other == attacker || !IsAvailable(other))
                continue;

            if (!TryGetLaneProgress(other.transform.position, attacker.transform.position, targetPosition,
                    out float progress, out float lateralDistance))
            {
                continue;
            }

            if (lateralDistance > _attackLaneHalfWidth || progress >= nearestProgress)
                continue;

            nearestBlocker = other;
            nearestProgress = progress;
        }

        return nearestBlocker;
    }

    private bool IsBlockingLane(EnemyCore possibleBlocker, EnemyCore attacker, Vector3 targetPosition)
    {
        return TryGetLaneProgress(
                   possibleBlocker.transform.position,
                   attacker.transform.position,
                   targetPosition,
                   out _,
                   out float lateralDistance) &&
               lateralDistance <= _attackLaneHalfWidth;
    }

    private bool TryGetLaneProgress(
        Vector3 point,
        Vector3 laneStart,
        Vector3 laneEnd,
        out float progress,
        out float lateralDistance)
    {
        Vector3 lane = Flatten(laneEnd - laneStart);
        float laneLength = lane.magnitude;
        if (laneLength <= 0.001f)
        {
            progress = 0f;
            lateralDistance = float.PositiveInfinity;
            return false;
        }

        Vector3 laneDirection = lane / laneLength;
        Vector3 offset = Flatten(point - laneStart);
        progress = Vector3.Dot(offset, laneDirection);
        lateralDistance = (offset - laneDirection * progress).magnitude;
        return progress > 0.1f && progress < laneLength - _attackLaneEndPadding;
    }

    private void AskLaneBlockersToYield(EnemyCore attacker, Vector3 targetPosition)
    {
        foreach (EnemyCore other in _agentStates.Keys)
        {
            if (other != attacker && IsAvailable(other) && IsBlockingLane(other, attacker, targetPosition))
                AskEnemyToYield(other, attacker, targetPosition);
        }
    }

    private void AskEnemyToYield(EnemyCore blocker, EnemyCore attacker, Vector3 targetPosition)
    {
        if (!_agentStates.TryGetValue(blocker, out AgentState state))
            return;

        Vector3 laneDirection = Flatten(targetPosition - attacker.transform.position).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, laneDirection);
        float side = Vector3.Dot(Flatten(blocker.transform.position - attacker.transform.position), right);

        if (Mathf.Abs(side) <= 0.08f)
            side = state.OrbitSign;

        state.YieldDirection = right * Mathf.Sign(side);
        state.YieldUntil = Mathf.Max(state.YieldUntil, _combatClock + _yieldDuration);
        state.NextActionTime = 0f;
    }

    private void RefreshPreferences(EnemyCore enemy, AgentState state)
    {
        float sampleDelta = Mathf.Max(0f, _combatClock - state.LastPreferenceSampleTime);
        state.ComfortRadius = Mathf.MoveTowards(
            state.ComfortRadius,
            state.TargetComfortRadius,
            sampleDelta * 0.45f);
        state.LastPreferenceSampleTime = _combatClock;

        if (_combatClock < state.NextPreferenceTime)
            return;

        float normalizedIdNoise = Mathf.Repeat(StableId(enemy) * 0.6180339f + _combatClock * 0.071f, 1f);
        state.TargetComfortRadius = Mathf.Lerp(_minimumCombatRadius, _maximumCombatRadius, normalizedIdNoise);

        if (Random.value < 0.3f)
            state.OrbitSign *= -1f;

        state.NextPreferenceTime = _combatClock + RandomInRange(_preferenceChangeInterval);
    }

    private AgentState CreateAgentState(EnemyCore enemy)
    {
        int stableId = StableId(enemy);
        float seed = Mathf.Repeat(stableId * 0.6180339f, 1f);
        float comfortRadius = Mathf.Lerp(_minimumCombatRadius, _maximumCombatRadius, seed);
        return new AgentState
        {
            ComfortRadius = comfortRadius,
            TargetComfortRadius = comfortRadius,
            OrbitSign = (stableId & 1) == 0 ? 1f : -1f,
            NextPreferenceTime = _combatClock + RandomInRange(_preferenceChangeInterval),
            LastPreferenceSampleTime = _combatClock,
            NextActionTime = 0f,
            Action = EnemyPositioningAction.Hold
        };
    }

    private bool TryGetActiveState(EnemyCore enemy, out AgentState state)
    {
        if (enemy == null)
        {
            state = null;
            return false;
        }

        if (!_agentStates.TryGetValue(enemy, out state))
        {
            Register(enemy);
            _agentStates.TryGetValue(enemy, out state);
        }

        return state != null && IsAvailable(enemy);
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

    private void RegisterDiscoveredEnemies()
    {
        EnemyCore[] sceneEnemies = FindObjectsByType<EnemyCore>();
        foreach (EnemyCore enemy in sceneEnemies)
        {
            if (FindFor(enemy) == this)
                Register(enemy);
        }
    }

    private void CleanupInvalidEnemies()
    {
        _cleanupBuffer.Clear();
        foreach (EnemyCore enemy in _agentStates.Keys)
        {
            if (enemy == null || !enemy.isActiveAndEnabled || enemy.CurrentHP <= 0f)
                _cleanupBuffer.Add(enemy);
        }

        foreach (EnemyCore enemy in _cleanupBuffer)
        {
            _agentStates.Remove(enemy);
            if (enemy != null)
                Unregister(enemy);
        }
    }

    private static bool IsAvailable(EnemyCore enemy)
    {
        return enemy != null && enemy.isActiveAndEnabled && enemy.CurrentHP > 0f;
    }

    private static Vector3 GetPassingSide(Vector3 forward)
    {
        return Vector3.Cross(Vector3.up, forward);
    }

    private static int StableId(EnemyCore enemy)
    {
        return enemy.GetEntityId().GetHashCode();
    }

    private static Vector3 Flatten(Vector3 value)
    {
        value.y = 0f;
        return value;
    }

    private static float RandomInRange(Vector2 range)
    {
        return Random.Range(range.x, range.y);
    }

    private void OnValidate()
    {
        _minimumCombatRadius = Mathf.Max(0f, _minimumCombatRadius);
        _maximumCombatRadius = Mathf.Max(_minimumCombatRadius, _maximumCombatRadius);
        _radialDeadZone = Mathf.Max(0f, _radialDeadZone);
        _personalSpaceRadius = Mathf.Max(0.1f, _personalSpaceRadius);
        _motionLookAhead = Mathf.Max(0f, _motionLookAhead);
        _minimumAttackDistance = Mathf.Max(0f, _minimumAttackDistance);
        _maximumAttackDistance = Mathf.Max(_minimumAttackDistance, _maximumAttackDistance);
        _attackLaneHalfWidth = Mathf.Max(0.1f, _attackLaneHalfWidth);
        ClampRange(ref _preferenceChangeInterval);
        ClampRange(ref _actionHoldInterval);
    }

    private static void ClampRange(ref Vector2 range)
    {
        range.x = Mathf.Max(0.05f, range.x);
        range.y = Mathf.Max(range.x, range.y);
    }

    private void OnDrawGizmosSelected()
    {
        if (!_drawDebugGizmos)
            return;

        foreach (EnemyCore enemy in _enemies)
        {
            if (enemy == null || enemy.TargetTransform == null)
                continue;

            Gizmos.color = enemy == _committedAttacker ? Color.red : Color.cyan;
            Gizmos.DrawLine(enemy.transform.position + Vector3.up * 0.1f,
                enemy.TargetTransform.position + Vector3.up * 0.1f);

            if (_agentStates.TryGetValue(enemy, out AgentState state) && _combatClock < state.YieldUntil)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(enemy.transform.position + Vector3.up * 0.2f, state.YieldDirection * 1.5f);
            }
        }
    }
}
