#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class AnimationRootMotionMultiplierPreview : MonoBehaviour
{
    [SerializeField] private Transform _animatedObject;
    [SerializeField] private float _multiply = 2.5f;

    private Vector3 _originParentPosition;
    private Quaternion _originParentRotation;

    private Vector3 _originAnimatedLocalPosition;

    private bool _captured;

    private void OnEnable()
    {
        EditorApplication.update -= EditorUpdate;
        EditorApplication.update += EditorUpdate;
    }

    private void OnDisable()
    {
        EditorApplication.update -= EditorUpdate;
    }

    [ContextMenu("Capture At Animation Start")]
    private void Capture()
    {
        _originParentPosition = transform.position;
        _originParentRotation = transform.rotation;

        _originAnimatedLocalPosition =
            _animatedObject.localPosition;

        _captured = true;
    }

    [ContextMenu("Reset")]
    private void ResetPreview()
    {
        transform.SetPositionAndRotation(
            _originParentPosition,
            _originParentRotation
        );
    }

    private void EditorUpdate()
    {
        if (Application.isPlaying)
            return;

        if (!_captured || _animatedObject == null)
            return;

        if (!AnimationMode.InAnimationMode())
        {
            transform.SetPositionAndRotation(
                _originParentPosition,
                _originParentRotation
            );

            return;
        }

        Vector3 rootMotionOffset =
            _animatedObject.localPosition -
            _originAnimatedLocalPosition;

        // Character 자체가 이미 1배 움직이고 있으므로
        // 부모에는 나머지만 추가한다.
        Vector3 additionalOffset =
            rootMotionOffset * (_multiply - 1f);

        transform.position =
            _originParentPosition +
            _originParentRotation * additionalOffset;

        SceneView.RepaintAll();
    }
}

#endif