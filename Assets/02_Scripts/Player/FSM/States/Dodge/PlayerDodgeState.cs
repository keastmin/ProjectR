using UnityEngine;

public abstract class PlayerDodgeState : PlayerStateBase
{
    protected abstract string AnimationTrigger { get; }

    public PlayerDodgeState(PlayerCore core) : base(core) { }

    protected bool IsPerfectDodge = false;
    protected bool IsPerfectDodgeEnd = false;

    public override void Enter()
    {
        // 초기화
        IsPerfectDodge = false;
        IsPerfectDodgeEnd = false;

        // 애니메이션 재생
        Core.Animator.SetTrigger(AnimationTrigger);

        // 회피 판별
        if (Core.IsPerfectDodge(out EnemyCore enemy))
        {
            IsPerfectDodge = true;

            Debug.Log(enemy.name);

            // 플레이어, 적 전체, VFX 슬로우 모션 Fade In 진입
            // 화면 Effect 발동
            // 플레이어 캐릭터 트레일 연출
            // 이 모든 것을 여기서 직접 호출하지 않고 Core의 함수를 호출하여 Action을 Invoke를 하는 등 외부에서 느슨하게 호출 가능한 구조를 통해 진행한다.
        }

        base.Enter();
    }

    public override void FixedTick()
    {
        if (IsDamaged)
            return;

        Core.Mover.Move(AnimDeltaPos / Time.fixedDeltaTime);
        AnimDeltaPos = Vector3.zero;
    }

    public override void AnimatorTick()
    {
        if (IsDamaged)
            return;

        AnimDeltaPos += Core.Animator.deltaPosition;
    }

    public override void Exit()
    {
        // 초기화
        IsPerfectDodge = false;
        IsPerfectDodgeEnd = false;

        // 애니메이션 트리거 리셋
        Core.Animator.ResetTrigger(AnimationTrigger);

        base.Exit();
    }
}