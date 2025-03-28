﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class flightGroup
{

    //public int eventTriggerCount = 0; //If the number of vehicles in this flight goes below zero it'll trigger an event (such as getting new fighters)

    //So for this we're going to have a flight of aircraft
    public List<actorWrapper> spawnedVehicles = new List<actorWrapper>();


    //Built in return functions
    public bool removeActor(GameObject thisActor)
    {

        return true;
    }

	public bool addActor(actorWrapper thisActor)
    {
		spawnedVehicles.Add(thisActor);
		return true;
    }
}

//Because we've made many of the elements of the game into mini-games we can really drill down what our missions will be and simplify things further here
public class MissionConstructionBase : MonoBehaviour {
	bool bWaitingToFinishMision = false;

    private static MissionConstructionBase instance = null;
    public static MissionConstructionBase Instance { get { return instance; } }

    public enum enMissionPlayState { NULL, START, PLAYING, FINISHED }
    public enMissionPlayState MissionPlayState = enMissionPlayState.START;

    void Awake()
    {
        if (instance)
        {
            Debug.Log("Duplicate attempt to create MissionConstructor");
            Destroy(this);
            return;
        }

        instance = this;
    }

    //We're going to have to start somewhere!
    IEnumerator Start() {

		yield return null;

		if (!prefabManager.Instance)
		{
			yield return null;
		}
		//Now we should be fine for sending through the start function
		DoStart();
    }

    public virtual void DoStart()
    {
        MissionPlayState = enMissionPlayState.PLAYING;
    }

	public void DelayFinishMission(bool bSuccess, float delayTime)
    {
		if (!bWaitingToFinishMision)
		{
			bWaitingToFinishMision = true;
			StartCoroutine(DoDelayFinishMission(delayTime));
		}
    }

	IEnumerator DoDelayFinishMission(float delayTime)
    {
		yield return new WaitForSeconds(delayTime);
		((LevelController)LevelControllerBase.Instance).finishMatch(false);
	}

    public virtual void ConstructMission()
    {
        //So to kick off we're going to need a place where the mission is happening
        //Depending on the mission type we'll have to set our objectives which will be destroy/protect
        //These objectives mightn't happen all at once, so we'll need some sort of global "wave handler" system for controlling how waves come into the scene
        //We'll also need bonus objectives to complete littered around in a constructive way
        //Really that's about it on the surface
    }

	public void Update()
    {
		DoUpdate();
    }

	public virtual void DoUpdate()
    {

    }

	public virtual void RemoveActor(GameObject thisActor)
    {

	}
}
