using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//A point that does something when it's triggered, but also has things of interest around it
public class reconPointActor : Actor {

	public MissionConstructionBase ourMissionConstructor;

	public enum enReconPointState { NULL, WAITING, TRIGGERED, RESOLVED }
	public enReconPointState ReconPointState = enReconPointState.WAITING;

	public float triggerDistance = 25; 
	public bool bTriggerDistanceFlat = true; //Of course this is "flat distance" unless specificed

	//So what can happen here? Really just fighters and maybe associated events.
	public int FightersToSpawn = 3;

	//And then there's things we'll have to have prepared before play begins
	public int numPhotoPoints = 2;
	public int numBalloons = 2;

	public bool bMissionKeyPoint = false;

	public float stateTriggerDistance = 25f;


	// Use this for initialization
	void Start () {
		DoStart();
	}
	
	public virtual void DoStart()
    {

    }

	public virtual void SpawnPhotoPoints(Vector3 newPosition, int count)
    {
		if (ourMissionConstructor)
		{
			ourMissionConstructor.addPhotoCluster(newPosition, 40f, new Range(count, count));
		}
    }

	public virtual void SpawnBalloonsAround(Vector3 newPosition, int count)
    {
		if (ourMissionConstructor)
		{
			ourMissionConstructor.addBarrageBalloons(newPosition, 25f, new Range(count, count), 1); ;
		}
	}

	public virtual void DoSetup(Vector3 newPosition, MissionConstructionBase newMissionConstructor, int newNumBalloons, int newNumSpawnedFighters, int newNumPhotoPoints, bool bIsMissionIKeyPoint)
    {
		ourMissionConstructor = newMissionConstructor;
		numBalloons = newNumBalloons;
		numPhotoPoints = newNumPhotoPoints;
		FightersToSpawn = newNumSpawnedFighters;

		if (numBalloons > 0)
		{
			SpawnBalloonsAround(gameObject.transform.position, numBalloons);
		}
		if (numPhotoPoints > 0)
		{
			SpawnPhotoPoints(gameObject.transform.position, numPhotoPoints);
		}

		//And need to handle the fighters
		//Add ourselves to the level controller
		//((LevelController)LevelControllerBase.Instance).AddReconPoint(0, gameObject);
	}

	// Update is called once per frame
	void Update () {
		//Lets work with flat trigger distance for the moment
		if (ReconPointState == enReconPointState.WAITING)
        {
			if (Vector2.SqrMagnitude(new Vector2(gameObject.transform.position.x, gameObject.transform.position.z) - new Vector2(PlayerController.Instance.transform.position.x, PlayerController.Instance.transform.position.z)) < triggerDistance * triggerDistance)
            {
				TriggerReconPoint();
				ReconPointState = enReconPointState.TRIGGERED;
            }
        }
	}

	public virtual void TriggerReconPoint()
    {
		//Ok, now what? I assume that there's something we've got to do here?
		//Lets start by spitting out what there is to do at this point!
		//Debug.LogError("Photos: " + numPhotoPoints + " Balloons: " + numBalloons + " Fighters: " + FightersToSpawn);
		//We should disable or change our icon for the radar
		((LevelController)LevelControllerBase.Instance).RemoveReconPoint(gameObject);	//We should remove this so that it's not hanging around, and make sure that our activities happen

		//Need to play some sort of comment to inform the player of what's happening around here
		if (numPhotoPoints + numBalloons + FightersToSpawn == 0)	//Empty node
        {
			if (LevelChatterController.Instance)
			{
				LevelChatterController.Instance.playChatter("nothinghappening");
			}
		} else if (numPhotoPoints > 0 && numBalloons + FightersToSpawn == 0)	//Only photos
        {
			if (LevelChatterController.Instance)
			{
				LevelChatterController.Instance.playChatter("hasphotos");
			}
		} else if (numBalloons > 0)
        {
			if (LevelChatterController.Instance)
			{
				LevelChatterController.Instance.playChatter("hasballoons");
			}
		}
	}
}
