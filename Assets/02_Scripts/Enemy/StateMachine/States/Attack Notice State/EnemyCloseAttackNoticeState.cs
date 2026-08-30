using UnityEngine;

public class EnemyCloseAttackNoticeState : EnemyAttackNoticeState
{
    private const EnemyAttackID _attackID1 = EnemyAttackID.CloseAttack1Hit;
    private const EnemyAttackID _attackID2 = EnemyAttackID.CloseAttack2Hit;
    private EnemyAttackSO _attackSO1;
    private EnemyAttackSO _attackSO2;

    private float _currentTime = 0f;

    public EnemyCloseAttackNoticeState(EnemyCore core) : base(core)
    {
        _attackSO1 = core.AttackDataDictionary[_attackID1];
        _attackSO2 = core.AttackDataDictionary[_attackID2];
    }

    public override void Enter()
    {
        base.Enter();
        _currentTime = 0f;
        PushAttackWindow();
    }

    public override void UpdateTick()
    {
        _currentTime += Time.deltaTime;
        if(_currentTime >= _attackSO1.AttackAnimationTransitionTime)
        {
            Core.StateMachine.Transition(Core.StateMachine.CloseAttackState);
            return;
        }

        base.UpdateTick();
    }

    public override void Exit()
    {
        base.Exit();
    }

    private void PushAttackWindow()
    {

    }
}
