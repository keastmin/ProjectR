using UnityEngine;

public class EnemyBoxHitbox : EnemyHitbox
{
    private BoxCollider _collider;

    private void Awake()
    {
        _collider = GetComponent<BoxCollider>();
        if (_collider != null)
            _collider.enabled = false;
    }

    public override void Configure(EnemyAttackHitboxInfo hitboxInfo)
    {
        if (_collider == null)
            return;

        // HitboxInfo is expressed in the Hitboxies local space. Keep this pooled
        // object's transform untouched so it continues to follow its current parent.
        _collider.center = hitboxInfo.Center;
        _collider.size = hitboxInfo.Size;
    }

    public override Collider[] DetectTargets(LayerMask targetLayers)
    {
        if (_collider == null)
            return System.Array.Empty<Collider>();

        Vector3 halfExtents = Vector3.Scale(_collider.size, Abs(transform.lossyScale)) * 0.5f;
        return Physics.OverlapBox(
            transform.TransformPoint(_collider.center),
            halfExtents,
            transform.rotation,
            targetLayers,
            QueryTriggerInteraction.Collide);
    }

}
