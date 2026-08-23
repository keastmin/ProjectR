using UnityEngine;

public class PlayerRunStartState : PlayerStateBase
{
    private string _animName = "Run Start";

    private float _endAnimNormalTime = 0.99f;
    private AnimatorStateInfo _stateInfo;
    private Vector3 _animDeltaPos;
    private int _animHash;

    public PlayerRunStartState(PlayerCore player) : base(player)
    {
        _animHash = Animator.StringToHash("Base Layer.Run." + _animName);
    }

    public override void Enter()
    {
        Core.Animator.SetTrigger("IsRunStart");
        _animDeltaPos = Vector3.zero;
    }

    public override void UpdateTick()
    {
        _stateInfo = Core.Animator.GetCurrentAnimatorStateInfo(0);
        float currAnimNormalTime = _stateInfo.normalizedTime;
        bool isRunStartState = _stateInfo.fullPathHash == _animHash;

        // 회전
        Rotation();

        // 기본 공격 입력이 있다면 기본 공격 상태로 전환
        if (Core.InputCollector.IsInputAttack)
        {
            Core.StateMachine.Transition(Core.StateMachine.BasicAttack1State);
            return;
        }

        // 이동 입력이 없다면 달리기 종료
        if (!Core.InputCollector.IsInputMove)
        {
            FrontFoot currentFrontFoot = Core.FootPosDetector.GetCurrentFrontFoot();
            if (currentFrontFoot == FrontFoot.LeftFoot)
            {
                Core.StateMachine.Transition(Core.StateMachine.RunStopLeftState);
                return;
            }
            else
            {
                Core.StateMachine.Transition(Core.StateMachine.RunStopRightState);
                return;
            }
        }

        // 애니메이션 종료까지 입력이 있다면 달리기 유지
        if(currAnimNormalTime >= _endAnimNormalTime && isRunStartState)
        {
            Core.StateMachine.Transition(Core.StateMachine.RunLoopState);
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

    private void Rotation()
    {
        Vector3 targetDirection = Core.DirCalculator.GetTargetDirection(Core.InputCollector.MoveValue, Core.MainCamera.transform);
        Core.Rotator.RotateToward(targetDirection);
    }
}