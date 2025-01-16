using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A sub-class to handle all the mission events and make sure the necessary calls are sent back and forth to everything
//and to unclutter the level controller

[System.Serializable]
public class LevelObjectiveSet
{
	public LevelObjectiveSet(enObjectiveType newObjective, float newDensity)
    {
		thisObjective = newObjective;
		density = newDensity;
    }
	public enum enObjectiveType { NONE, FIGHTERS, BALLOONS, BOMBERS, BASE }
	public enObjectiveType thisObjective;
	public float density = 3.0f;
}

public class MissionEventsManager : MonoBehaviour {

	private static MissionEventsManager instance = null;
	public static MissionEventsManager Instance { get { return instance; } }

	public List<missionEvent> missionEvents = new List<missionEvent>(); //This is what our mission events are going to be populated into
	public List<MissionEventObject> gameMissionEvents = new List<MissionEventObject>();
	public List<missionEvent> activeEvents = new List<missionEvent>();	//Events that the player can actively "complete" through an action such as destroying, photographing, etc.
	public List<missionEvent> passiveEvents = new List<missionEvent>(); //Events that the player cannot "complete" in a level and are intended to last the level duration

	public List<missionEvent> hiddenActiveEvents = new List<missionEvent>();  //Unlisted events that the player can actively "complete" through an action such as destroying, photographing, etc.
	public List<missionEvent> hiddenPassiveEvents = new List<missionEvent>(); //Unlisted events that the player cannot "complete" in a level and are intended to last the level duration


	float gameTime = 0;
	// Use this for initialization
	public void StartMission()
    {
		//Quickly knock something together
		
		List<LevelObjectiveSet> ListLevelObjectives = new List<LevelObjectiveSet>();
		LevelObjectiveSet newObjective = new LevelObjectiveSet(LevelObjectiveSet.enObjectiveType.FIGHTERS, 1);
		//LevelObjectiveSet balloonObjective = new LevelObjectiveSet(LevelObjectiveSet.enObjectiveType.BALLOONS, 1);
		ListLevelObjectives.Add(newObjective);
		//ListLevelObjectives.Add(balloonObjective);
		ConstructMission(ListLevelObjectives, 4);

		Debug.Log("Starting Mission");
		ProcessMissionEvents();

		//We also need to add our wingmen
		//AddWingmen();
    }

	public virtual void AddWingmen()
    {
		addWingman(0, prefabManager.Instance.getFriendlyFighter(0F, 1F), PlayerController.Instance.ourAircraft.transform.position + Vector3.back * 5f + Vector3.right * 5f, Quaternion.identity, "PLAYER", 1);
		addWingman(0, prefabManager.Instance.getFriendlyFighter(0F, 1F), PlayerController.Instance.ourAircraft.transform.position + Vector3.back * 5f - Vector3.right * 5f, Quaternion.identity, "PLAYER", 0);
	}

	public actorWrapper addWingman(int thisTeam, GameObject thisPrefab, Vector3 thisPosition, Quaternion thisRotation, string groupTag, int formationPosition)
	{

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
		newActor.radarObject.transform.SetParent(LevelController.Instance.targetRadar.gameObject.transform); //child it to this.
		newActor.radarObject.transform.localScale = Vector3.one;
		newActor.radarLink = newActor.radarObject.GetComponent<RadarItem>();


		((AI_Fighter)newActor.ourController).formationNumber = formationPosition; // LevelController.Instance.getFormationNumber(thisTeam, PlayerController.Instance.ourAircraft.gameObject);
																				  //newActor.ourController.setPatrol(Random.Range(30, 35)); //set everything here on patrol
		((AI_Fighter)newActor.ourController).pattern = "FOLLOW";

		((AI_Fighter)newActor.ourController).followTarg = PlayerController.Instance.ourAircraft.gameObject; //follow this
		((AI_Fighter)newActor.ourController).flightGroup = groupTag;

		//Now we need to figure out which list we add it to
		if (thisTeam == 0)
		{
			LevelController.Instance.friendlyList.Add(newActor);
		}
		else if (thisTeam == 1)
		{
			LevelController.Instance.enemyList.Add(newActor);
			newActor.actor.gradeSkill(0.5f);

		}

		//return newTarget; //for those of us who need the vehicle
		return newActor;
	}

