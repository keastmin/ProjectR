using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackInstanceContainer : MonoBehaviour
{
    [SerializeField] private AttackDamageField _basicAttack1DamageField;
    [SerializeField] private AttackDamageField _basicAttack2DamageField;

    private readonly HashSet<IDamageable> _damagedTargets = new();

    // 기본 공격 1 데미지 주기
    public void OnGiveDamageBasicAttack1()
    {
        GiveDamageField(_basicAttack1DamageField);
    }

    // 기본 공격 2 데미지 주기
    public void OnGiveDamageBasicAttack2()
    {
        GiveDamageField(_basicAttack2DamageField);
    }

    // 데미지를 주는 대상을 해쉬에 추가하면서 공격
    public void GiveDamageField(AttackDamageField damageField)
    {
        if (damageField == null)
        {
            Debug.Log("데미지 필드 존재하지 않음");
            return;
        }

        Collider[] hits = damageField.DetectTargets();

        foreach (Collider hit in hits)
        {
            IDamageable damageable = hit.GetComponentInParent<IDamageable>();
            if (damageable != null && _damagedTargets.Add(damageable))
                damageable.TakeDamage(damageField.Damage);
        }
    }

    // 데미지 입은 대상 해쉬 정리
    public void ClearDamagedTargets()
    {
        _damagedTargets.Clear();
    }
}
