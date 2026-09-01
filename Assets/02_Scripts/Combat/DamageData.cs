using UnityEngine;

public enum StaggerLevel
{
    None = 0,
    Level1 = 1,
    Level2 = 2,
    Level3 = 3,
    Level4 = 4,
    Level5 = 5
}

public struct DamageData
{
    public GameObject Sender;
    public float DamageAmount;
    public int HitStopFrame;
    public StaggerLevel StaggerLevel;

    public DamageData(
        GameObject sender,
        float damageAmount,
        int hitStopFrame,
        StaggerLevel staggerLevel = StaggerLevel.None)
    {
        Sender = sender;
        DamageAmount = damageAmount;
        HitStopFrame = hitStopFrame;
        StaggerLevel = staggerLevel;
    }
}
