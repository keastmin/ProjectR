using UnityEngine;

public class EnemyTargetDetector : MonoBehaviour
{
    [SerializeField] private LayerMask _targetLayer;
    [SerializeField] private float _detectRange; // 플레이어 감지 거리
    [SerializeField] private float _lostRange; // 플레이어 이탈 거리
    [SerializeField] private bool _isDebugRange = true;

    private Collider[] _playerDetectColliders;
    private Collider _targetCollider;

    public Transform TargetTransform => _targetCollider != null ? _targetCollider.transform : null;

    private void Awake()
    {
        _playerDetectColliders = new Collider[1];
    }

    private void Update()
    {
        if (_targetCollider == null)
            DetectPlayer();
        else
            LostPlayer();
    }

    private void DetectPlayer()
    {
        int detectCount = Physics.OverlapSphereNonAlloc(transform.position, _detectRange, _playerDetectColliders, _targetLayer);

        if (detectCount > 0)
        {
            _targetCollider = _playerDetectColliders[0];
        }
    }

    private void LostPlayer()
    {
        Vector3 targetPos = _targetCollider.transform.position;
        float dist = Vector3.Distance(transform.position, targetPos);

        if (dist > _lostRange)
            _targetCollider = null;
    }

    private void OnDrawGizmosSelected()
    {
        if (_isDebugRange)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _detectRange);
            Gizmos.color = Color.purple;
            Gizmos.DrawWireSphere(transform.position, _lostRange);
        }
    }
}