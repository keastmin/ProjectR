using UnityEngine;

public class DirectionCalculator
{
    // 움직임 입력과 카메라 트랜스폼을 바탕으로 목표 회전 방향을 찾는 함수
    public Vector3 GetTargetDirection(Vector2 moveValue, Transform lookCam)
    {
        Vector3 cameraForward = lookCam.forward;
        Vector3 cameraRight = lookCam.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 targetDirection = cameraForward * moveValue.y + cameraRight * moveValue.x;

        return targetDirection.normalized;
    }
}