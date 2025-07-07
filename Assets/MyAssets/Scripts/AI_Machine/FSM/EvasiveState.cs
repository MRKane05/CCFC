using UnityEngine;

// Evasive State - handles desperate evasive maneuvers
public class EvasiveState : IAIState
{
    public void Enter(AI_Fighter fighter)
    {
        fighter.patternStage = "PICK";
        fighter.patternTime = Time.time;
        fighter.patternDuration = fighter.escapeDuration.GetRandom();
    }

    public void Update(AI_Fighter fighter)
    {
        if (fighter.patternStage == "PICK")
        {
            float rndVal = Random.Range(0, 4);
            if (rndVal < 1)
                fighter.patternStage = "LOOP";
            else if (rndVal < 2)
                fighter.patternStage = "WEAVE";
            else if (rndVal < 3)
                fighter.patternStage = "TIGHTENINGTURN";
            else 
                fighter.patternStage = "SCISSOR";
        }

        switch (fighter.patternStage)
        {
            case "LOOP":
                fighter.DoLoop(out fighter.returnRotation, out fighter.aircraftYaw, 20);
                fighter.targetSpeed = 1;
                CheckBreak(fighter);
                break;
            case "WEAVE":
                fighter.DoWeave(out fighter.returnRotation, out fighter.aircraftYaw, 20);
                fighter.targetSpeed = 0.75f + Mathf.Sin(Time.time) * 0.25f;
                CheckBreak(fighter);
                break;
            case "TIGHTENINGTURN":
                fighter.DoTighteningTurn(out fighter.returnRotation, out fighter.aircraftYaw, 20f * ((Time.time - fighter.patternTime) / fighter.patternDuration));
                fighter.targetSpeed = 1;
                CheckBreak(fighter);
                break;
            case "SCISSOR":
                fighter.DoScissors(out fighter.returnRotation, out fighter.aircraftYaw);
                fighter.targetSpeed = 1;
                CheckBreak(fighter);
                break;
        }
    }

    private void CheckBreak(AI_Fighter fighter)
    {
        if (Time.time - fighter.patternTime > fighter.patternDuration)
        {
            fighter.stateMachine.ChangeState("ATTACK");
        }
        //We could also do with something that says "we're in an advantageous position over our enemy" for this break
        float targetDot = Vector3.Dot((fighter.ourAircraft.transform.position- fighter.target.transform.position).normalized, fighter.target.transform.forward);
        if (targetDot < -0.25)  //We're behind our target but still doing evasive manuvers. We should switch to attacking
        {
            fighter.stateMachine.ChangeState("ATTACK");
        }

    }

    public void Exit(AI_Fighter fighter) { }
    public string GetStateName() => "DOEVASIVE";
}