	//So I think the core question is "where do we construct our mission?"
	void ConstructMission(List<LevelObjectiveSet> objectives, float difficulty) //, enObjectiveType secondaryObjective, enObjectiveType tirtaryObjective)
	{
		//So every mission is like telling a story that'll be connected to the main game.
		//From the mission map we might get goals which will give us our primary mission objective/context
		//Primary objective
		//Secondary objective (optional)
		//Tirtary objective (optional)
		//Generic filler
		//Twist(s) present in mission

		//Pacing: needs to have time for the player to get their bearings, build up to a climax and be something the player can overcome with flair

		//I'd imagine that we'll need to have constructors for the events to keep things neat (i.e. aircraft constructor etc.)

		missionEvents.Clear();
		missionEvents = new List<missionEvent>();

		foreach(LevelObjectiveSet thisObjective in objectives)
        {
			switch (thisObjective.thisObjective)
            {
				case LevelObjectiveSet.enObjectiveType.FIGHTERS:
					createFighterEvent();
					break;
				case LevelObjectiveSet.enObjectiveType.BALLOONS:
					//We need a location for these to happen around
					Vector3 randomPoint = new Vector3(Random.Range(-50, 50), 80, Random.Range(-50, 50));
					createBalloonEvent(randomPoint, 12f);
					break;
				default:
					break;
            }
        }
		//For things like the base/balloons we could do with spawning them in a cluster x distance from the starting point of the player,
		//and then building around this setup
		//Logically from that the different objectives will have different locatoins too
	}

	void createBalloonEvent(Vector3 clusterCenter, float clusterSpacing)
    {
		//I think balloons will be put down as their own objects so we've a bit of control over their location
		missionEvent newEvent = new missionEvent();
		newEvent.eventType = missionEvent.enEventType.BALLOON;
		newEvent.playerTask = missionEvent.enPlayerTask.DESTROY;
		newEvent.objectiveType = missionEvent.enObjectiveType.VISIBLE;
		newEvent.triggerType = missionEvent.enTriggerType.START;

		newEvent.eventTeam = 1; //Enemies for the moment

		int flightCount = Mathf.RoundToInt(Random.Range(1, 3));
		//int flightCount = 1;	//PROBLEM this is a testing hack
		float lineAngle = Random.Range(0f, 360f);
		Quaternion AngleAxis = Quaternion.AngleAxis(lineAngle, Vector3.up);
		Vector3 lineForward = new Vector3(clusterSpacing, 0, 0);
		for (int i = 0; i < flightCount; i++)
		{
			newEvent.spawnObject.Add(prefabManager.Instance.enemyBalloons[0]);
			//So there are a couple of ways we could put down balloons:
			//Within a circle
			//More realistically along a line...
			Vector3 spawnLocation = clusterCenter;
			spawnLocation += AngleAxis * lineForward*i;
			//Make our line wonky
			float clusterNoise = clusterSpacing / 3f;
			spawnLocation += new Vector3(Random.Range(-clusterNoise, clusterNoise), Random.Range(-clusterNoise, clusterNoise), Random.Range(-clusterNoise, clusterNoise));
			newEvent.spawnLocations.Add(spawnLocation);
		}

		missionEvents.Add(newEvent);
	}

