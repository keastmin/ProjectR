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

        transform.localPosition = hitboxInfo.Offset;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Abs(hitboxInfo.Size);
        _collider.center = Vector3.zero;
        _collider.size = Vector3.one;
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
