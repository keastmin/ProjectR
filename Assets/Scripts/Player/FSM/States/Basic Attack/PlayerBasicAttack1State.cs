using UnityEngine;
using UnityEngine.Playables;

public class PlayerBasicAttack1State : PlayerStateBase
{
    private PlayableDirector _director;

    private Vector3 _animDeltaPos = Vector3.zero;

    public PlayerBasicAttack1State(PlayerCore player) : base(player)
    {
        _director = player.DirectorContainer.Directors[DirectorID.BasicAttack1];
    }

    public override void Enter()
    {
        Vector3 targetDirection = Core.DirCalculator.GetTargetDirection(Core.InputCollector.MoveValue, Core.MainCamera.transform);
        Core.Rotator.RotateImmediately(targetDirection);

        _director.stopped += OnTimelineFinished;
        _director.time = 0;
        _director.Play();
    }

    public override void UpdateTick()
    {
        //AnimatorStateInfo stateInfo = Core.Animator.GetCurrentAnimatorStateInfo(0);
        //bool isSameState = stateInfo.fullPathHash == _animHash;
        //float normalizeTime = stateInfo.normalizedTime;

        //// 기본 공격 입력이 있다면 다음 공격으로 전환
        //if (_isCanNextBasicAttack && Core.InputCollector.IsInputAttack)
        //{
        //    Core.StateMachine.Transition(Core.StateMachine.BasicAttack2State, "IsBasicAttack");
        //    return;
        //}

        //// 이동 입력이 있다면 달리기 시작으로 전환
        //if(_isCanOtherBehaviour && Core.InputCollector.IsInputMove)
        //{
        //    Core.StateMachine.Transition(Core.StateMachine.RunStartState, "IsRunStart");
        //    return;
        //}

        //// 공격 모션이 모두 끝나면 Idle 상태로 전환
        //if(normalizeTime >= _animEndTime && isSameState)
        //{
        //    Core.StateMachine.Transition(Core.StateMachine.IdleState, "IsIdle");
        //    return;
        //}
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
        _director.stopped -= OnTimelineFinished;

        if (_director.state == PlayState.Playing)
            _director.Stop();

        _animDeltaPos = Vector3.zero;
    }

    private void OnTimelineFinished(PlayableDirector director)
    {
        Core.StateMachine.Transition(Core.StateMachine.IdleState);
    }
}
