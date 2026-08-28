using UnityEngine;

[ExecuteAlways]
public class TimelineRootMotionTester : MonoBehaviour
{
    void OnAnimatorMove()
    {
#if UNITY_EDITOR
        // 에디터 미리보기(Scrubbing) 또는 플레이 모드에서 작동
        if (GetComponent<Animator>() != null)
        {
            transform.position += GetComponent<Animator>().deltaPosition;
            transform.rotation *= GetComponent<Animator>().deltaRotation;
        }
#endif
    }
}
