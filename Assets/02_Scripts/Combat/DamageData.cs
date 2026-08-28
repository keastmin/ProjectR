using UnityEngine;

public struct DamageData
{
    public GameObject Sender;
    public float DamageAmount;
    public int HitStopFrame;

    public DamageData(GameObject sender, float damageAmount, int hitStopFrame)
    {
        Sender = sender;
        DamageAmount = damageAmount;
        HitStopFrame = hitStopFrame;
    }
}