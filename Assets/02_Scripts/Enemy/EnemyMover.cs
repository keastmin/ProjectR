using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyMover : MonoBehaviour
{
    private Rigidbody _rigidbody;

    private Vector3 _inputVelocity = Vector3.zero;
    private bool _isHitStopped;
    private RigidbodyConstraints _constraintsBeforeHitStop;

    private void Awake()
    {
        TryGetComponent(out _rigidbody);
        _rigidbody.freezeRotation = true;
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    private void FixedUpdate()
    {
        if (_isHitStopped)
        {
            _rigidbody.linearVelocity = Vector3.zero;
            _inputVelocity = Vector3.zero;
            return;
        }

        _rigidbody.linearVelocity = _inputVelocity;
        _inputVelocity = Vector3.zero;
    }

    private void OnDisable()
    {
        SetHitStopped(false);
    }

    public void Move(Vector3 velocity)
    {
        if (_isHitStopped)
            return;

        _inputVelocity = velocity;
    }

    public void WarpTo(Vector3 worldPosition)
    {
        if (_isHitStopped)
            return;

        _inputVelocity = Vector3.zero;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.position = worldPosition;
        transform.position = worldPosition;
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
}
