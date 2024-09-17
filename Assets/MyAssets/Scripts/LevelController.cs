using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class actorWrapper {
	public GameObject vehicle;
	public Actor actor;
	public ActorController ourController; //good for known reference, but we almost need a type checking here
	public int targetValue; //this is to be used for various different things 
	public int team; //We shouldn't need this, but anyway.
	public GameObject radarObject; //should be assigned by the radar system
	public RadarItem radarLink;
	public bool bActorDead = false;
	
	//Finally are the extra bits and pieces our system uses
	public GameObject targetListPrefab; //a model displayed on our target list
}

//small wrapperish class that acts to handle the players statistics
[System.Serializable]
public class playerStats {
	public int kills=0; //how many vehicles have we destroyed in this level?
	public int murders=0;
	//do we want to record the shots etc? probably should
	public int shots=0;
	public int hits=0;

}

[System.Serializable]
public class waypointWrapper {
	public GameObject waypoint;
	public float checkDistance; //distance from the end goal where we activate.
	public float sqrCheckDistance; //used for checking...man that's a stupid little annotation really
	public waypoint wayScript;
}

public class LevelController : MonoBehaviour {

	public enum enEndMethod { ALL, WAYPOINTS, ENEMIES, TIME } //different mission end conditions...not sure how these will "evolve" if you will
	public enEndMethod endMethod = enEndMethod.ALL;

	public float enemyFactor=0.5F; //what our enemies are skill wise.
	private static LevelController instance = null;
	public static LevelController Instance {get {return instance;}}

	public MissionEventsManager ourMissionEventsManager;

	public CameraController levelCameraController;
	
	public GUIText ourGUIText;
	
	public GameObject playerAircraft; //Usually we'd
	//public GameObject enemyPrefab, friendlyPrefab;
	public int friendlyCount=3, enemyCount=5;
	
	public playerStats levelPlayerStats; //our stats for this level

	#region levelStuff
	public GameObject waypointPrefab;
	public GameObject enemySpawnNodePrefab; //this is a little touchy I suppose, will need development here

	public waypointWrapper[] waypoints; //we can make this active so that the targeting system could always see it?
	public int currentWaypoint=0; //because 0 is always the level start

	public enum enPlayState { Setup, Playing, Finished }
	public enPlayState playState = enPlayState.Setup;
	
	#endregion
	//These need to be changed into lists so that we can start handling our target changes/additions/removals
	//public GameObject[] enemyList, friendlyList;
	
	public List<actorWrapper> enemyList, friendlyList; //list of all our fighters in the scene
	
	public float score = 0;
	
	public Player_Radar targetRadar;
	
	#region waypointStuff

	GameObject missionEndGoal;

	public GameObject getMissionEndGoal {
		get { return missionEndGoal; }
	}

	bool bWaypointsComplete=false;						//Are the waypoints completed for running?
	bool bEndGoalComplete = false; 						//Have we got to the waypoint at the end of the level?

	Vector2 endGoal2D = Vector2.zero;

	public void populateWaypoints(GameObject[] theseWaypoint, GameObject endGoal) {

		missionEndGoal = endGoal; //is more of a formality actually

		endGoal2D = new Vector2(endGoal.transform.position[0], endGoal.transform.position[2]);

		//Can you assign an array to an array or does it have to be populated?
		waypoints = new waypointWrapper[theseWaypoint.Length];

		NGUI_Base.Instance.assignWaypoint(missionEndGoal);

		//populate our array
		for (int i=0; i< theseWaypoint.Length; i++) {
			waypointWrapper newWaypoint = new waypointWrapper(); //erp?
			
			newWaypoint.waypoint = theseWaypoint[i];
			newWaypoint.wayScript = theseWaypoint[i].GetComponent<waypoint>();
			newWaypoint.checkDistance = newWaypoint.wayScript.checkDistance;
			newWaypoint.sqrCheckDistance = newWaypoint.checkDistance*newWaypoint.checkDistance; //the actual bones of the matter
			/*
			if (i==currentWaypoint) {
				newWaypoint.wayScript.waypointActive =true; //Set this active as our next point
				NGUI_Base.Instance.assignWaypoint(newWaypoint);
			}
			else {
			*/
			newWaypoint.wayScript.waypointActive=false;
			//}
			
			waypoints[i] = newWaypoint;
		}

		//and don't forget to ping our first waypoint in case it requires that.
		waypoints[0].wayScript.orderCallWaypoint();
	}

