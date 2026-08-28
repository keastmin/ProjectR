public class PlayerBasicAttack2State : PlayerBasicAttackState
{
    protected override DirectorID AttackDirectorId => DirectorID.BasicAttack2;
    protected override string AnimationTrigger => "IsBasicAttackNext";

    public PlayerBasicAttack2State(PlayerCore core) : base(core) { }

    protected override void TransitionNextCombo() => Core.StateMachine.Transition(Core.StateMachine.BasicAttack3State);
}
