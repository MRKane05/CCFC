using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//AKA "Flee State"
public class AI_SetupState : AI_State
{

	public override bool enter()
	{
		patternTime = Time.time;
		patternDuration = baseAI.attackSetupDuration.GetRandom();
		return base.enter();
	}

	public override void DoUpdate(out Quaternion newRotation, out float newYaw)
	{
		//===========Target Seeking Functions...in reverse...======================
		//PROBLEM: This AI function breaks during tailgunner missions
		if (baseAI.target)
		{
			transform.LookAt(baseAI.target.transform.position, Vector3.up); //we can have our GameObject do this as it's a child of the aircraft
																	 //And the opposite direction to this isn't the inverse...
			transform.eulerAngles += new Vector3(0, 180, 0);
		}
		//Figure out what our turn angles should be doing...
		newYaw = 1.0f;
		if (transform.localEulerAngles[1] > 180)
			newYaw = -1F;


		newYaw *= baseAI.aircraftRollCurve.Evaluate(Mathf.Abs(transform.localEulerAngles[1]));
		newRotation = transform.rotation;
		//ourAircraft.AIUpdateInput(transform.rotation, newYaw);
	}

	public override void CheckTransition()
	{
		base.CheckTransition();


		if (Time.time - patternTime > patternDuration)
		{
			//Go into attack state
		}
	}
}