	//called back after triggering a waypoint condition. Won't be used anymore...
	/*
	public void waypointCallback() {
		
		waypoints[currentWaypoint].wayScript.waypointActive=false; //turn the prior one off
		Debug.Log ("Moving to next waypoint: " + currentWaypoint);
		currentWaypoint++;
		if (currentWaypoint >= waypoints.Length) { //that should be level up in the most part, will need to make sure the enemies are mopped up
			Debug.LogError("Waypont Condition Complete");
		}
		else {
			waypoints[currentWaypoint].wayScript.setupWaypoint();
			waypoints[currentWaypoint].wayScript.waypointActive=true; //turn this one on.
			NGUI_Base.Instance.assignWaypoint(waypoints[currentWaypoint]); //update the GUI for this
		}
		
	}
	*/

	//if one aircraft wants to send a message to all fighters in the flight group (for whatever reason)
	public void notifyWithTag(int thisTeam, string thisMessage, string thisTag) {
		Debug.Log("Aircraft Sent Message. Team: " + thisTeam + " Message: " + thisMessage + " Tag: " + thisTag);
		if (thisTag=="NONE")
			return; //this is a null call

		if (thisTeam==0) {
			foreach(actorWrapper thisActor in friendlyList) {
				if (thisActor.ourController.flightGroup == thisTag || thisTag == "ALL") {
					thisActor.ourController.getNotification(thisMessage, thisTag);
				}
			}
		}
		else if (thisTeam==1) {
			foreach(actorWrapper thisActor in enemyList) {
				if (thisActor.ourController.flightGroup == thisTag || thisTag == "ALL") {
					thisActor.ourController.getNotification(thisMessage, thisTag);
				}
			}
		}
	}

	public int getFormationNumber(int team, GameObject followTarget) {
		if (team == 0) {
			return formationNumber (followTarget, friendlyList);
		} else if (team==1) {
			return formationNumber (followTarget, enemyList);
		}

		return -1;
	}

	int formationNumber(GameObject followTarget, List<actorWrapper> activeList) {
		int wings = 0;

		if (activeList == null || activeList.Count ==0) {
			return 0;
		}

		for (int i=0; i<activeList.Count; i++) {
			//there's always the possibility that this might not cast to a fighterAI...
			if (activeList[i].ourController is AI_Fighter) {
				AI_Fighter fighterAI = (AI_Fighter)activeList[i].ourController;

				//we almost need a cast down and back up to make sure we always get something here...
				if (fighterAI.followTarg == followTarget && fighterAI.pattern=="FOLLOW") {
					wings ++;
				}

			}
		}

		return wings;
	}

	//called when we've hit an enemy trigger of sorts
	//Will have other details like: where they're coming from
	public actorWrapper addFighterActor(GameObject thisFighter, int thisTeam, Vector3 thisLocation, Quaternion thisRotation, string groupTag, MissionEventObject thisOwner) {
		//Do something to spawn some enemies around our player
		//If this is an ambush:

		return addActor(thisTeam, thisFighter, thisLocation, thisRotation, groupTag, thisOwner);
		/*
		for (int i=0; i< enemyCount; i++) {
			
			float angle = Random.Range(0, 360);
			float distance = Random.Range (60, 150);
			Vector3 spawnOffset = new Vector3(Mathf.Sin (angle*Mathf.Deg2Rad) * distance, Random.Range (-40, 40), Mathf.Cos (angle*Mathf.Deg2Rad)*distance);
			
			addActor(1, prefabManager.Instance.getEnemyFighter(0F, 1F), playerAircraft.transform.position+spawnOffset, Quaternion.identity);
			
		}
		*/
		
	}
	
	#endregion

	#region scoreKeeping
	public void addKill() {
		levelPlayerStats.kills++;
	}

	public void addMurder() {
		levelPlayerStats.murders++;
	}


	#endregion

