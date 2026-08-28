using UnityEngine;

public class PlayerHitBackState : PlayerHitState
{
    protected override string AnimationTrigger => "IsBackHit";

    public PlayerHitBackState(PlayerCore core) : base(core) { }

    public override void Enter()
    {
        // 적의 BackHitState와 동일하게, 뒤에서 맞으면 공격자 반대쪽을 즉시 바라본다.
        RotateImmediatelyForHitReaction(faceAttacker: false);
        base.Enter();
    }
}
