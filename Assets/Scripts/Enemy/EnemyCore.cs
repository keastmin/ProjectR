using UnityEngine;

public class EnemyCore : MonoBehaviour, IDamageable
{
    [SerializeField, Min(0f)] private float _maxHP = 100f;
    [SerializeField] private float _currentHP;

    public float CurrentHP => _currentHP;

    private Rigidbody _rigidbody;

    private void Awake()
    {
        _currentHP = _maxHP;
        TryGetComponent(out _rigidbody);
    }

    private void FixedUpdate()
    {
        _rigidbody.linearVelocity = Vector3.zero;
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f || _currentHP <= 0f)
            return;

        _currentHP = Mathf.Max(_currentHP - damage, 0f);

        Debug.Log("피해 입음");
    }
}
