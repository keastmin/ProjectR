public class PlayerBasicAttack3State : PlayerBasicAttackState
{
    protected override DirectorID AttackDirectorId => DirectorID.BasicAttack3;
    protected override string AnimationTrigger => "IsBasicAttackNext";

    public PlayerBasicAttack3State(PlayerCore core) : base(core) { }

    protected override void TransitionNextCombo() => Core.StateMachine.Transition(Core.StateMachine.BasicAttack4State);
}
