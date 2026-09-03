using UnityEngine;

public class EnemyCloseAttackState : EnemyAttackState
{
    protected override string AnimationTrigger => "IsCloseAttack";

    private const EnemyAttackID SecondAttackID = EnemyAttackID.CloseAttack2Hit;
    private static readonly int SecondAttackStateHash =
        Animator.StringToHash("Attack_Combo_01_02_Anim");
    private readonly EnemyAttackSO _secondAttack;
    private bool _secondPhaseStarted;
    private bool _attackSequenceCompleted;

    public EnemyCloseAttackState(EnemyCore core) : base(core)
    {
        core.AttackDataDictionary.TryGetValue(SecondAttackID, out _secondAttack);
    }

    public override void Enter()
    {
        _secondPhaseStarted = false;
        _attackSequenceCompleted = false;
        Core.AnimationEvent.OnAttack += HandleAttack;

        base.Enter();
    }

    public override void UpdateTick()
    {
        TryBeginSecondAttackPhase();

        if (Core.AttackTargeting.IsTracking)
            Core.UpdateAttackTargeting();

        EnemyAttackSO attackData = Core.AttackTargeting.AttackData;
        float rotationSpeed = attackData != null
            ? attackData.AttackRotationAnglePerSecond
            : 0f;
        Core.Rotator.RotateToward(Core.AttackTargeting.TargetForward, rotationSpeed);

        base.UpdateTick();
    }

    public override void AnimatorTick()
    {
        TryBeginSecondAttackPhase();

        if (_attackSequenceCompleted)
            return;

        AnimDeltaPos += Core.AttackTargeting.WarpRootMotion(
            Core.Animator.deltaPosition,
            AnimDeltaPos);
    }

    public override void Exit()
    {
        _secondPhaseStarted = false;
        _attackSequenceCompleted = false;
        Core.AnimationEvent.OnAttack -= HandleAttack;

        Core.AnimationEvent.SwordTrailEffectActive(false);
        base.Exit();
    }

    private void HandleAttack(EnemyAttackSO attackSO)
    {
        if (attackSO != null && attackSO.AttackID == EnemyAttackID.CloseAttack1Hit)
            return;

        _attackSequenceCompleted = true;
        Core.EndAttackTargeting();
    }

    private void TryBeginSecondAttackPhase()
    {
        if (_secondPhaseStarted || _secondAttack == null || _attackSequenceCompleted)
            return;

        Animator animator = Core.Animator;
        bool isSecondState =
            animator.GetCurrentAnimatorStateInfo(0).shortNameHash == SecondAttackStateHash;

        if (!isSecondState && animator.IsInTransition(0))
        {
            isSecondState =
                animator.GetNextAnimatorStateInfo(0).shortNameHash == SecondAttackStateHash;
        }

        if (!isSecondState)
            return;

        _secondPhaseStarted = true;
        Core.BeginAttackTargeting(_secondAttack);
    }
}
