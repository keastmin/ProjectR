using UnityEngine;

public class PlayerHitLargeState : PlayerHitState
{
    protected override string AnimationTrigger => "IsLargeHit";

    public PlayerHitLargeState(PlayerCore core) : base(core) { }
}