public class PlayerBasicAttack1State : PlayerBasicAttackState
{
    protected override DirectorID AttackDirectorId => DirectorID.BasicAttack1;
    protected override string AnimationTrigger => "IsBasicAttackStart";

    public PlayerBasicAttack1State(PlayerCore core) : base(core) { }

    protected override void TransitionNextCombo() => Core.StateMachine.Transition(Core.StateMachine.BasicAttack2State);
}
