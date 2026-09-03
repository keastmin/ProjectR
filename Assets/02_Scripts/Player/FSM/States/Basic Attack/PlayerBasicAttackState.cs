using UnityEngine;
using UnityEngine.Playables;

public abstract class PlayerBasicAttackState : PlayerStateBase
{
    private PlayableDirector _director;
    private bool _isNextComboEnable;
    private bool _isOtherBehaviourEnable;

    protected abstract DirectorID AttackDirectorId { get; }
    protected abstract string AnimationTrigger { get; }

    protected PlayerBasicAttackState(PlayerCore core) : base(core)
    {
        _director = core.DirectorContainer.Directors[AttackDirectorId];
    }

    public override void Enter()
    {
        Debug.Log($"{GetType().Name} 진입");
        Core.Animator.SetTrigger(AnimationTrigger);
        Core.DirectorContainer.Play(AttackDirectorId);
        _isNextComboEnable = false;
        _isOtherBehaviourEnable = false;
        Core.AnimationEvent.OnEnableNextBasicAttack += HandleNextComboEnableEvent;
        Core.AnimationEvent.OnDisableNextBasicAttack += HandleNextComboDisableEvent;

        Collider target = Core.TargetDetector.AcquireBasicAttackTarget();
        Vector3 targetDirection = target == null
            ? Core.DirCalculator.GetTargetDirection(Core.InputCollector.MoveValue, Core.MainCamera.transform)
            : (target.transform.position - Core.transform.position).normalized;
        Core.Rotator.RotateImmediately(targetDirection);

        base.Enter();
    }

    public override void UpdateTick()
    {
        if (IsDamaged)
            return;

        AnimatorStateInfo stateInfo = Core.Animator.IsInTransition(0)
            ? Core.Animator.GetNextAnimatorStateInfo(0)
            : Core.Animator.GetCurrentAnimatorStateInfo(0);

        if (Core.InputCollector.IsInputDodge)
        {
            Core.StateMachine.Transition(Core.InputCollector.IsInputMove
                ? Core.StateMachine.FrontDodgeState
                : Core.StateMachine.BackDodgeState);
            return;
        }

        if (_isNextComboEnable && Core.InputCollector.IsInputAttack)
        {
            TransitionNextCombo();
            return;
        }

        if (_isOtherBehaviourEnable)
        {
            if (Core.InputCollector.IsInputAttack)
                Core.StateMachine.Transition(Core.StateMachine.BasicAttack1State);
            else if (Core.InputCollector.IsInputMove)
                Core.StateMachine.Transition(Core.StateMachine.RunStartState);
            return;
        }

        if (stateInfo.normalizedTime >= 0.92f)
            Core.StateMachine.Transition(Core.StateMachine.IdleState);
    }

    public override void FixedTick()
    {
        if (IsDamaged)
            return;

        MoveRootMotionAlongFacingDirection();
    }

    public override void AnimatorTick()
    {
        if (IsDamaged)
            return;

        AnimDeltaPos += Core.Animator.deltaPosition;
    }

    public override void Exit()
    {
        Core.AttackInstanceContainer.ClearDamagedTargets();
        Core.AnimationEvent.OnEnableNextBasicAttack -= HandleNextComboEnableEvent;
        Core.AnimationEvent.OnDisableNextBasicAttack -= HandleNextComboDisableEvent;

        if (_director.state == PlayState.Playing)
            _director.Stop();

        base.Exit();
    }

    protected abstract void TransitionNextCombo();

    private void HandleNextComboEnableEvent() => _isNextComboEnable = true;

    private void HandleNextComboDisableEvent()
    {
        _isNextComboEnable = false;
        _isOtherBehaviourEnable = true;
    }
}