	public void setRadar(Player_Radar thisRadar) {
		targetRadar = thisRadar;	
	}
	
	void Awake() {
		if (instance)
		{
			Debug.Log("Duplicate attempt to create LevelController");
			Destroy(this);
			return;
		}
		
		instance = this;
	}

	//this used by the AI to see if its spotted anything
	public bool checkSight(ActorController thisController, float FOVAngle) {
		if (thisController.team == 0)
			return checkSighted(thisController, enemyList, FOVAngle);
		else if (thisController.team==1)
			return checkSighted(thisController, friendlyList, FOVAngle); //least I think that's how it'll work

		return false;
	}

	bool checkSighted(ActorController thisController, List<actorWrapper> targetList, float FOVAngle) {

		Vector3 ourPosition = thisController.ourAircraft.gameObject.transform.position;
		Vector3 ourForward = thisController.ourAircraft.gameObject.transform.forward; //ouch...that's long

		foreach (actorWrapper thisActor in targetList) {
			if (Vector3.Angle(ourForward, thisActor.vehicle.transform.position - ourPosition) < FOVAngle)
				return true; //so we can technically see something here
		}

		return false; //nothing sighted
	}
	
	
	//The fighters can use this to call for another target
	public void requestTarget(ActorController ourController) {
		//For the moment just assign it something.
		int target=-1;
		if (ourController.team==0) { //we're friendly, and a small tweak for testing...
			target = GetBestTarget(ourController, enemyList);
			if (target!=-1)
				ourController.targetCallback(enemyList[target].actor, enemyList[target].vehicle, 1); //list this target and attack it!
			else
				ourController.targetCallback(null, null, -1);
		}
		else if (ourController.team==1) { //we're friendly
			target = GetBestTarget(ourController, friendlyList);
			if (target!=-1)
				ourController.targetCallback(friendlyList[target].actor, friendlyList[target].vehicle, 1); //list this target and attack it!
			else
				ourController.targetCallback(null, null, -1);
		}
		//We've had our AI fighter request a target. First up is that we need to know which list we're drawing from
		else {
			ourController.targetCallback(null, null, -1);
		}
	}

	//this just selects a target that's +direction from our current one.
	//this is only used for our player . This seems to be working fine now :)
	public void requestListTarget(int direction, ActorController ourController) {
		//Debug.Log ("RequestListTarget");
		for (int i=0; i<enemyList.Count; i++) {
			if (ourController.target==enemyList[i].vehicle) { //add one to this
				int targetNumber = (int)Mathf.Repeat(i+direction, enemyList.Count);
				//Debug.Log ("TargetNumber: " + targetNumber);
				ourController.targetCallback(enemyList[targetNumber].actor, enemyList[targetNumber].vehicle, 1);
				break; //cancel this so that it doesn't "count out"
			}
		}
	}

	//can also be used by the player I suppose. For now this will work.
	int GetBestTarget(ActorController thisController, List<actorWrapper> listTargets) {
		int bestTarget = -1;
		float bestPropensity = float.MaxValue, thisTargetPropensity=0; //we use an inverse system

		if (listTargets.Count != 0) { //This just needs to return an int...cunning...oh no wait...I wrote this of course!
			for (int i=0; i<listTargets.Count; i++) {
				thisTargetPropensity = 0; //annul this before recalculating
				//first step: how far away from us is it?
				thisTargetPropensity += (thisController.ourAircraft.gameObject.transform.position-listTargets[i].actor.gameObject.transform.position).magnitude;
			
				//So now consider our direction to the target (is this the correct way around?)
				//Graduate a bit. Down by a quater so that it won't be at a 180m range for an override
				thisTargetPropensity += (Vector3.Angle(thisController.ourAircraft.gameObject.transform.forward, (listTargets[i].actor.gameObject.transform.position-thisController.ourAircraft.gameObject.transform.position)))/4F;

				//Do we want to do something with the health of the target?

				//we don't want to be targeting things that are shot down (at least with the player anyway)
				if (listTargets[i].actor.bIsDead) //don't target this
					thisTargetPropensity = float.MaxValue;

				//Debug.Log ("Target Angle: " + Vector3.Angle(thisController.ourAircraft.gameObject.transform.forward, (listTargets[i].actor.gameObject.transform.position-thisController.ourAircraft.gameObject.transform.position)));

				if (thisTargetPropensity < bestPropensity) { //pick this target
					bestPropensity = thisTargetPropensity;
					bestTarget = i;
				}
			
			}
		}
		
		return bestTarget;
	}

