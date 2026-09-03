using UnityEngine;

/// <summary>
/// Builds an attack pose from attack data instead of authored notice colliders.
/// The pose is continuously retargeted while it is unlocked, then consumed by
/// the attack state as a root-motion warp destination.
/// </summary>
public sealed class EnemyAttackTargetingController
{
    private const float MinimumContactPadding = 0.05f;
    private const float MinimumExtent = 0.001f;

    private readonly EnemyCore _core;
    private readonly Collider[] _overlapResults = new Collider[16];
    private readonly RaycastHit[] _castResults = new RaycastHit[16];

    private EnemyAttackSO _attackData;
    private Vector3 _warpOrigin;
    private Quaternion _originRotation;
    private Vector3 _targetPosition;
    private Quaternion _targetRotation;
    private float _plannedWarpDistance;
    private bool _isActive;
    private bool _isTracking;

    public EnemyAttackSO AttackData => _attackData;
    public Vector3 TargetPosition => _targetPosition;
    public Quaternion TargetRotation => _targetRotation;
    public Vector3 TargetForward => _targetRotation * Vector3.forward;
    public bool IsActive => _isActive;
    public bool IsTracking => _isTracking;

    public EnemyAttackTargetingController(EnemyCore core)
    {
        _core = core;
    }

    public void Begin(EnemyAttackSO attackData)
    {
        if (attackData == null)
        {
            End();
            return;
        }

        _attackData = attackData;
        _warpOrigin = FlattenAtHeight(_core.transform.position, _core.transform.position.y);
        _originRotation = GetPlanarRotation(_core.Rotator.FacingDirection, _core.transform.rotation);
        _targetPosition = _warpOrigin;
        _targetRotation = _originRotation;
        _plannedWarpDistance = 0f;
        _isActive = true;
        _isTracking = true;

        UpdateTarget(0f);
    }

    public void UpdateTarget(float deltaTime)
    {
        if (!_isActive || !_isTracking || _attackData == null)
            return;

        Transform target = _core.TargetTransform;
        if (target == null)
            return;

        Vector3 toTarget = target.position - _warpOrigin;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude > MinimumExtent)
        {
            Quaternion desiredRotation = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
            float rotationSpeed = Mathf.Max(0f, _attackData.AttackRotationAnglePerSecond);
            float maximumAngle = Mathf.Min(
                180f,
                rotationSpeed * Mathf.Max(0f, _attackData.AttackAnimationTransitionTime));
            Quaternion boundedRotation = Quaternion.RotateTowards(
                _originRotation,
                desiredRotation,
                maximumAngle);

            if (deltaTime > 0f)
            {
                _targetRotation = Quaternion.RotateTowards(
                    _targetRotation,
                    boundedRotation,
                    rotationSpeed * deltaTime);
            }
        }

        Vector3 forward = _targetRotation * Vector3.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 primaryOffset = GetPrimaryOffset(_attackData);
        Vector3 hitboxCenterAtOrigin = _warpOrigin + _targetRotation * primaryOffset;
        Vector3 centerToTarget = target.position - hitboxCenterAtOrigin;
        centerToTarget.y = 0f;

        float desiredForwardTravel = Mathf.Max(0f, Vector3.Dot(centerToTarget, forward));
        float maximumWarpDistance = Mathf.Max(0f, _attackData.AttackWarpDistance);
        float candidateDistance = Mathf.Min(desiredForwardTravel, maximumWarpDistance);
        float contactDistance = FindFirstTargetContactDistance(
            target,
            forward,
            candidateDistance,
            _targetRotation);

        _plannedWarpDistance = Mathf.Min(candidateDistance, contactDistance);

        // Retargeting must never pull the attacker backwards after the lunge has
        // already covered part of the path. This is especially important for the
        // second combo hit, whose target remains live while its animation plays.
        Vector3 travelledFromOrigin = _core.transform.position - _warpOrigin;
        travelledFromOrigin.y = 0f;
        float reachedDistance = Mathf.Max(0f, Vector3.Dot(travelledFromOrigin, forward));
        _plannedWarpDistance = Mathf.Max(
            _plannedWarpDistance,
            Mathf.Min(maximumWarpDistance, reachedDistance));

