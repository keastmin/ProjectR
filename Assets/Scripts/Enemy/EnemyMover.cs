using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyMover : MonoBehaviour
{
    private Rigidbody _rigidbody;

    private Vector3 _inputVelocity = Vector3.zero;

    private void Awake()
    {
        TryGetComponent(out _rigidbody);
        _rigidbody.freezeRotation = true;
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    private void FixedUpdate()
    {
        _rigidbody.linearVelocity = _inputVelocity;
        _inputVelocity = Vector3.zero;
    }

    public void Move(Vector3 velocity)
    {
        _inputVelocity = velocity;
    }
}