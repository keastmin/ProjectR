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

        transform.localPosition = hitboxInfo.Offset;
        transform.localRotation = Quaternion.identity;

        Vector3 size = Abs(hitboxInfo.Size);
        if (size.sqrMagnitude > 0.000001f)
        {
            transform.localScale = size;
            _collider.radius = 0.5f;
        }
        else
        {
            transform.localScale = Vector3.one;
            _collider.radius = Mathf.Max(0f, hitboxInfo.Radius.x);
        }

        _collider.center = Vector3.zero;
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