        _targetPosition = _warpOrigin + forward * _plannedWarpDistance;
    }

    public void Lock()
    {
        _isTracking = false;
    }

    public void End()
    {
        _attackData = null;
        _isActive = false;
        _isTracking = false;
        _plannedWarpDistance = 0f;
    }

    /// <summary>
    /// Preserves the animation's root-motion curve while scaling its planar
    /// displacement to the currently planned travel distance. Movement is clamped
    /// at the target, so a close target can never be crossed.
    /// </summary>
    public Vector3 WarpRootMotion(Vector3 animatorDelta, Vector3 queuedMotion)
    {
        if (!_isActive || _attackData == null)
            return animatorDelta;

        Vector3 predictedPosition = _core.transform.position + queuedMotion;
        Vector3 remaining = _targetPosition - predictedPosition;
        remaining.y = 0f;
        float remainingDistance = remaining.magnitude;
        if (remainingDistance <= MinimumExtent || _plannedWarpDistance <= MinimumExtent)
            return Vector3.zero;

        Vector3 planarDelta = animatorDelta;
        planarDelta.y = 0f;
        // The animation delta can still point along the previous facing direction
        // while the attack is turning. Its planar magnitude is the authored motion;
        // redirect that magnitude toward the warp target instead of losing distance
        // through a forward dot product.
        float authoredMotionDelta = planarDelta.magnitude;
        // AttackWarpDistance is a maximum range, not a fixed root-motion
        // multiplier. Scaling by the actual planned distance preserves the
        // authored acceleration profile and prevents close targets from being
        // reached at the very beginning of the lunge.
        float rootMotionScale = _plannedWarpDistance;
        float warpedDistance = Mathf.Min(
            remainingDistance,
            authoredMotionDelta * rootMotionScale);

        return remaining / remainingDistance * warpedDistance;
    }

    public void DrawGizmos(Color damageColor, Color noticeColor, Color pathColor)
    {
        if (!_isActive || _attackData == null || _attackData.HitboxInfo == null)
            return;

        Matrix4x4 previousMatrix = Gizmos.matrix;
        Color previousColor = Gizmos.color;

        Gizmos.color = pathColor;
        Gizmos.DrawLine(_warpOrigin, _targetPosition);
        Gizmos.DrawWireSphere(_warpOrigin, 0.08f);
        Gizmos.DrawWireSphere(_targetPosition, 0.12f);

        foreach (EnemyAttackHitboxInfo hitbox in _attackData.HitboxInfo)
        {
            Vector3 center = _targetPosition + _targetRotation * hitbox.Offset;
            Gizmos.matrix = Matrix4x4.TRS(center, _targetRotation, Vector3.one);

            if (hitbox.HitboxType == EnemyAttackHitboxType.Sphere)
            {
                Gizmos.color = damageColor;
                Gizmos.DrawWireSphere(Vector3.zero, GetSphereRadius(hitbox, false));
                Gizmos.color = noticeColor;
                Gizmos.DrawWireSphere(Vector3.zero, GetSphereRadius(hitbox, true));
            }
            else
            {
                Gizmos.color = damageColor;
                Gizmos.DrawWireCube(Vector3.zero, GetBoxHalfExtents(hitbox, false) * 2f);
                Gizmos.color = noticeColor;
                Gizmos.DrawWireCube(Vector3.zero, GetBoxHalfExtents(hitbox, true) * 2f);
            }
        }

        Gizmos.matrix = previousMatrix;
        Gizmos.color = previousColor;
    }

    public bool IsPlayerInNoticeRange()
    {
        if (!_isActive || _attackData == null || _attackData.HitboxInfo == null)
            return false;

        Transform target = _core.TargetTransform;
        if (target == null)
            return false;

        foreach (EnemyAttackHitboxInfo hitbox in _attackData.HitboxInfo)
        {
            Vector3 center = _targetPosition + _targetRotation * hitbox.Offset;

            if (hitbox.HitboxType == EnemyAttackHitboxType.Sphere)
            {
                float radius = GetSphereRadius(hitbox, true);
                int count = Physics.OverlapSphereNonAlloc(
                    center,
                    radius,
                    _overlapResults,
                    _attackData.DamagedLayer,
                    QueryTriggerInteraction.Collide);
                if (ContainsTarget(count, target))
                    return true;
            }
            else
            {
                Vector3 halfExtents = GetBoxHalfExtents(hitbox, true);
                int count = Physics.OverlapBoxNonAlloc(
                    center,
                    halfExtents,
                    _overlapResults,
                    _targetRotation,
                    _attackData.DamagedLayer,
                    QueryTriggerInteraction.Collide);
                if (ContainsTarget(count, target))
                    return true;
            }
        }

        return false;
    }

    private float FindFirstTargetContactDistance(
        Transform target,
        Vector3 direction,
        float maximumDistance,
        Quaternion rotation)
    {
        if (_attackData.HitboxInfo == null || _attackData.HitboxInfo.Length == 0)
            return maximumDistance;

        float closestDistance = maximumDistance;

        foreach (EnemyAttackHitboxInfo hitbox in _attackData.HitboxInfo)
        {
            Vector3 center = _warpOrigin + rotation * hitbox.Offset;

            if (IsTargetOverlapping(hitbox, center, rotation, target))
                return 0f;

            if (maximumDistance <= MinimumExtent)
                continue;

            int count;
            if (hitbox.HitboxType == EnemyAttackHitboxType.Sphere)
            {
                count = Physics.SphereCastNonAlloc(
                    center,
                    GetSphereRadius(hitbox, false),
                    direction,
                    _castResults,
                    maximumDistance,
                    _attackData.DamagedLayer,
                    QueryTriggerInteraction.Collide);
            }
            else
            {
                count = Physics.BoxCastNonAlloc(
                    center,
                    GetBoxHalfExtents(hitbox, false),
                    direction,
                    _castResults,
                    rotation,
                    maximumDistance,
                    _attackData.DamagedLayer,
                    QueryTriggerInteraction.Collide);
            }

            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = _castResults[i];
                if (!IsTargetCollider(hit.collider, target))
                    continue;

                closestDistance = Mathf.Min(
                    closestDistance,
                    Mathf.Min(
                        maximumDistance,
                        hit.distance + GetTargetContactAdvance(hit.collider, direction)));
            }
        }

        return closestDistance;
    }

    private bool IsTargetOverlapping(
        EnemyAttackHitboxInfo hitbox,
        Vector3 center,
        Quaternion rotation,
        Transform target)
    {
        int count;
        if (hitbox.HitboxType == EnemyAttackHitboxType.Sphere)
        {
            count = Physics.OverlapSphereNonAlloc(
                center,
                GetSphereRadius(hitbox, false),
                _overlapResults,
                _attackData.DamagedLayer,
                QueryTriggerInteraction.Collide);
        }
        else
        {
            count = Physics.OverlapBoxNonAlloc(
                center,
                GetBoxHalfExtents(hitbox, false),
                _overlapResults,
                rotation,
                _attackData.DamagedLayer,
                QueryTriggerInteraction.Collide);
        }

        return ContainsTarget(count, target);
    }

    private bool ContainsTarget(int count, Transform target)
    {
        for (int i = 0; i < count; i++)
        {
            if (IsTargetCollider(_overlapResults[i], target))
                return true;
        }

        return false;
    }

    private static bool IsTargetCollider(Collider collider, Transform target)
    {
        if (collider == null || target == null)
            return false;

        Transform hitTransform = collider.transform;
        return hitTransform == target ||
               hitTransform.IsChildOf(target) ||
               target.IsChildOf(hitTransform);
    }

    private static float GetTargetContactAdvance(Collider targetCollider, Vector3 direction)
    {
        if (targetCollider == null)
            return MinimumContactPadding;

        // A cast reports the instant both surfaces touch. Advancing by the target's
        // projected half thickness places its centre just inside the damage field,
        // leaving enough overlap for a stable hit without moving through the player.
        Vector3 extents = targetCollider.bounds.extents;
        Vector3 absoluteDirection = Abs(direction.normalized);
        float projectedHalfThickness = Vector3.Dot(extents, absoluteDirection);
        return Mathf.Max(MinimumContactPadding, projectedHalfThickness + MinimumContactPadding);
    }

    private static Vector3 GetPrimaryOffset(EnemyAttackSO attackData)
    {
        return attackData.HitboxInfo != null && attackData.HitboxInfo.Length > 0
            ? attackData.HitboxInfo[0].Offset
            : Vector3.zero;
    }

    private static Vector3 GetBoxHalfExtents(EnemyAttackHitboxInfo hitbox, bool includeNoticeRange)
    {
        Vector3 size = Abs(hitbox.Size);
        if (includeNoticeRange)
            size += Abs(hitbox.AdditiveNoticeRange);

        return new Vector3(
            Mathf.Max(MinimumExtent, size.x * 0.5f),
            Mathf.Max(MinimumExtent, size.y * 0.5f),
            Mathf.Max(MinimumExtent, size.z * 0.5f));
    }

    private static float GetSphereRadius(EnemyAttackHitboxInfo hitbox, bool includeNoticeRange)
    {
        Vector3 size = Abs(hitbox.Size);
        float radius = Mathf.Max(size.x, size.y, size.z) * 0.5f;
        if (radius <= MinimumExtent)
            radius = Mathf.Max(0f, hitbox.Radius.x);

        if (includeNoticeRange)
        {
            Vector3 additional = Abs(hitbox.AdditiveNoticeRange);
            radius += Mathf.Max(additional.x, additional.y, additional.z) * 0.5f;
        }

        return Mathf.Max(MinimumExtent, radius);
    }

    private static Quaternion GetPlanarRotation(Vector3 direction, Quaternion fallback)
    {
        direction.y = 0f;
        return direction.sqrMagnitude > MinimumExtent
            ? Quaternion.LookRotation(direction.normalized, Vector3.up)
            : fallback;
    }

    private static Vector3 FlattenAtHeight(Vector3 position, float height)
    {
        position.y = height;
        return position;
    }

    private static Vector3 Abs(Vector3 value)
    {
        return new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
    }
}
