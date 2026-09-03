using UnityEngine;

public class PlayerTargetDetector : MonoBehaviour
{
    [SerializeField] private LayerMask _targetingLayer;
    [SerializeField] private float _detectRange = 10f;
    [SerializeField] private bool _alwaysDetect = false;
    [SerializeField] private bool _isDebugRange = true;

    private Collider[] _targetEnemyColliders;
    private Collider _nearestEnemyCollider;
    private Collider _basicAttackTargetCollider;

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
    public Collider BasicAttackTargetCollider => IsTargetAvailable(_basicAttackTargetCollider)
        ? _basicAttackTargetCollider
        : null;

    private void Awake()
    {
        _targetEnemyColliders = new Collider[100];
        _nearestEnemyCollider = null;
        _basicAttackTargetCollider = null;
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
            if (!IsTargetAvailable(_targetEnemyColliders[i]))
                continue;

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

    public Collider AcquireBasicAttackTarget()
    {
        // 콤보가 이어지는 동안에는 더 가까운 적이 생겨도 기존 대상을 유지합니다.
        if (!IsTargetAvailable(_basicAttackTargetCollider))
            _basicAttackTargetCollider = DetectTargets();

        return BasicAttackTargetCollider;
    }

    public void ClearBasicAttackTarget()
    {
        _basicAttackTargetCollider = null;
    }

    private static bool IsTargetAvailable(Collider target)
    {
        if (target == null || !target.enabled || !target.gameObject.activeInHierarchy)
            return false;

        EnemyCore enemy = target.GetComponentInParent<EnemyCore>();
        return enemy == null || (enemy.isActiveAndEnabled && enemy.CurrentHP > 0f);
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
