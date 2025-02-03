using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Base class for the state machine. I'm actually really happy with how it was setup for the AI/Movement in Chasing Dawn so will be looking to do something like that
//I want to drop this script as a subcomponent to the AI system in the model setup to have the ability to assemble different behaviors
//based off of attached AI components
public class AI_State : MonoBehaviour {
	public AI_Fighter baseAI = null;
	//The old system had patterns and sub-patterns for behaviours. I'll need to think deeply about how these should be implemented in the FSM
	public string pattern ="";
	public string patternStage = "";
	//This is used in our escape checking stuff
	public float patternTime = 0;
	public float patternDuration = 0;

	public Quaternion targetRotation, returnRotation;
	public float aircraftYaw;
	public float targetSpeed = 1;
	public virtual bool enter() {	//We won't always meet the conditions to enter this state
		return true;
	}

	public virtual bool exit() {	//We won't always meet the conditions to exit this state
		return true;
	}


	public virtual bool interruptexit() { //Interrupt exit is for when something drastic happens that doesn't necessariarally have an optional
		return true;
	}


	//Our state update tick
	public virtual void DoUpdate(out Quaternion newRotation, out float newYaw)
	{
		newRotation = Quaternion.identity;
		newYaw = 1;
    }

	public virtual void DoLateUpdate()
    {

    }

	public virtual void CheckTransition()
    {

    }

	public virtual void OnTakeDamage(GameObject instigator)
    {

    }
}
