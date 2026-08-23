using System;
using UnityEngine;

[Serializable]
public class AttackDamageField
{
    [SerializeField] private string _name = "Damage Field";
    [SerializeField] private Collider _hitbox;
    [SerializeField, Min(0f)] private float _damage = 10f;
    [SerializeField] private LayerMask _targetLayers = ~0;
    [SerializeField] private QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Collide;

    public string Name => _name;
    public float Damage => _damage;

    public void DisablePhysicalCollision()
    {
        if (_hitbox != null)
            _hitbox.enabled = false;
    }

    public Collider[] DetectTargets()
    {
        if (_hitbox == null)
        {
            Debug.LogWarning($"Damage Field '{_name}'에 Hitbox가 지정되지 않았습니다.");
            return Array.Empty<Collider>();
        }

        if (_hitbox is BoxCollider box)
            return DetectBox(box);

        if (_hitbox is SphereCollider sphere)
            return DetectSphere(sphere);

        if (_hitbox is CapsuleCollider capsule)
            return DetectCapsule(capsule);

        Debug.LogWarning($"Damage Field '{_name}'은 Box, Sphere, Capsule Collider만 지원합니다.");
        return Array.Empty<Collider>();
    }

    public void DrawGizmo(Color color)
    {
        if (_hitbox == null)
            return;

        Matrix4x4 previousMatrix = Gizmos.matrix;
        Color previousColor = Gizmos.color;
        Gizmos.color = color;

        if (_hitbox is BoxCollider box)
        {
            Gizmos.matrix = box.transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.center, box.size);
        }
        else if (_hitbox is SphereCollider sphere)
        {
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.DrawWireSphere(sphere.transform.TransformPoint(sphere.center), GetSphereRadius(sphere));
        }
        else if (_hitbox is CapsuleCollider capsule)
        {
            GetCapsuleWorldShape(capsule, out Vector3 pointA, out Vector3 pointB, out float radius);
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.DrawWireSphere(pointA, radius);
            Gizmos.DrawWireSphere(pointB, radius);
            Gizmos.DrawLine(pointA, pointB);
        }

        Gizmos.matrix = previousMatrix;
        Gizmos.color = previousColor;
    }

    private Collider[] DetectBox(BoxCollider box)
    {
        Vector3 halfExtents = Vector3.Scale(box.size, Abs(box.transform.lossyScale)) * 0.5f;

        return Physics.OverlapBox(
            box.transform.TransformPoint(box.center),
            halfExtents,
            box.transform.rotation,
            _targetLayers,
            _triggerInteraction);
    }

    private Collider[] DetectSphere(SphereCollider sphere)
    {
        return Physics.OverlapSphere(
            sphere.transform.TransformPoint(sphere.center),
            GetSphereRadius(sphere),
            _targetLayers,
            _triggerInteraction);
    }

    private Collider[] DetectCapsule(CapsuleCollider capsule)
    {
        GetCapsuleWorldShape(capsule, out Vector3 pointA, out Vector3 pointB, out float radius);
        return Physics.OverlapCapsule(pointA, pointB, radius, _targetLayers, _triggerInteraction);
    }

    private static float GetSphereRadius(SphereCollider sphere)
    {
        Vector3 scale = Abs(sphere.transform.lossyScale);
        return sphere.radius * Mathf.Max(scale.x, scale.y, scale.z);
    }

    private static void GetCapsuleWorldShape(
        CapsuleCollider capsule,
        out Vector3 pointA,
        out Vector3 pointB,
        out float radius)
    {
        Vector3 scale = Abs(capsule.transform.lossyScale);
        Vector3 localAxis;
        float axisScale;
        float radiusScale;

        switch (capsule.direction)
        {
            case 0:
                localAxis = Vector3.right;
                axisScale = scale.x;
                radiusScale = Mathf.Max(scale.y, scale.z);
                break;
            case 2:
                localAxis = Vector3.forward;
                axisScale = scale.z;
                radiusScale = Mathf.Max(scale.x, scale.y);
                break;
            default:
                localAxis = Vector3.up;
                axisScale = scale.y;
                radiusScale = Mathf.Max(scale.x, scale.z);
                break;
        }

        Vector3 center = capsule.transform.TransformPoint(capsule.center);
        Vector3 axis = capsule.transform.TransformDirection(localAxis).normalized;
        radius = capsule.radius * radiusScale;
        float halfLineLength = Mathf.Max(capsule.height * axisScale * 0.5f - radius, 0f);
        pointA = center + axis * halfLineLength;
        pointB = center - axis * halfLineLength;
    }

    private static Vector3 Abs(Vector3 value)
    {
        return new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
    }
}
