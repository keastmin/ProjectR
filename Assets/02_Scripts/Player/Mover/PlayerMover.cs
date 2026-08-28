using TMPro;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerMover : MonoBehaviour
{
    [SerializeField] private float _height = 2f;
    [SerializeField] private float _thickness = 1f;
    [SerializeField] private Vector3 _offset = Vector3.zero;
    [SerializeField] private float _stepHeight = 0.3f;

    private Rigidbody _rigidbody;
    private CapsuleCollider _capsuleCollider;

    private Vector3 _inputVelocity = Vector3.zero;
    private bool _isHitStopped;
    private RigidbodyConstraints _constraintsBeforeHitStop;

    #region MonoBehaviour

    private void OnValidate()
    {
        InitComponents();
        SetColliderDimention();
    }

    private void Awake()
    {
        OnValidate();
    }

    private void FixedUpdate()
    {
        if (_isHitStopped)
        {
            _rigidbody.linearVelocity = Vector3.zero;
            _inputVelocity = Vector3.zero;
            return;
        }

        ApplyVelocity(_inputVelocity);
        UpdateCleanup();
    }

    private void OnDisable()
    {
        SetHitStopped(false);
    }

    #endregion

    #region API

    public void Move(Vector3 velocity)
    {
        if (_isHitStopped)
            return;

        _inputVelocity = velocity;
    }

    public void SetHitStopped(bool stopped)
    {
        if (_isHitStopped == stopped)
            return;

        _isHitStopped = stopped;
        _inputVelocity = Vector3.zero;
        _rigidbody.linearVelocity = Vector3.zero;

        if (stopped)
        {
            _constraintsBeforeHitStop = _rigidbody.constraints;
            _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }
        else
        {
            _rigidbody.constraints = _constraintsBeforeHitStop;
        }
    }

    #endregion

    #region Core

    private void InitComponents()
    {
        TryGetComponent(out _rigidbody);
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rigidbody.useGravity = false;
        _rigidbody.freezeRotation = true;

        TryGetComponent(out _capsuleCollider);
    }

    private void SetColliderDimention()
    {
        SetHeight();
        SetThickness();
    }

    private void SetHeight()
    {
        if (_stepHeight > _height) _stepHeight = _height;
        Vector3 center = _offset + new Vector3(0f, _height / 2f, 0f);
        center.y += _stepHeight / 2f;
        _capsuleCollider.center = center;
        _capsuleCollider.height = _height - _stepHeight;
        LimitRadius();
    }

    private void SetThickness()
    {
        float radius = _thickness / 2f;
        _capsuleCollider.radius = radius;
        LimitRadius();
    }

    private void LimitRadius()
    {
        if (_capsuleCollider.radius * 2f > _capsuleCollider.height) 
            _capsuleCollider.radius = _capsuleCollider.height / 2f;
    }

    private void ApplyVelocity(Vector3 velocity)
    {
        _rigidbody.linearVelocity = velocity;
    }

    private void UpdateCleanup()
    {
        _inputVelocity = Vector3.zero;
    }

    #endregion
}
