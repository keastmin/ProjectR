using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackInstanceContainer : MonoBehaviour
{
    [SerializeField] private List<AttackDamageField> _damageFields = new();

    private readonly HashSet<IDamageable> _damagedTargets = new();

    private void Awake()
    {
        DisablePhysicalCollisions();
    }

    // Animation Event에서 Damage Field 목록의 Element 인덱스를 int 파라미터로 전달한다.
    public void OnGiveDamageField(int damageFieldIndex)
    {
        if (_damageFields == null || damageFieldIndex < 0 || damageFieldIndex >= _damageFields.Count)
        {
            Debug.LogWarning($"Damage Field 인덱스 {damageFieldIndex}가 존재하지 않습니다.", this);
            return;
        }

        AttackDamageField damageField = _damageFields[damageFieldIndex];
        if (damageField == null)
            return;

        _damagedTargets.Clear();
        Collider[] hits = damageField.DetectTargets();

        foreach (Collider hit in hits)
        {
            IDamageable damageable = hit.GetComponentInParent<IDamageable>();
            if (damageable != null && _damagedTargets.Add(damageable))
                damageable.TakeDamage(damageField.Damage);
        }
    }

    private void OnValidate()
    {
        DisablePhysicalCollisions();
    }

    private void OnDrawGizmosSelected()
    {
        if (_damageFields == null)
            return;

        for (int i = 0; i < _damageFields.Count; i++)
        {
            AttackDamageField damageField = _damageFields[i];
            if (damageField == null)
                continue;

            Color color = Color.HSVToRGB(i * 0.17f % 1f, 0.8f, 1f);
            damageField.DrawGizmo(color);
        }
    }

    private void DisablePhysicalCollisions()
    {
        if (_damageFields == null)
            return;

        foreach (AttackDamageField damageField in _damageFields)
            damageField?.DisablePhysicalCollision();
    }
}
