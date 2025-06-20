// Pullup State - handles ground avoidance
public class PullupState : IAIState
{
    public void Enter(AI_Fighter fighter)
    {
        // Ground avoidance initialization
    }

    public void Update(AI_Fighter fighter)
    {
        // Pull up logic - transition back to previous state when clear
        fighter.stateMachine.ChangeState("ATTACK");
    }

    public void Exit(AI_Fighter fighter) { }
    public string GetStateName() => "PULLUP";
}