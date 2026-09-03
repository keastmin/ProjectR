using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CameraBehindEnemyDetector : MonoBehaviour
{
    [SerializeField] private Camera _detectCamera;
    [SerializeField] private PlayerCore _player;
    [SerializeField] private float _ringHeight = 1f;
    [SerializeField] private float _ringRadius = 2f;
    [SerializeField] private EnemyCore[] _enemies;
    [SerializeField] private GameObject _normalMarkerPrefab;
    [SerializeField] private GameObject _warningMarkerPrefab;

    private GameObject[] _normalMarkers;
    private GameObject[] _warningMarkers;

    private bool _isDebugPlayerExist = false;

    private void OnValidate()
    {
        _isDebugPlayerExist = _player != null;
    }

    private void Awake()
    {
        CreateMarkers();
    }

    private void Update()
    {
        UpdateMarkers();
    }

    private void CreateMarkers()
    {
        _normalMarkers = new GameObject[_enemies.Length];
        _warningMarkers = new GameObject[_enemies.Length];

        for (int i = 0; i < _enemies.Length; i++)
        {
            _normalMarkers[i] = CreateMarker(_normalMarkerPrefab);
            _warningMarkers[i] = CreateMarker(_warningMarkerPrefab);
        }
    }

    private static GameObject CreateMarker(GameObject prefab)
    {
        if (prefab == null)
            return null;

        GameObject marker = Instantiate(prefab);
        marker.SetActive(false);
        return marker;
    }

    private void UpdateMarkers()
    {
        if (_player == null || _detectCamera == null)
            return;

        Vector3 ringCenter =
            _player.transform.position +
            Vector3.up * _ringHeight;

        for (int i = 0; i < _enemies.Length; i++)
        {
            EnemyCore enemy = _enemies[i];
            GameObject normalMarker = _normalMarkers[i];
            GameObject warningMarker = _warningMarkers[i];

            if (enemy == null)
            {
                SetMarkerActive(normalMarker, false);
                SetMarkerActive(warningMarker, false);
                continue;
            }

            // 화면에 보이는 적은 마커를 표시하지 않는다.
            if (IsEnemyInCamera(enemy))
            {
                SetMarkerActive(normalMarker, false);
                SetMarkerActive(warningMarker, false);
                continue;
            }

            bool showWarning = enemy.IsAttackWarningActive && warningMarker != null;
            GameObject marker = showWarning ? warningMarker : normalMarker;
            GameObject hiddenMarker = showWarning ? normalMarker : warningMarker;

            SetMarkerActive(hiddenMarker, false);
            SetMarkerActive(marker, true);

            if (marker == null)
                continue;

            UpdateMarker(
                marker.transform,
                ringCenter,
                enemy.transform.position
            );
        }
    }

    private static void SetMarkerActive(GameObject marker, bool isActive)
    {
        if (marker != null)
            marker.SetActive(isActive);
    }

    private bool IsEnemyInCamera(EnemyCore enemy)
    {
        Vector3 viewport =
            _detectCamera.WorldToViewportPoint(
                enemy.transform.position
            );

        bool isInFront = viewport.z > 0f;

        bool isInsideScreen =
            viewport.x >= 0f &&
            viewport.x <= 1f &&
            viewport.y >= 0f &&
            viewport.y <= 1f;

        return isInFront && isInsideScreen;
    }

    private void UpdateMarker(
    Transform marker,
    Vector3 ringCenter,
    Vector3 enemyPosition)
    {
        // 플레이어 → 적 방향
        Vector3 direction =
            enemyPosition - _player.transform.position;

        // 높이는 무시하고 XZ 방향만 사용
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        direction.Normalize();

        // 원 둘레에 배치
        marker.position =
            ringCenter +
            direction * _ringRadius;

        // Sprite가 카메라를 바라보면서,
        // Sprite의 +Y가 적 방향을 가리키도록 한다.
        Vector3 toCamera =
            _detectCamera.transform.position -
            marker.position;

        marker.rotation =
            Quaternion.LookRotation(
                toCamera.normalized,
                direction
            );
    }

    private void OnDrawGizmos()
    {
        if (_isDebugPlayerExist)
        {
            PlayerAroundRingGizmo();
        }
    }

    private void PlayerAroundRingGizmo()
    {
#if UNITY_EDITOR
        Handles.color = Color.green;

        Handles.DrawWireDisc(
            _player.transform.position +
            Vector3.up * _ringHeight,
            Vector3.up,
            _ringRadius
        );
#endif
    }
}
