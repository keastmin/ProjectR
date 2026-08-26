using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackInstanceContainer : MonoBehaviour
{
    [SerializeField] private AttackDamageField _basicAttack1DamageField;
    [SerializeField] private AttackDamageField _basicAttack2DamageField;
    [SerializeField] private AttackDamageField _basicAttack3DamageField;
    [SerializeField] private AttackDamageField _basicAttack4DamageField;
    [SerializeField] private AttackDamageField _runAttack1HitDamageField;
    [SerializeField] private AttackDamageField _runAttack2HitDamageField;

    [SerializeField] private Transform _basicAttack2GroundCrackTransform;
    [SerializeField] private ParticleSystem _basicAttack2GroundCrackParticle;

    private readonly HashSet<IDamageable> _damagedTargets = new();

    // 기본 공격 1 데미지 주기
    public void OnGiveDamageBasicAttack1()
    {
        GiveDamageFieldHashing(_basicAttack1DamageField);
    }

    // 기본 공격 2 데미지 주기
    public void OnGiveDamageBasicAttack2()
    {
        GiveDamageFieldHashing(_basicAttack2DamageField);
    }

    // 기본 공격 2 바닥 크랙 파티클 생성
    public void OnGroundCrackParticleBasicAttack2()
    {
        Vector3 pos = _basicAttack2GroundCrackTransform.position;
        Quaternion rot = _basicAttack2GroundCrackTransform.rotation;
        Instantiate(_basicAttack2GroundCrackParticle, pos, rot);
        Debug.Log("호출");
    }

    // 기본 공격 3 해시하고 데미지 주기
    public void OnGiveDamageBasicAttack3Hashing()
    {
        GiveDamageFieldHashing(_basicAttack3DamageField);
    }

    // 기본 공격 3 해시하지 않고 데미지 주기
    public void OnGiveDamageBasicAttack3NoHasing()
    {
        GiveDamageFieldNoHasing(_basicAttack3DamageField);
    }

    // 기본 공격 4 해시하고 데미지 주기
    public void OnGiveDamageBasicAttack4Hashing()
    {
        GiveDamageFieldHashing(_basicAttack4DamageField);
    }

    // 기본 공격 4 해시하지 않고 데미지 주기
    public void OnGiveDamageBasicAttack4NoHashing()
    {
        GiveDamageFieldNoHasing(_basicAttack4DamageField);
    }

    // 달리기 공격 1타 데미지 주기
    public void OnGiveDamageRunAttack1HitNoHashing()
    {
        GiveDamageFieldNoHasing(_runAttack1HitDamageField);
    }

    // 달리기 공격 2타 데미지 주기
    public void OnGiveDamageRunAttack2HitNoHashing()
    {
        GiveDamageFieldNoHasing(_runAttack2HitDamageField);
    }

    // 데미지를 주는 대상을 해쉬에 추가하면서 공격
    public void GiveDamageFieldHashing(AttackDamageField damageField)
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
            {
                DamageData data = new DamageData(this.gameObject, damageField.Damage, damageField.HitStopFrame);
                damageable.TakeDamage(data);
            }
        }
    }

    // 데미지를 주는 대상을 해쉬에 추가하지 않으면서 공격
    public void GiveDamageFieldNoHasing(AttackDamageField damageField)
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
            if (damageable != null)
            {
                DamageData data = new DamageData(this.gameObject, damageField.Damage, damageField.HitStopFrame);
                damageable.TakeDamage(data);
            }
        }
    }

    // 데미지 입은 대상 해쉬 정리
    public void ClearDamagedTargets()
    {
        _damagedTargets.Clear();
    }
}
