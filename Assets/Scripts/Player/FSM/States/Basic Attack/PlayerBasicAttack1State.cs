using UnityEngine;
using UnityEngine.Playables;

public class PlayerBasicAttack1State : PlayerStateBase
{
    private PlayableDirector _director;

    private bool _isNextComboEnable = false;
    private bool _isOtherBehaviourEnable = false;

    public PlayerBasicAttack1State(PlayerCore player) : base(player)
    {
        _director = player.DirectorContainer.Directors[DirectorID.BasicAttack1];
    }

    public override void Enter()
    {
        Debug.Log("PlayerBasicAttack1State 진입");
        // 애니메이션 재생
        Core.Animator.SetTrigger("IsBasicAttackStart");

        // 타임 라인 재생
        Core.DirectorContainer.Play(DirectorID.BasicAttack1);

        _isNextComboEnable = false;
        _isOtherBehaviourEnable = false;
        Core.AnimationEvent.OnEnableNextBasicAttack += HandleNextComboEnableEvent;
        Core.AnimationEvent.OnDisableNextBasicAttack += HandleNextComboDisableEvent;

        Vector3 targetDirection = Core.TargetDetector.NearestEnemyCollider == null ?
            Core.DirCalculator.GetTargetDirection(Core.InputCollector.MoveValue, Core.MainCamera.transform) :
            Core.TargetDetector.NearestEnemyDirection;
        Core.Rotator.RotateImmediately(targetDirection);
    }

    public override void UpdateTick()
    {
        AnimatorStateInfo stateInfo = Core.Animator.IsInTransition(0) ?
            Core.Animator.GetNextAnimatorStateInfo(0) :
            Core.Animator.GetCurrentAnimatorStateInfo(0);
        float normalizeTime = stateInfo.normalizedTime;

        // 회피 입력이 있다면 회피
        if (Core.InputCollector.IsInputDodge)
        {
            if (Core.InputCollector.IsInputMove)
                Core.StateMachine.Transition(Core.StateMachine.FrontDodgeState);
            else
                Core.StateMachine.Transition(Core.StateMachine.BackDodgeState);
            return;
        }

        // 다음 콤보가 가능하고 기본 공격 입력이 있다면 다음 공격으로 전환
        if (_isNextComboEnable && Core.InputCollector.IsInputAttack)
        {
            Core.StateMachine.Transition(Core.StateMachine.BasicAttack2State);
            return;
        }

        // 다른 행동이 가능할 때
        if (_isOtherBehaviourEnable)
        {
            // 이동 입력이 있다면 달리기, 공격 입력이 있다면 1타로 전환
            if (Core.InputCollector.IsInputAttack)
                Core.StateMachine.Transition(Core.StateMachine.BasicAttack1State);
            else if (Core.InputCollector.IsInputMove)
                Core.StateMachine.Transition(Core.StateMachine.RunStartState);
            return;
        }

        // 공격 모션이 모두 끝나면 Idle 상태로 전환
        if (normalizeTime >= 0.92f)
        {
            Core.StateMachine.Transition(Core.StateMachine.IdleState);
            return;
        }
    }

    public override void FixedTick()
    {
        Core.Mover.Move(Core.Rotator.FacingRotation * (AnimDeltaPos / Time.fixedDeltaTime));
        AnimDeltaPos = Vector3.zero;
    }

    public override void AnimatorTick()
    {
        AnimDeltaPos += Core.Animator.deltaPosition;
    }

    public override void Exit()
    {
        Core.AttackInstanceContainer.ClearDamagedTargets();
        Core.AnimationEvent.OnEnableNextBasicAttack -= HandleNextComboEnableEvent;
        Core.AnimationEvent.OnDisableNextBasicAttack -= HandleNextComboDisableEvent;

        if (_director.state == PlayState.Playing)
            _director.Stop();
    }

    private void HandleNextComboEnableEvent()
    {
        _isNextComboEnable = true;
    }

    private void HandleNextComboDisableEvent()
    {
        _isNextComboEnable = false;
        _isOtherBehaviourEnable = true;
    }
}
