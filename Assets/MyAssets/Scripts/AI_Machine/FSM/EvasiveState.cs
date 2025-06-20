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
            float rndVal = Random.Range(0, 2);
            if (rndVal < 1)
                fighter.patternStage = "LOOP";
            else
                fighter.patternStage = "WEAVE";
        }

        if (fighter.patternStage == "LOOP")
        {
            fighter.DoLoop(out fighter.returnRotation, out fighter.aircraftYaw, 20);
            fighter.targetSpeed = 1;
            CheckBreak(fighter);
        }
        else if (fighter.patternStage == "WEAVE")
        {
            fighter.DoWeave(out fighter.returnRotation, out fighter.aircraftYaw, 20);
            fighter.targetSpeed = 0.75f + Mathf.Sin(Time.time) * 0.25f;
            CheckBreak(fighter);
        }
    }

    private void CheckBreak(AI_Fighter fighter)
    {
        if (Time.time - fighter.patternTime > fighter.patternDuration)
        {
            fighter.stateMachine.ChangeState("ATTACK");
        }
    }

    public void Exit(AI_Fighter fighter) { }
    public string GetStateName() => "DOEVASIVE";
}