using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_EvadeState : AI_State {

    public override bool enter()
    {
		patternTime = Time.time;
		patternDuration = baseAI.escapeDuration.GetRandom();
		return base.enter();
    }

    public override void DoUpdate(out Quaternion returnRotation, out float aircraftYaw)
	{
        //base.DoUpdate(out Quaternion newRotation, out float newYaw);
		baseAI.DoTaticalManuver(out returnRotation, out aircraftYaw);

		if (patternStage == "PICK")
		{
			float rndVal = Random.Range(0, 2);
			if (rndVal < 1)
				patternStage = "LOOP";
			else if (rndVal < 2)
				patternStage = "WEAVE";

		}
		//things like loop, immulman, barrel roll etc.
		//Not really sure how I'll programme this in given the AI methods...
		//Even though the factor can be whatever we want, at the end of the day we're only as manuveriable as our aircraft
		//Measured functions
		//We've only got two sub-functions for handling evasive behaviour at this stage however
		if (patternStage == "LOOP")
		{
			baseAI.DoLoop(out returnRotation, out aircraftYaw, 20);
			targetSpeed = 1;
			//CheckBreak();
		}
		if (patternStage == "WEAVE")
		{
			baseAI.DoWeave(out returnRotation, out aircraftYaw, 20);
			//this is to be a little more entropic
			targetSpeed = 0.75F + Mathf.Sin(Time.time) * 0.25F; //vary our time, although this needs a better ticker
			//CheckBreak();
		}

		CheckTransition();	//See if we should be changing states
	}

    public override void CheckTransition()
    {
        base.CheckTransition();

		
		if (Time.time - patternTime > patternDuration)
		{
			//Need to check and see if we're in a state to attack or if we should do an evasive manuver...
			if ((transform.position - baseAI.target.transform.position).magnitude < 10)
			{ //do an evasive
				pattern = "DOEVASIVE";
				patternStage = "PICK";	//This is basically a check to see what sort of evasive behaviour we should be doing
				patternTime = Time.time;
				patternDuration = baseAI.escapeDuration.GetRandom();
			}
			else
			{
				pattern = "ATTACK";
				patternStage = "ATTACK";	//Change our state to attacking
				patternTime = Time.time;
				patternDuration = baseAI.attackDuration;    //Really this should be set when entering the state
			}
		}
	}
}
