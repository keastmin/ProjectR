using UnityEngine;

public class PlayerBasicAttack1State : PlayerStateBase
{
    private float _animEndTime = 0.99f;
    private bool _isCanOtherBehaviour = false;
    private bool _isCanNextBasicAttack = false;
    private Vector3 _animDeltaPos = Vector3.zero;
    private string _animPath = "Base Layer.Basic Attack.Basic Attack 1";
    private int _animHash;

    public PlayerBasicAttack1State(PlayerCore player) : base(player)
    {
        _animHash = Animator.StringToHash(_animPath);
    }

    public override void Enter()
    {
        _isCanOtherBehaviour = false;
        _isCanNextBasicAttack = false;
        Core.AnimationEvent.OnEnableNextBasicAttack += EnableNextAttack;
        Core.AnimationEvent.OnDisableNextBasicAttack += DisableNextAttack;
    }

    public override void UpdateTick()
    {
        AnimatorStateInfo stateInfo = Core.Animator.GetCurrentAnimatorStateInfo(0);
        bool isSameState = stateInfo.fullPathHash == _animHash;
        float normalizeTime = stateInfo.normalizedTime;

        // 기본 공격 입력이 있다면 다음 공격으로 전환
        if (_isCanNextBasicAttack && Core.InputCollector.IsInputAttack)
        {
            Core.StateMachine.Transition(Core.StateMachine.BasicAttack2State, "IsBasicAttack");
            return;
        }

        // 이동 입력이 있다면 달리기 시작으로 전환
        if(_isCanOtherBehaviour && Core.InputCollector.IsInputMove)
        {
            Core.StateMachine.Transition(Core.StateMachine.RunStartState, "IsRunStart");
            return;
        }

        // 공격 모션이 모두 끝나면 Idle 상태로 전환
        if(normalizeTime >= _animEndTime && isSameState)
        {
            Core.StateMachine.Transition(Core.StateMachine.IdleState, "IsIdle");
            return;
        }
    }

    public override void FixedTick()
    {
        Core.Mover.Move(Core.Rotator.FacingRotation * (_animDeltaPos / Time.fixedDeltaTime));
        _animDeltaPos = Vector3.zero;
    }

    public override void AnimatorTick()
    {
        _animDeltaPos += Core.Animator.deltaPosition;
    }

    public override void Exit()
    {
        Core.AnimationEvent.OnEnableNextBasicAttack -= EnableNextAttack;
        Core.AnimationEvent.OnDisableNextBasicAttack -= DisableNextAttack;
        _animDeltaPos = Vector3.zero;
        _isCanOtherBehaviour = false;
        _isCanNextBasicAttack = false;
    }

    private void EnableNextAttack()
    {
        _isCanNextBasicAttack = true;
    }

    private void DisableNextAttack()
    {
        _isCanNextBasicAttack = false;
        _isCanOtherBehaviour = true;
    }
}