using UnityEngine;
using UnityEngine.Playables;

public class PlayerSkillState : PlayerStateBase
{
    public override bool IsInvulnerable => true;
    private PlayableDirector _director;
    private bool _isTransitionIdle = false;

    public PlayerSkillState(PlayerCore core) : base(core)
    {
        _director = core.DirectorContainer.Directors[DirectorID.Skill];
    }

    public override void Enter()
    {
        // 초기화
        _isTransitionIdle = false;

        // 이벤트 연결
        Core.AnimationEvent.OnAnimationEnd += SetTransitionIdle;

        // 스킬 사용
        Core.UseSkillGauge();

        // 타임라인 재생
        Core.DirectorContainer.Play(DirectorID.Skill);

        // 회전
        Collider target = Core.TargetDetector.AcquireBasicAttackTarget();
        Vector3 targetDirection = target == null
            ? Core.DirCalculator.GetTargetDirection(Core.InputCollector.MoveValue, Core.MainCamera.transform)
            : (target.transform.position - Core.transform.position).normalized;
        Core.Rotator.RotateImmediately(targetDirection);

        base.Enter();
    }

    public override void UpdateTick()
    {
        double currTime = _director.time;
        double totalTime = _director.duration;
        float progress = (float)(currTime / totalTime);
        if (currTime > 2.05f)
        {
            Core.StateMachine.Transition(Core.StateMachine.IdleState);
            return;
        }
    }

    public override void FixedTick()
    {
        Core.Mover.Move(AnimDeltaPos / Time.fixedDeltaTime);
        AnimDeltaPos = Vector3.zero;
    }

    public override void AnimatorTick()
    {
        AnimDeltaPos += Core.Animator.deltaPosition;
    }

    public override void OnHitStopStarted()
    {
        // Hit notifications arrive between animation and physics updates. Keep
        // this frame's root motion, plus any movement awaiting a physics tick,
        // so repeated skill hits pause the dash instead of deleting its distance.
        AnimDeltaPos += Core.Mover.ConsumePendingDisplacement();
    }

    public override void Exit()
    {
        // 초기화
        _isTransitionIdle = false;

        // 이벤트 해제
        Core.AnimationEvent.OnAnimationEnd -= SetTransitionIdle;

        // 타임라인 초기화
        // Paused timelines also own temporary model-fade materials.
        _director.Stop();

        base.Exit();
    }

    private void SetTransitionIdle()
    {
        _isTransitionIdle = true;
    }
}
