using UnityEngine;
using System.Collections;

public class LevelBuilder_Patrol : LevelBuilder_Base {
	
	//waypoit details for setting up the mission.

	public float eventFrequency=0.2f; //what's the frequency of getting a random event?ß

	Vector2 waypointAngle = new Vector2(50, 140); //what is the angle between each waypoint path?
	Vector2 waypointDistance = new Vector2(60, 120); //our distance range between each waypoint.
	Vector2 waypointNumRange = new Vector2(4, 6); //a bit of random I suppose
	
	GameObject[] newWaypoints;
	GameObject[] enemyNodes;
	
	void Start() {
		//setupLevel();
	}

	//will need some sort of difficulty measure passed through to this...
	public override void setupLevel() {
		//this needs to bring up the events that will be happening in this level and then arrange how they'll happen with
		//the player going

		//so really we're breaking down our events into "waypoints of interest" I guess, although they won't be waypoints as such
		//...might need to make a few more of these (like the intercept fighters waypoint)

		/*
		int levelEvents=2; //we're only after about 4 different things of interest that'll happen here

		float missionDist = 200f; //this is a flat measure now.

		Vector3 playerLoc = PlayerController.Instance.ourAircraft.transform.position;

		Vector3 waypointPosition = playerLoc + PlayerController.Instance.ourAircraft.transform.rotation*Vector3.forward*missionDist; //mission distance...


		GameObject endGoal = Instantiate(LevelController.Instance.waypointPrefab, waypointPosition, Quaternion.identity) as GameObject;

		//really we need to do a random draw here...

		newWaypoints = new GameObject[levelEvents];

		for (int i = 0; i < levelEvents; i++) {
			float randomDraw = Random.Range(-0.4f/(levelEvents), 0.4f/(levelEvents)); //Random.Range (-1f/(0.5f*(levelEvents+1)),1f/(0.5f*(levelEvents+1))); //use this to calculate the distance stuff

			//Zero isn't an option I'm afraid...
			float goalDist = Mathf.Clamp((float)(i+1)/(float)(levelEvents) + randomDraw, 0.05f, 0.9f); //Mathf.Clamp01(((float)(i)/(float)(levelEvents) + randomDraw)); // randomDraw*missionDist; //distance to our end goal or distance along the leg if you will.

			//this was then meant to send everything through to the main system, and we might be getting a little bit of double handling with the script stuff here...anyway...
			//waypointPosition = Vector3.Lerp (endGoal.transform.position, playerLoc, goalDist); //meaningless positional stuff...
			waypointPosition = endGoal.transform.position - Vector3.forward*goalDist*missionDist;

			//this should be something from some sort of "mission event class handler" which automatically draws things up for the player.
			newWaypoints[levelEvents-i-1] = Instantiate(waypointManager.Instance.requestWayGoal(0f), waypointPosition, Quaternion.identity) as GameObject;

			waypoint thisWaypoint = newWaypoints[levelEvents-i-1].GetComponent<waypoint>();

			thisWaypoint.checkDistance = goalDist * missionDist; //how far until we trigger.

		}

		LevelController.Instance.populateWaypoints(newWaypoints, endGoal);
		*/
		//can we add a wingman through this also.
		addWingman(0, prefabManager.Instance.getFriendlyFighter(0F, 1F), PlayerController.Instance.ourAircraft.transform.position + Vector3.back*5f + Vector3.right * 5f, Quaternion.identity, "PLAYER", 0);
		addWingman(0, prefabManager.Instance.getFriendlyFighter(0F, 1F), PlayerController.Instance.ourAircraft.transform.position + Vector3.back*5f - Vector3.right * 5f, Quaternion.identity, "PLAYER", 1);


	}

	//this should be in the base class of things
	public actorWrapper addWingman(int thisTeam, GameObject thisPrefab, Vector3 thisPosition, Quaternion thisRotation, string groupTag, int formationPosition) {
		
		GameObject newTarget = Instantiate(thisPrefab, thisPosition, thisRotation) as GameObject;
		
		actorWrapper newActor;
		
		//Assign our stuff
		newActor = new actorWrapper();
		newActor.vehicle = newTarget;
		newActor.actor = newTarget.GetComponent<Actor>(); //Was aircraft controller but we don't need to be that high up the stack really
		//newActor.vehicle = newActor.actor.getModel();
		newActor.team = thisTeam;
		
		//not totally sure this is the best option
		newActor.ourController = newTarget.GetComponentInChildren<ActorController>(); //might cause issues with AI guns...
		//Optional link pulled from the AircraftController perhaps?
		
		newActor.ourController.team = thisTeam;
		newActor.actor.setTeam(thisTeam);
		//Need to assign the variables to the aircraft controller
		
		//Handle our radar stuff
		newActor.radarObject = Instantiate(newActor.actor.ourRadarPrefab) as GameObject; //Put down our radar object
		newActor.radarObject.transform.parent = LevelController.Instance.targetRadar.gameObject.transform; //child it to this.
		newActor.radarObject.transform.localScale = Vector3.one;
		newActor.radarLink = newActor.radarObject.GetComponent<RadarItem>();


		((AI_Fighter)newActor.ourController).formationNumber = formationPosition; // LevelController.Instance.getFormationNumber(thisTeam, PlayerController.Instance.ourAircraft.gameObject);
		//newActor.ourController.setPatrol(Random.Range(30, 35)); //set everything here on patrol
		((AI_Fighter)newActor.ourController).pattern = "FOLLOW";

		((AI_Fighter)newActor.ourController).followTarg = PlayerController.Instance.ourAircraft.gameObject; //follow this
		((AI_Fighter)newActor.ourController).flightGroup = groupTag;

		//Now we need to figure out which list we add it to
		if (thisTeam==0) {
			LevelController.Instance.friendlyList.Add(newActor);
		}
		else if (thisTeam==1) {
			LevelController.Instance.enemyList.Add(newActor);
			newActor.actor.gradeSkill(0.5f);
			
		}

		//return newTarget; //for those of us who need the vehicle
		return newActor;
	}
	
