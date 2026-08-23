using UnityEngine;

public class PlayerRunStopRightState : PlayerStateBase
{
    private string _rFootAnimName = "Run Stop R Foot";

    private float _endAnimNormalTime = 0.99f;
    private AnimatorStateInfo _stateInfo;
    private Vector3 _animDeltaPos;
    private int _animHash;

    public PlayerRunStopRightState(PlayerCore player) : base(player)
    {
        _animHash = Animator.StringToHash("Base Layer.Run." + _rFootAnimName);
    }

    public override void Enter()
    {
        Core.Animator.SetTrigger("IsRunStopRight");
        _animDeltaPos = Vector3.zero;
    }

    public override void UpdateTick()
    {
        _stateInfo = Core.Animator.GetCurrentAnimatorStateInfo(0);
        float currAnimNormalTime = _stateInfo.normalizedTime;
        bool isRunStopState = _stateInfo.fullPathHash == _animHash;

        // 기본 공격 입력이 있다면 기본 공격 상태로 전환
        if (Core.InputCollector.IsInputAttack)
        {
            Core.StateMachine.Transition(Core.StateMachine.BasicAttack1State);
            return;
        }

        // 이동 입력이 있다면 달리기 시작
        if (Core.InputCollector.IsInputMove)
        {
            Core.StateMachine.Transition(Core.StateMachine.RunStartState);
            return;
        }

        // 이동 입력이 없다면 달리기 종료
        if (currAnimNormalTime >= _endAnimNormalTime && isRunStopState)
        {
            Core.StateMachine.Transition(Core.StateMachine.IdleState);
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
        _animDeltaPos = Vector3.zero;
    }
}
