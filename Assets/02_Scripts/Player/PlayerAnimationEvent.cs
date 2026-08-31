using System;
using UnityEngine;

public class PlayerAnimationEvent : MonoBehaviour
{
    [SerializeField] private Transform _leftFootDashTransform;
    [SerializeField] private Transform _rightFootDashTransform;
    [SerializeField] private ParticleSystem _dashExplosionParticle;
    [SerializeField] private Transform _dashWindTransform;
    [SerializeField] private ParticleSystem _dashWindParicle;
    [SerializeField] private LayerMask _groundLayer;

    [Header("Dodge Attack")]
    [SerializeField] private ParticleSystem _dodgeAttackParticle;
    [SerializeField] private ParticleSystem _dodgeAttackProjectile;
    [SerializeField] private Transform _1hitTransform;
    [SerializeField] private Transform _2hitTransform;
    [SerializeField] private Transform _projectileTransform;

    public event Action OnEnableNextBasicAttack;
    public event Action OnDisableNextBasicAttack;
    public event Action OnHighSpeedRotationEnd; // 빠른 속도로 회전 종료
    public event Action OnEnableOtherBehaviour; // 다른 행동 가능
    public event Action OnAnimationEnd; // 공용 다음 상태로 이어서 가는 이벤트
    public event Action OnPerfectDodgeEnd; // 완벽 회피 종료 이벤트

    // 다음 기본 공격 가능 이벤트 발동
    public void OnEnableNextBasicAttackActionInvoke()
    {
        OnEnableNextBasicAttack?.Invoke();
    }

    // 다음 기본 공격 불가능 이벤트 발동
    public void OnDisableNextBasicAttackActionInvoke()
    {
        OnDisableNextBasicAttack?.Invoke();
    }

    // 빠른 회전 중단 이벤트 발동
    public void OnHighSpeedRotationEndActionvInvoke()
    {
        OnHighSpeedRotationEnd?.Invoke();
    }

    // 다른 행동 가능 이벤트 발동
    public void OnEnableOtherBehaviourActionInvoke()
    {
        OnEnableOtherBehaviour?.Invoke();
    }

    // 다음 상태로 넘어가는 이벤트 발동
    public void OnAnimationEndActionInvoke()
    {
        OnAnimationEnd?.Invoke();
    }

    public void OnDashExplosionEffectBoth()
    {
        OnDashExplosionEffectLeft();
        OnDashExplosionEffectRight();
    }

    public void OnDashExplosionEffectLeft()
    {
        Vector3 leftPos = _leftFootDashTransform.position;
        if (Physics.Raycast(leftPos, Vector3.down, out RaycastHit hit, 10f, _groundLayer))
            leftPos = hit.point;
        CombatVfxTime.RegisterHierarchy(Instantiate(_dashExplosionParticle, leftPos, Quaternion.identity).gameObject);
    }

    public void OnDashExplosionEffectRight()
    {
        Vector3 rightPos = _rightFootDashTransform.position;
        if (Physics.Raycast(rightPos, Vector3.down, out RaycastHit hit, 10f, _groundLayer))
            rightPos = hit.point;
        CombatVfxTime.RegisterHierarchy(Instantiate(_dashExplosionParticle, rightPos, Quaternion.identity).gameObject);
    }

    public void OnDashWindEffect()
    {
        Vector3 pos = _dashWindTransform.position;
        Quaternion rot = _dashWindTransform.rotation;
        CombatVfxTime.RegisterHierarchy(Instantiate(_dashWindParicle, pos, rot).gameObject);
    }

    // Perfect Dodge를 종료 시키는 함수
    public void OnPerfectDodgeEndInvoke()
    {
        OnPerfectDodgeEnd?.Invoke();
    }

    public void OnDodgeAttack1HitEffect()
    {
        CreateEffect(_1hitTransform, _dodgeAttackParticle);
    }

    public void OnDodgeAttack2HitEffect()
    {
        CreateEffect(_2hitTransform, _dodgeAttackParticle);
    }

    public void OnDodgeAttackProjectileEffect()
    {
        CreateEffect(_projectileTransform, _dodgeAttackProjectile);
    }

    private void CreateEffect(Transform createTransform, ParticleSystem particle)
    {
        Instantiate(particle, createTransform);
    }
}