	public void removeSelf(GameObject thisActor, int thisTeam) {
		if (thisTeam == 0) { //Pull us out of the list
			for (int i=0; i<friendlyList.Count; i++) { //Need another method here
				if (friendlyList[i].vehicle == thisActor) { //remove this entry
					if (friendlyList[i].radarObject)
						Destroy (friendlyList[i].radarObject);

					friendlyList.RemoveAt(i); //that way we'll get the correct one
					
				}
			}
		}
		else if (thisTeam ==1) {
			for (int i=0; i<enemyList.Count; i++) {
				if (enemyList[i].vehicle == thisActor) { //remove this entry
					if (enemyList[i].radarObject)
						Destroy (enemyList[i].radarObject);

					enemyList.RemoveAt(i); //that way we'll get the correct one
					
				}
			}
		}
	}
	
	//For the moment
	//We need to do the fighter class stuff somehows
	public actorWrapper addActor(int thisTeam, GameObject thisPrefab, Vector3 thisPosition, Quaternion thisRotation, string groupTag, MissionEventObject thisOwner) {

		GameObject newTarget = Instantiate(thisPrefab, thisPosition, thisRotation) as GameObject;
			
		actorWrapper newActor;
		
		//Assign our stuff
		newActor = new actorWrapper();
		newActor.vehicle = newTarget;
		newActor.actor = newTarget.GetComponent<Actor>(); //Was aircraft controller but we don't need to be that high up the stack really
		//newActor.vehicle = newActor.actor.getModel();
		newActor.team = thisTeam;
		newActor.actor.owner = thisOwner;
		//not totally sure this is the best option
		newActor.ourController = newTarget.GetComponentInChildren<ActorController>(); //might cause issues with AI guns...
		newActor.ourController.flightGroup = groupTag;

		//Debug.Log ("\tGroupTag: " + ((AI_Fighter)newActor.ourController).flightGroup + " | " + groupTag);


		newActor.ourController.team = thisTeam;
		newActor.actor.setTeam(thisTeam);
		//Need to assign the variables to the aircraft controller
		
		//Handle our radar stuff
		newActor.radarObject = Instantiate(newActor.actor.ourRadarPrefab) as GameObject; //Put down our radar object
		newActor.radarObject.transform.parent = targetRadar.gameObject.transform; //child it to this.
		newActor.radarObject.transform.localScale = Vector3.one;
		newActor.radarLink = newActor.radarObject.GetComponent<RadarItem>();

		newActor.ourController.setPatrol(Random.Range(30, 35)); //set everything here on patrol

		//Now we need to figure out which list we add it to
		if (thisTeam==0) {
			friendlyList.Add(newActor);
		}
		else if (thisTeam==1) {
			enemyList.Add(newActor);
			newActor.actor.gradeSkill(enemyFactor);

		}

		//return newTarget; //for those of us who need the vehicle
		return newActor;
	}
	
	void Start() {
		//For the moment:
		//StartCoroutine(StartMatch());
		
	}
	
	//This needs another number to gauge the difficulity
	public void createMatch(int enemies, int wingmen) { //and details go here!
														//will need to call the LevelBuilder_ that we're using for this match when it's all in pace, not this system...
														//StartCoroutine(StartMatch(enemies, wingmen));
														//Lets use this for the moment
		ourMissionEventsManager.StartMission(); //precanned hardwired setup

	}

	public void finishMatch()
    {
		//Of course we need some concept of how well this has gone
		StartCoroutine(FinishLevel());
    }

	IEnumerator FinishLevel()
    {
		yield return new WaitForSeconds(3f);    //Give a little pause after it's complete
		gameManager.Instance.ConcludeMission();
    }

