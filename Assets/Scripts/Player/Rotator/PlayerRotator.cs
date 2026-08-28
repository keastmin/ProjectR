using UnityEngine;

public class PlayerRotator : MonoBehaviour
{
    [SerializeField] private Transform _modelTransform;
    [SerializeField] private float _rotationLerpSpeed = 10f;
    [SerializeField] private float _rotationTowardSpeed = 360f;

    private Quaternion _facingRotation = Quaternion.identity;

    public Quaternion FacingRotation => _facingRotation;
    public Vector3 FacingDirection => FacingRotation * Vector3.forward;

    private void Awake()
    {
        Quaternion originModelRotation = _modelTransform.rotation;
        _facingRotation = _modelTransform.rotation;
        transform.rotation = Quaternion.identity;
        _modelTransform.rotation = originModelRotation;
    }

    private void Update()
    {
        _modelTransform.rotation = _facingRotation;
    }

    public void RotateLerp(Vector3 direction)
    {
        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        _facingRotation = Quaternion.Slerp(_facingRotation, targetRotation, _rotationLerpSpeed * Time.deltaTime);
    }

    public void RotateToward(Vector3 direction)
    {
        RotateToward(direction, _rotationTowardSpeed);
    }

    public void RotateToward(Vector3 direction, float rotationTowardSpeed)
    {
        if (direction.sqrMagnitude <= 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        _facingRotation = Quaternion.RotateTowards(_facingRotation, targetRotation, rotationTowardSpeed * Time.deltaTime);
    }

    public void RotateImmediately(Vector3 direction)
    {
        if (direction.sqrMagnitude <= 0.001f)
            return;

        _facingRotation = Quaternion.LookRotation(direction, Vector3.up);
        _modelTransform.rotation = _facingRotation;
    }
}