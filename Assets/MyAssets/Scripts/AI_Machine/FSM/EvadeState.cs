using UnityEngine;

// Evade State - handles evasive maneuvers
public class EvadeState : IAIState
{
    public void Enter(AI_Fighter fighter)
    {
        fighter.patternTime = Time.time;
        fighter.patternDuration = fighter.evadeDuration.GetRandom();
    }

    public void Update(AI_Fighter fighter)
    {
        fighter.DoTaticalManuver(out fighter.returnRotation, out fighter.aircraftYaw);

        if (fighter.target == null)
        {
            fighter.stateMachine.ChangeState("ATTACK");
            return;
        }

        if (Time.time - fighter.patternTime > fighter.patternDuration)
        {
            if ((fighter.transform.position - fighter.target.transform.position).magnitude < 10)
            {
                fighter.stateMachine.ChangeState("DOEVASIVE");
            }
            else
            {
                fighter.stateMachine.ChangeState("ATTACK");
            }
        }
    }

    public void Exit(AI_Fighter fighter) { }
    public string GetStateName() => "EVADE";
}