using UnityEngine;

public static class HitDirectionCalculator
{
    public static HitDirectionType GetHitDirection(DamageData damageData, Vector3 myPos, Vector3 myLookDir)
    {
        Vector3 toSender = damageData.Sender.transform.position - myPos;

        myLookDir.y = 0f;
        toSender.y = 0f;

        if (toSender.sqrMagnitude <= 0.001f)
            return HitDirectionType.Front;

        float dot = Vector3.Dot(myLookDir.normalized, toSender.normalized);

        return dot >= 0f ? HitDirectionType.Front : HitDirectionType.Back;
    }   
}