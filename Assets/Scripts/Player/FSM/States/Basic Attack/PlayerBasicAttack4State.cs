using UnityEngine;
using UnityEngine.Playables;

public class PlayerBasicAttack4State : PlayerStateBase
{
    private PlayableDirector _director;

    private bool _isNextComboEnable = false;
    private bool _isOtherBehaviourEnable = false;

    public PlayerBasicAttack4State(PlayerCore player) : base(player)
    {
        _director = player.DirectorContainer.Directors[DirectorID.BasicAttack4];
    }

    public override void Enter()
    {
        Debug.Log("PlayerBasicAttack4State 진입");
        // 애니메이션 재생
        Core.Animator.SetTrigger("IsBasicAttack");

        // 타임 라인 재생
        _director.time = 0;
        _director.Play();

        _isNextComboEnable = false;
        _isOtherBehaviourEnable = false;
        Core.AnimationEvent.OnEnableNextBasicAttack += HandleNextComboEnableEvent;
        Core.AnimationEvent.OnDisableNextBasicAttack += HandleNextComboDisableEvent;

        Vector3 targetDirection = Core.DirCalculator.GetTargetDirection(Core.InputCollector.MoveValue, Core.MainCamera.transform);
        Core.Rotator.RotateImmediately(targetDirection);
    }

    public override void UpdateTick()
    {
        AnimatorStateInfo stateInfo = Core.Animator.IsInTransition(0) ?
            Core.Animator.GetNextAnimatorStateInfo(0) :
            Core.Animator.GetCurrentAnimatorStateInfo(0);
        float normalizeTime = stateInfo.normalizedTime;

        // 기본 공격 입력이 있다면 다음 공격으로 전환
        if (_isNextComboEnable && Core.InputCollector.IsInputAttack)
        {
            Core.StateMachine.Transition(Core.StateMachine.BasicAttack1State);
            return;
        }

        // 이동 입력이 있다면 달리기 시작으로 전환
        if (_isOtherBehaviourEnable && Core.InputCollector.IsInputMove)
        {
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
