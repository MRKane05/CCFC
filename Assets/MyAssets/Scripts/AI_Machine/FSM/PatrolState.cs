// Patrol State - handles patrol behavior
public class PatrolState : IAIState
{
    public void Enter(AI_Fighter fighter)
    {
        // Patrol initialization
    }

    public void Update(AI_Fighter fighter)
    {
        // Check for enemies in sight
        var levelController = (LevelController)LevelControllerBase.Instance;
        if (levelController.checkSight(fighter, 45f))
        {
            fighter.stateMachine.ChangeState("ATTACK");
            return;
        }

        fighter.CheckBreak();
    }

    public void Exit(AI_Fighter fighter) { }
    public string GetStateName() => "PATROL";
}