	//Really we need to have details for our setup passed through to us here instead of this crappy setup I've got
	void createFighterEvent() {
		//For the moment lets dump in a collection of elements
		int numElements = 1; //Why do we even have this? I want to add the event, not multiples OF the event
		//numElements = 1;
		for (int i=0; i<numElements; i++)
        {
			missionEvent newEvent = MEV_MakeFlight();	//The fighers are added in the event, where as the balloons are added in the create event function
			newEvent.triggerType = missionEvent.enTriggerType.TIME;
			newEvent.triggerValue = 3 + i * 10;
			missionEvents.Add(newEvent);
        }
    }

	//Make a flight of fighters as a mission event object
	missionEvent MEV_MakeFlight()
    {
		missionEvent newFlight = new missionEvent();
		newFlight.eventType = missionEvent.enEventType.PLANES;
		newFlight.playerTask = missionEvent.enPlayerTask.DESTROY;
		newFlight.objectiveType = missionEvent.enObjectiveType.VISIBLE;

		newFlight.eventTeam = 1; //Enemies for the moment

		int flightCount = Mathf.RoundToInt(Random.Range(1, 3));
		flightCount = 1;
		for (int i=0; i<flightCount; i++)
        {
			newFlight.spawnObject.Add(prefabManager.Instance.enemyPrefabList[0]);
        }

		return newFlight;
    }


	void ProcessMissionEvents () {
		gameTime = 0;
		//foreach (missionEvent thisMissionEvent in missionEvents)
		for (int i=0; i<missionEvents.Count; i++) {
			//public enum enEventType { NONE, PLANES, BASE, STATIC, VEHICLES, BALLOON, PHOTO } //This will be an event driver that'll make something of it
			//public enEventType eventType = enEventType.NONE;
			missionEvent thisMissionEvent = missionEvents[i]; //For ease of handling
			GameObject eventObject = Instantiate(new GameObject("Fighter Event:" + i.ToString()), this.gameObject.transform);
			MissionEventObject newGameMissionEvent;

			//Assign mission event to goals
            switch (thisMissionEvent.playerTask)
            {
				case missionEvent.enPlayerTask.DESTROY:
					activeEvents.Add(thisMissionEvent);
					break;
				case missionEvent.enPlayerTask.PROTECT:
					activeEvents.Add(thisMissionEvent);
					break;
				default:
					break;
            }

			//Create mission event
            switch (thisMissionEvent.eventType)
			{
				case missionEvent.enEventType.NONE:
					Debug.LogError("Mission event set to none. You screwed up somewhere.");
					break;
				case missionEvent.enEventType.PLANES:
					//GameObject eventObject = Instantiate(new GameObject("Fighter Event:"+i.ToString()), this.gameObject.transform);
					newGameMissionEvent = eventObject.AddComponent<MEV_Fighters>();  //new MEV_Fighters();
					newGameMissionEvent.ourMissionEvent = thisMissionEvent;
					newGameMissionEvent.ourMissionEventManager = this;
					Debug.Log(newGameMissionEvent);	//This is coming through as null?
					gameMissionEvents.Add(newGameMissionEvent);
					Debug.Log("Adding Planes mission event");
					break;
				case missionEvent.enEventType.BALLOON:
					//GameObject eventObject = Instantiate(new GameObject("Fighter Event:" + i.ToString()), this.gameObject.transform);
					newGameMissionEvent = eventObject.AddComponent<MEV_Balloons>();  //new MEV_Fighters();
					newGameMissionEvent.ourMissionEvent = thisMissionEvent;
					newGameMissionEvent.ourMissionEventManager = this;
					Debug.Log(newGameMissionEvent); //This is coming through as null?
					gameMissionEvents.Add(newGameMissionEvent);
					Debug.Log("Adding balloons mission event");
					break;
				default:
					//doSomething
					break;
			}
		}

		for (int i=0; i<gameMissionEvents.Count; i++) { //Our setup pass for the spawned mission events
			//So we've successfully "made" this event and hopefully set everything up correctly
			//newGameMissionEvent.ourMissionEvent = thisMissionEvent; //Assign this so that the class knows what's going on
			//Assign additional trigger events here, or downstream in a final linking pass
			switch (gameMissionEvents[i].ourMissionEvent.triggerType)
            {
				case missionEvent.enTriggerType.DESTROYED:
					//We need to assign the event that we're watching so that we know when to do our trigger
					gameMissionEvents[i].ourMissionEvent.watchedEvent = gameMissionEvents[gameMissionEvents[i].ourMissionEvent.watchedMisionEvent];
					break;
				case missionEvent.enTriggerType.DAMAGED:
					gameMissionEvents[i].ourMissionEvent.watchedEvent = gameMissionEvents[gameMissionEvents[i].ourMissionEvent.watchedMisionEvent];
					break;
				case missionEvent.enTriggerType.DISTANCE:	//Hang on, is this player distance to something, or player distance to us? Do we know where we are?
					gameMissionEvents[i].ourMissionEvent.watchedEvent = gameMissionEvents[gameMissionEvents[i].ourMissionEvent.watchedMisionEvent];
					break;

				default:
					break;

			}
        }
	}

