using System;
using UnityEngine;

public enum AttackHitStopMode
{
    AttackerAndVictims = 0,
    VictimsOnly = 1
}

[Serializable]
public class AttackDamageField
{
    [SerializeField] private string _name = "Damage Field";
    [SerializeField] private Collider _hitbox;
    [SerializeField, Min(0f)] private float _damage = 10f;
    [SerializeField, Min(0), Tooltip("60Hz 전투 프레임 기준 플레이어 히트스탑 길이입니다. 적은 자동으로 1프레임 더 정지합니다.")]
    private int _hitStopFrame = 0;
    [SerializeField, Tooltip("Attacker And Victims는 플레이어와 적, Victims Only는 적만 정지합니다. 0프레임이면 히트스톱을 생략합니다.")]
    private AttackHitStopMode _hitStopMode;
    [SerializeField, Tooltip("적의 최소 요구 레벨 이상일 때만 피격 상태에 진입합니다. None도 피해와 히트스탑은 그대로 적용됩니다.")]
    private StaggerLevel _staggerLevel = StaggerLevel.None;
    [SerializeField] private LayerMask _targetLayers = ~0;
    [SerializeField] private QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Collide;
    [SerializeField] private float _skillGaugeAdditive;

    public string Name => _name;
    public float Damage => _damage;
    public int HitStopFrame => _hitStopFrame;
    public AttackHitStopMode HitStopMode => _hitStopMode;
    public StaggerLevel StaggerLevel => _staggerLevel;
    public float SkillGaugeAdditive => _skillGaugeAdditive;

    public Collider Hitbox => _hitbox;

    public void AssignHitbox(string name, Collider hitbox)
    {
        _name = name;
        _hitbox = hitbox;
    }

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
