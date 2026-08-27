using UnityEngine;

public class EnemyEngageState : EnemyCompositeState
{
    // 상태 정의
    private readonly EnemyCombatHoldState _holdState;
    private readonly EnemyCombatMoveLeftState _moveLeftState;
    private readonly EnemyCombatMoveRightState _moveRightState;
    private readonly EnemyCombatMoveForwardState _moveForwardState;
    private readonly EnemyCombatMoveBackwardState _moveBackwardState;

    private float _nextAttackTime = 0f;
    private float _decisionTime;
    private bool _isDamaged = false;

    public EnemyEngageState(EnemyCore core) : base(core)
    {
        // 상태 생성
        _holdState = new EnemyCombatHoldState(core, this);
        _moveLeftState = new EnemyCombatMoveLeftState(core, this);
        _moveRightState = new EnemyCombatMoveRightState(core, this);
        _moveForwardState = new EnemyCombatMoveForwardState(core, this);
        _moveBackwardState = new EnemyCombatMoveBackwardState(core, this);
    }

    public override void Enter()
    {
        // 초기화
        _isDamaged = false;

        // 이벤트 연결
        Core.OnDamaged += SetDamaged;

        ResetDecisionTime();
        TransitionSubState(_holdState);
    }

    public override void UpdateTick()
    {
        if (_isDamaged)
            return;

        if (Core.TargetTransform == null)
        {
            Core.StateMachine.Transition(Core.StateMachine.IdleState);
            return;
        }

        Vector3 direction =
            Core.TargetTransform.position - Core.transform.position;

        direction.y = 0f;
        Core.Rotator.RotateToward(direction);

        _decisionTime -= Time.deltaTime;

        if (_decisionTime <= 0f)
        {
            SelectNextAction();
            ResetDecisionTime();
        }

        base.UpdateTick();
    }

    public override void Exit()
    {
        // 초기화
        _isDamaged = false;

        // 이벤트 해제
        Core.OnDamaged -= SetDamaged;

        base.Exit();
    }

    private void SelectNextAction()
    {
        if (CanEnterSlashAttack())
        {
            Core.StateMachine.Transition(Core.StateMachine.SlashAttackState);
            return;
        }

        float distance = Vector3.Distance(
            Core.transform.position,
            Core.TargetTransform.position);

        if (distance < 2f)
        {
            TransitionSubState(_moveBackwardState);
            return;
        }

        if (distance > 5f)
        {
            TransitionSubState(_moveForwardState);
            return;
        }

        float value = Random.value;

        if (value < 0.25f)
            TransitionSubState(_holdState);
        else if (value < 0.60f)
            TransitionSubState(_moveLeftState);
        else
            TransitionSubState(_moveRightState);
    }

    private void ResetDecisionTime()
    {
        _decisionTime = Random.Range(0.4f, 1.1f);
    }

    private void SetDamaged(DamageData damageData)
    {
        _isDamaged = true;

        HitDirectionType type = HitDirectionCalculator.GetHitDirection(damageData, Core.transform.position, Core.Rotator.FacingDirection);
        if (type == HitDirectionType.Front)
            Core.StateMachine.Transition(Core.StateMachine.FrontHitState);
        else if (type == HitDirectionType.Back)
            Core.StateMachine.Transition(Core.StateMachine.BackHitState);
    }

    private bool CanEnterSlashAttack()
    {
        Transform target = Core.TargetTransform;

        if (target == null)
            return false;

        Vector3 toTarget = target.position - Core.transform.position;
        toTarget.y = 0f;

        float sqrDistance = toTarget.sqrMagnitude;

        if (sqrDistance > Core.SlashAttackRange * Core.SlashAttackRange)
            return false;

        if (sqrDistance <= 0.001f)
            return false;

        Vector3 targetDirection = toTarget.normalized;

        float facingDot = Vector3.Dot(
            Core.Rotator.FacingDirection,
            targetDirection);

        float minimunAttackFacingDot = Mathf.Cos(Core.SlashAttackAngle * Mathf.Deg2Rad);
        if (facingDot < minimunAttackFacingDot)
            return false;

        if (Time.time < _nextAttackTime)
            return false;

        return true;
    }
}
