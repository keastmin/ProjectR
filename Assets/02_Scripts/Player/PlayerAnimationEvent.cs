using System;
using UnityEngine;

public class PlayerAnimationEvent : MonoBehaviour
{
    [SerializeField] private PlayerAnimationEventSender _sender;
    [SerializeField] private Transform _leftFootDashTransform;
    [SerializeField] private Transform _rightFootDashTransform;
    [SerializeField] private ParticleSystem _dashExplosionParticle;
    [SerializeField] private Transform _dashWindTransform;
    [SerializeField] private ParticleSystem _dashWindParicle;
    [SerializeField] private LayerMask _groundLayer;

    public event Action OnEnableNextBasicAttack;
    public event Action OnDisableNextBasicAttack;
    public event Action OnHighSpeedRotationEnd; // 빠른 속도로 회전 종료
    public event Action OnEnableOtherBehaviour; // 다른 행동 가능
    public event Action OnAnimationEnd; // 공용 다음 상태로 이어서 가는 이벤트

    private void Awake()
    {

    }

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
        Instantiate(_dashExplosionParticle, leftPos, Quaternion.identity);
    }

    public void OnDashExplosionEffectRight()
    {
        Vector3 rightPos = _rightFootDashTransform.position;
        if (Physics.Raycast(rightPos, Vector3.down, out RaycastHit hit, 10f, _groundLayer))
            rightPos = hit.point;
        Instantiate(_dashExplosionParticle, rightPos, Quaternion.identity);
    }

    public void OnDashWindEffect()
    {
        Vector3 pos = _dashWindTransform.position;
        Quaternion rot = _dashWindTransform.rotation;
        Instantiate(_dashWindParicle, pos, rot);
    }
}