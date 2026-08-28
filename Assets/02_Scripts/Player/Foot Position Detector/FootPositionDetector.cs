using UnityEngine;

public class FootPositionDetector : MonoBehaviour
{
    [SerializeField] private Transform _modelTransform;
    [SerializeField] private Transform _lFootTransform;
    [SerializeField] private Transform _rFootTransform;

    // 현재 더 앞에 있는 발이 어느 발인지 알아냄
    public FrontFoot GetCurrentFrontFoot()
    {
        Vector3 leftLocal = _modelTransform.InverseTransformPoint(_lFootTransform.position);
        Vector3 rightLocal = _modelTransform.InverseTransformPoint(_rFootTransform.position);

        return (leftLocal.z > rightLocal.z) ? FrontFoot.LeftFoot : FrontFoot.RightFoot;
    }
}