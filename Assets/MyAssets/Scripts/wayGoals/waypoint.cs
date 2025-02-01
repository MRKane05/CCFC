using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//waypoint stuff
//This class itself will be expanded to control the basic behaviour of "enemy nodes" that spawn the enemies
//we encounter along our way - and the various behaviour that happens within
//Of course players could go around the long way and trigger them that way, but perhaps we'd better think of something
//to suss that one out

//We're modifying this to trigger as distance from the end goal to keep the player more "free" in their flight methods
public class waypoint : MonoBehaviour {
	//waypoint types:
	//standard (fly into area to clear)
	//intel collection waypoint (need to press a button going through)
	
	public enum enTriggerType { Check, Distance, Target, Timer, Goal } //What's our trigger here? Doesn't have to be a trigger, can also be an interest target
	public enTriggerType triggerType = enTriggerType.Check;

	public enum enCallType { Free, Forward } //Free: can be activated when the player hits the limit. Forward: needs to be activated after the last waypoint
	public enCallType callType = enCallType.Free;

	public enum enWaypointState { Uncalled, Active, Done } //our state command stuff, I don't think we need this many, but it never hurts to be forward thinking
	public enWaypointState waypointState = enWaypointState.Uncalled;

	public float checkDistance = 90F; //how far from the end goal will we trigger? Sent from the level controller

	public bool waypointActive = false;

	/*
	public bool bIsActive {
		get { return waypointActive; }
		set { waypointActive = value;
			if (value)
				setupWaypoint();
		}
	}
	*/


	float previousDist=float.MaxValue;
	
	//GameObject[] specialTargets; //targets we need to destroy to cycle out this waypoint

	//List<Actor> specialTargets = new List<Actor>(); //our special target list.

	actorWrapper[] specialTargets; //not sure that this is valid for this class only now

	//only really works for special behaviours
	public virtual void setupWaypoint() { 
		if (triggerType==enTriggerType.Target) { //add in some targets here, and make sure our lists are populated
			//public void addActor(int thisTeam, GameObject thisPrefab, Vector3 thisPosition, Quaternion thisRotation)
			//GameObject targetActor = Instantiate(prefabManager.Instance.getEnemyBalloon(0, 0), gameObject.transform.position, Quaternion.identity) as GameObject;
			//specialTargets = new List<Actor>();

			specialTargets = new actorWrapper [1];

			//specialTargets.Add (targetActor); //will still need to check and cycle through these.
			//add the interest target to our list
			//specialTargets[0] = ((LevelController)LevelControllerBase.Instance).addActor(1, prefabManager.Instance.getEnemyBalloon(0, 0), gameObject.transform.position, Quaternion.identity, "NONE").GetComponent<Actor>();
		
		}
	}

	// Update is called once per frame
	/*
	void LateUpdate () {
		//If this is active there are three things we need to do
		//turn on the GUI marker
		//Turn on the visual marker
		//Turn on the Radar Marker
		//Why is it never easy
		if (waypointActive && PlayerController.Instance!=null)
			checkWaypoint();
	}
	*/

	//called from the Level controller when the distance is less than the expected distance
	//This will be overwritten to contain our key behaviour in scripts extending this one (wayGoal_Script)
	public virtual void callWaypoint(float playerDist) { 

		waypointState = enWaypointState.Done; //for the best I suppose...

		//renderer.enabled = false; //turn this off so we know that it's been called

		//don't really need the player distance, but it can't hurt

		//this will then do the spawned behaviour...really this should just be triggering the waypoint actually.
		//if (triggerType==enTriggerType.Target) { //not perhaps quite...actually no.
		//this waypoint might either trigger a behaviour, or an "event"

		//I really need some sort of setup that can put down a prefab with an event happening.
		if (triggerType == enTriggerType.Goal) { //need to spawn the goal that we're pointing at. (it was "Event" but "Event" is a system callword. Silly computer language
			//but in essence won't every one of these be a goal of some sort? Perhaps we should just be having different calls for the different waypoint setups?


		}

		//and if it's not an event then it'll be an enemy node...bit of a tough one.

	}

	//the level controller has sent a call through to this waypoint for whatever reason.
	public virtual void orderCallWaypoint() { 

	}

	public virtual void triggerWaypoint() {
		//((LevelController)LevelControllerBase.Instance).waypointCallback(); //calls back to the level controller to say that we're done setting this up...
	}
}