	// Use this to make a game
	IEnumerator StartMatch (int enemies, int wingmen) {
		
		//this will need a better setup, but for the moment
		friendlyCount=wingmen;
		enemyCount=enemies;
		
		ourGUIText.text = "SCORE: " + 0F;
		
		//if (enemyList == null || enemyList.Length < 3) {
		
		//Wait for our game setup
		while (prefabManager.Instance == null) //There'll be more to add to this
			yield return null; //don't start until we've got a player.
		
		//We need to add a player. Not sure about positional stuff yet.
		playerAircraft = Instantiate(prefabManager.Instance.returnPlayerPrefab()) as GameObject;
		
		playerAircraft.transform.position = new Vector3(0, 60, 0); //for now that'll work.
		
		//link the camera to the player
		levelCameraController.ownerAircraft = playerAircraft;
		levelCameraController.viewCenter = playerAircraft.GetComponent<Actor>().viewCenter;
		
		
		//Add said player.
		actorWrapper newActor = new actorWrapper();
		newActor.vehicle = playerAircraft;
		newActor.actor = playerAircraft.GetComponent<AircraftController>();
		//newActor.vehicle = newActor.actor.getModel();

		newActor.team = 0;
		//newActor.radarObject = newActor.actor.ourRadarPrefab; //Put down our radar object
		
		newActor.ourController = playerAircraft.GetComponentInChildren<ActorController>();
		
		friendlyList.Add (newActor);
		
		/*
		for (int i=0; i<friendlyCount; i++) {
			addActor(0, prefabManager.Instance.getFriendlyFighter(0F, 1F));
		}
		for (int i=0; i<enemyCount; i++) {
			addActor(1, prefabManager.Instance.getEnemyFighter(0F, 1F));
		}
		*/
		
	}
	
	// Update is called once per frame
	void Update () {
		//monitorWaypoints(); //shouldn't be called until it's all setup properly

		//checkLevelClear();
	}

	void checkLevelClear() {
		if (endMethod == enEndMethod.ALL) { //Down all enemies, clear all waypoints, reach final waypoint
			if (bWaypointsComplete && enemyList.Count == 0 && bEndGoalComplete) {
				//Debug.Log ("Level Complete");
			}
		}
	}

	//used to check and see if we should be turning waypoints on or off. Naturally they're all sequencial
	void monitorWaypoints() {
		bWaypointsComplete = true; //trigger this off if one isn't condition true.


		//our distances. Shame there's not a fast flat magnitude function as I don't want to be making heaps of Vector2s all the time...
		float playerSquare = (endGoal2D - new Vector2(PlayerController.Instance.ourAircraft.transform.position[0], PlayerController.Instance.ourAircraft.transform.position[2])).sqrMagnitude;

		if (playerSquare < 60) {
			bEndGoalComplete = true;
		} else {
			bEndGoalComplete = false;
		}

		//these should be in order of no particular order actually...
		foreach (waypointWrapper thisWaypoint in waypoints) {
			if (thisWaypoint.wayScript.waypointState != waypoint.enWaypointState.Done) {
				bWaypointsComplete=false;
			}

			if (playerSquare < thisWaypoint.sqrCheckDistance && thisWaypoint.wayScript.waypointState == waypoint.enWaypointState.Uncalled) { //this waypoint should be triggered
				//thisWaypoint.wayScript.waypointState = waypoint.enWaypointState.Done;

				thisWaypoint.wayScript.callWaypoint(playerSquare); //call this waypoint into action!

				Debug.Log ("triggered waypoint: " + thisWaypoint.sqrCheckDistance);
			}
		}
	}

	//called when a waypoint thinks that it's complete, allowing us to move on or similar.
	public void waypointCallback(GameObject thisWaypoint) {
		//check to see if the waypoint ahead of this one needs to be activated (ie a goal or similar)
		for (int i=0; i<waypoints.Length; i++) {
			if (thisWaypoint == waypoints[i].waypoint) {
				//Debug.Log ("Completed Waypoint: " + i);
				//Depending on the type of waypoint we should ping through the option to become active and have it handle
				//things accordingly :)

				if (i+1 < waypoints.Length) { //we've got something to call forward to...
					waypoints[i+1].wayScript.orderCallWaypoint(); //just in case we're setup to have something that'll use this...
				}
			}
		}
	}
}
