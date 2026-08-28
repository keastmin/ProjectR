using UnityEngine;

public class PlayerHitFrontState : PlayerHitState
{
    protected override string AnimationTrigger => "IsFrontHit";

    public PlayerHitFrontState(PlayerCore core) : base(core) { }

    public override void Enter()
    {
        // 적의 FrontHitState와 동일하게, 앞에서 맞으면 공격자를 즉시 바라본다.
        RotateImmediatelyForHitReaction(faceAttacker: true);
        base.Enter();
    }
}