	//we need a few levels here as to what is the nature of this level setup.
	public void OldsetupLevel() {
		//Sets out the stuff required for the level to "function"


		//For this class we need to outline some patrol points and some points for enemies to be added to the match
		
		newWaypoints = new GameObject[5];
		//Ok, our path first:
		//Set the direction of the players aircraft
		
		//do some basic stuff to figure out what the pathnodes are.
		Vector3 waypointPosition = new Vector3(0, 150, 0);
		
		Quaternion waypointRotation = Quaternion.identity;

		//somewhere in this system we'll have to make sure that we're not putting waypoints in the ground
		
		GameObject lastWaypoint = Instantiate(LevelController.Instance.waypointPrefab, waypointPosition, waypointRotation) as GameObject;
		//Player aircraft for the moment starts at 0, 150, 0 with Quaternion.Identity rotation
		for (int i=0; i< 5; i++) {
			waypointPosition += lastWaypoint.transform.forward*Random.Range(waypointDistance[0], waypointDistance[1]);
			waypointRotation.eulerAngles += new Vector3(0, Random.Range(-Random.Range(waypointAngle[0], waypointAngle[1]), Random.Range (waypointAngle[0], waypointAngle[1])), 0);
			lastWaypoint = Instantiate(LevelController.Instance.waypointPrefab, waypointPosition, waypointRotation) as GameObject;
			lastWaypoint.name = "Waypoint" + i;

			waypoint waypointController = lastWaypoint.GetComponent<waypoint>();

			//look at what this waypoint does (as now we've got the option of it being an interest target)
			if (Random.value < eventFrequency && i!=0) {//set this to an interest point
				waypointController.triggerType = waypoint.enTriggerType.Target; //so this will become a target when it's elected
			}
			else {
				//waypointController.triggerType = waypoint.enTriggerType.Check;
			}

			newWaypoints[i] = lastWaypoint; //fill this in...although these are only gameobjects and aren't actual waypoint prefabs
		}
		
		LevelController.Instance.populateWaypoints(newWaypoints, newWaypoints[4]);

		//need to go through here and populate our instances of enemy nodes.

		//so if we're doing 5 waypoints, lets have 3 enemy nodes distributed somehow through the system.
		//these should be spread between waypoints (as in graduated according to the vector between them etc)

		enemyNodes = new GameObject[4];
		int placedEnemyNodes = 0;

		//right method 3. pull a random number and place the node along the path according to how our
		//progression is. Could make the difficulty spike, but I think it's game.

		//this is a little unreliable
		//for (int i=0; i<enemyNodes.Length; i++) {
		bool bValidNode=true;

		//we can sometimes lock up here. Need to put a count system through to it'll dump down after some tries
		int cycles=0;
		while (placedEnemyNodes<4) {
			bValidNode=true; //set this true to begin so that we may use rules to disable this as necessary.
			cycles++; //incriment our cycle method

			float nodePosition = Random.Range (0F, (float)(newWaypoints.Length)); //used to get our random.

			int nodeBase = Mathf.FloorToInt(nodePosition); //drop back and clean out any remainer...
			float nodeDistance = nodePosition-nodeBase; //Mathf.Clamp((nodePosition-nodeBase), 0.2F, 0.8F); //is a clamp necessary?
			Vector3 nodeLocation = Vector3.zero;

			//need to make sure we don't index out of range with a freak value
			if (nodeBase<newWaypoints.Length-1) { //make sure we're not putting down a node at the very end
				nodeLocation = Vector3.Lerp (newWaypoints[nodeBase].transform.position, newWaypoints[nodeBase+1].transform.position, nodeDistance); //this is where the enemy node must go.

				//need to check that this node isn't too close to the others...
				if (placedEnemyNodes > 0) {
					for (int i=0; i<placedEnemyNodes; i++) {
						if ((enemyNodes[i].transform.position-nodeLocation).magnitude < 100) { //aribtary at the moment...
							//we want to skip this round
							bValidNode=false;
						}
					}
				}
			}
			else { //it's an invalid node
				bValidNode=false;
			}

			//this can time out and just drop a node regardless
			if (bValidNode || cycles > 8) { //this positional stuff has checked out OK...
				cycles=0; //reset our lock out counter
				//should add some visulisation to these for debugging purposes
				enemyNodes[placedEnemyNodes] = Instantiate(LevelController.Instance.enemySpawnNodePrefab, nodeLocation, Quaternion.identity) as GameObject;
				//we really need to pass this information through to the system how...?
				placedEnemyNodes++; //this is all fine. Cycle this on
		
			}
		}

	}
}
