public class PlayerBasicAttack4State : PlayerBasicAttackState
{
    protected override DirectorID AttackDirectorId => DirectorID.BasicAttack4;
    protected override string AnimationTrigger => "IsBasicAttackNext";

    public PlayerBasicAttack4State(PlayerCore core) : base(core) { }

    protected override void TransitionNextCombo() => Core.StateMachine.Transition(Core.StateMachine.BasicAttack1State);
}
