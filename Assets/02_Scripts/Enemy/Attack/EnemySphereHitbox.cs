using UnityEngine;

public class EnemySphereHitbox : EnemyHitbox
{
    private SphereCollider _collider;

    private void Awake()
    {
        _collider = GetComponent<SphereCollider>();
        if (_collider != null)
            _collider.enabled = false;
    }

    public override void Configure(EnemyAttackHitboxInfo hitboxInfo)
    {
        if (_collider == null)
            return;

        // Radius is stored as a Vector3 in the existing attack data. A sphere has
        // one radius, so its X value is the authored radius.
        _collider.center = hitboxInfo.Center;
        _collider.radius = Mathf.Max(0f, hitboxInfo.Radius.x);
    }

    public override Collider[] DetectTargets(LayerMask targetLayers)
    {
        if (_collider == null)
            return System.Array.Empty<Collider>();

        Vector3 scale = Abs(transform.lossyScale);
        float radius = _collider.radius * Mathf.Max(scale.x, scale.y, scale.z);
        return Physics.OverlapSphere(
            transform.TransformPoint(_collider.center),
            radius,
            targetLayers,
            QueryTriggerInteraction.Collide);
    }

}
