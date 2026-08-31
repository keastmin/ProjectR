using UnityEngine;

public class EnemyCloseAttackState : EnemyAttackState
{
    protected override string AnimationTrigger => "IsCloseAttack";

    public EnemyCloseAttackState(EnemyCore core) : base(core) { }

    public override void Enter()
    {
        // 이벤트 연결
        Core.AnimationEvent.OnSetAttackNoticeWindow += SetAttackNoticeCollider;

        base.Enter();
    }

    public override void UpdateTick()
    {
        Vector3 direction =
            Core.TargetTransform.position - Core.transform.position;

        direction.y = 0f;
        Core.Rotator.RotateToward(direction);

        base.UpdateTick();
    }

    public override void AnimatorTick()
    {
        AnimDeltaPos += (Core.Animator.deltaPosition * 3f);
    }

    public override void Exit()
    {
        // 이벤트 해제
        Core.AnimationEvent.OnSetAttackNoticeWindow -= SetAttackNoticeCollider;

        // 공격 예고 콜라이더 정리
        Core.ClearAttackNoticeCollider();

        Core.AnimationEvent.SwordTrailEffectActive(false);
        base.Exit();
    }

    private void SetAttackNoticeCollider(int index)
    {
        if (index < 0)
        {
            Core.ClearAttackNoticeCollider();
            return;
        }

        Collider[] noticeColliders = { Core.CloseAttackNotiveBoxies[index] };
        Core.SetAttackNoticeCollider(noticeColliders);
    }
}