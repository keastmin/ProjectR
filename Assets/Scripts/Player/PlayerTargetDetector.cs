using UnityEngine;

public class PlayerTargetDetector : MonoBehaviour
{
    [SerializeField] private LayerMask _targetingLayer;
    [SerializeField] private float _detectRange = 10f;
    [SerializeField] private bool _alwaysDetect = false;
    [SerializeField] private bool _isDebugRange = true;

    private Collider[] _targetEnemyColliders;
    private Collider _nearestEnemyCollider;

    public Collider NearestEnemyCollider
    {
        get
        {
            if (_alwaysDetect)
                return _nearestEnemyCollider;
            return DetectTargets();
        }
    }
    public Vector3 NearestEnemyPosition
    {
        get
        {
            return NearestEnemyCollider != null ? NearestEnemyCollider.transform.position : transform.position;
        }
    }
    public Vector3 NearestEnemyDirection
    {
        get
        {
            return NearestEnemyCollider != null ? (NearestEnemyPosition - transform.position).normalized : Vector3.zero;
        }
    }

    private void Awake()
    {
        _targetEnemyColliders = new Collider[100];
        _nearestEnemyCollider = null;
    }

    private void Update()
    {
        if (_alwaysDetect)
            _nearestEnemyCollider = DetectTargets();
    }

    private Collider DetectTargets()
    {
        // 초기화
        for(int i = 0; i < _targetEnemyColliders.Length; i++)
        {
            _targetEnemyColliders[i] = null;
        }

        // 감지
        int detectedCount = Physics.OverlapSphereNonAlloc(transform.position, _detectRange, _targetEnemyColliders, _targetingLayer);

        // 가장 가까운 콜라이더 검사
        float minDist = float.MaxValue;
        Collider nearestTarget = null;
        for(int i = 0; i < detectedCount; i++)
        {
            Vector3 dir = _targetEnemyColliders[i].transform.position - transform.position;
            float sqr = dir.sqrMagnitude;
            if(minDist > sqr)
            {
                minDist = sqr;
                nearestTarget = _targetEnemyColliders[i];
            }
        }

        return nearestTarget;
    }

    private void OnDrawGizmosSelected()
    {
        if (_isDebugRange)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _detectRange);
        }
    }
}