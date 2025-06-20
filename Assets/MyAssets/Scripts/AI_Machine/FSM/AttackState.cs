

// Attack State - handles combat behavior
using UnityEngine;

public class AttackState : IAIState
{
    public void Enter(AI_Fighter fighter)
    {
        fighter.patternStage = "SETUP";
        fighter.targetSpeed = 1;
        fighter.patternTime = Time.time;
        fighter.patternDuration = fighter.varValue(fighter.attackSetupDuration.GetRandom(), 0.5f);
        fighter.formationNumber = -1;
    }

    public void Update(AI_Fighter fighter)
    {
        // Handle attack pattern stages
        if (fighter.patternStage == "ATTACK" && fighter.target != null)
        {
            HandleAttackStage(fighter);
        }
        else if (fighter.patternStage == "ATTACK" && fighter.target == null)
        {
            fighter.CheckSituation();
        }
        else if (fighter.patternStage == "PAUSE")
        {
            HandlePauseStage(fighter);
        }
        else if (fighter.patternStage == "SETUP")
        {
            HandleSetupStage(fighter);
        }
    }

    private void HandleAttackStage(AI_Fighter fighter)
    {
        if ((fighter.transform.position - fighter.target.transform.position).magnitude < 10)
        {
            // Break away
            fighter.DoTaticalManuver(out fighter.returnRotation, out fighter.aircraftYaw);
            fighter.patternStage = "SETUP";
            fighter.targetSpeed = 1;
            fighter.patternTime = Time.time;
            fighter.patternDuration = fighter.varValue(fighter.attackSetupDuration.GetRandom(), 0.5f);
            fighter.CheckSituation();
        }
        else
        {
            fighter.SeekTarget(out fighter.returnRotation, out fighter.aircraftYaw);

            if (fighter.bIsFiring && !fighter.bWasFiring)
            {
                fighter.patternTime = Time.time;
                fighter.patternDuration = 2;
            }

            fighter.bWasFiring = fighter.bIsFiring;

            if (Time.time - fighter.patternTime > fighter.patternDuration)
            {
                fighter.patternStage = "PAUSE";
                fighter.patternTime = Time.time;
                fighter.patternDuration = fighter.varValue(0.7f, 0.5f); // attackBreakTime
            }
        }
    }

    private void HandlePauseStage(AI_Fighter fighter)
    {
        fighter.DoWeave(out fighter.returnRotation, out fighter.aircraftYaw, 10);
        if (Time.time - fighter.patternTime > fighter.patternDuration)
        {
            fighter.patternStage = "ATTACK";
            fighter.bWasFiring = false;
            fighter.patternDuration = fighter.evadeDuration.GetRandom();
        }
    }

    private void HandleSetupStage(AI_Fighter fighter)
    {
        fighter.FleeTarget(out fighter.returnRotation, out fighter.aircraftYaw);

        if (Time.time - fighter.patternTime > fighter.patternDuration)
        {
            fighter.patternStage = "ATTACK";
            fighter.targetSpeed = 1;
        }
    }

    public void Exit(AI_Fighter fighter)
    {
        // Cleanup when exiting attack state
    }

    public string GetStateName() => "ATTACK";
}