	public void EventComplete(MissionEventObject thisEvent)
    {
		if (gameManager.Instance.bDebugEvents)
		{
			Debug.LogError("Event Complete Callback: " + thisEvent);
		}
		if (activeEvents.Contains(thisEvent.ourMissionEvent)) {
			activeEvents.Remove(thisEvent.ourMissionEvent);
        }

		if (passiveEvents.Contains(thisEvent.ourMissionEvent)) {
			passiveEvents.Remove(thisEvent.ourMissionEvent);
        }

		if (hiddenActiveEvents.Contains(thisEvent.ourMissionEvent))
		{
			hiddenActiveEvents.Remove(thisEvent.ourMissionEvent);
		}

		if (hiddenPassiveEvents.Contains(thisEvent.ourMissionEvent))
		{
			hiddenPassiveEvents.Remove(thisEvent.ourMissionEvent);
		}

		if (activeEvents.Count == 0) {  //we have no more active events therefore the level is finished
			if (gameManager.Instance.bDebugEvents)
			{
				Debug.LogError("All active events cleared");
			}
			//Notify our systems
			LevelController.Instance.finishMatch(false);
		}
	}

	public void EventAllDestroyed(MissionEventObject thisEvent)
    {
		//This will then need to check against objectives (fail/complete) and pass information through to our other events least this has to
		//trigger something if we need to
		if (gameManager.Instance.bDebugEvents)
		{
			Debug.LogError("Entire missionEvent destroyed");
		}
		//if (thisEvent) //Check to see if this is critical for the mission completion (or logic like that)
		//For the moment lets check and see if we've got any mission events left active
		/*
		bool bEventActive = false;
		foreach (MissionEventObject thisMissionEvent in gameMissionEvents)
        {
			if (thisMissionEvent)
            {
				bEventActive = true;
            }
        }*/

		/*
		if (!bEventActive) {  //see about sending through the "level finished" call
			Debug.LogError("MissionEvents cleared");
		}*/

    }
    
	
	// Update is called once per frame
	void Update () {
		gameTime = Time.time;
		//Tick our mission events, and let them sort out what they should be doing
		foreach (MissionEventObject thisMissionEvent in gameMissionEvents)
        {
			thisMissionEvent.doTick(gameTime);
        }
	}

	public virtual void actorTakingDamage(MissionEventObject thisEvent, Actor thisActor, float healthRatio)
	{
		Debug.Log("Actor taking damage: " + healthRatio);
		//I'm not sure what we can do with this now? Send a trigger through the system? I can see it being useful, but likewise I'm not sure how it'll
		//handle in a group
		//specialTrigger(thisEvent, healthRatio);
		/*
		foreach (MissionEventObject thisMissionEvent in gameMissionEvents)
		{
			thisMissionEvent.specialTrigger(thisEvent, healthRatio);
		}*/
	}
}
