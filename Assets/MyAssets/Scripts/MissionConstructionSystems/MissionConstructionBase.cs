using System.Collections;
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

    flightGroup MakeFlight_Fighters(int team, int number)
    {
        flightGroup newFlight = new flightGroup();

        return newFlight;
    }

	#region Actor Construction Functions
	public static Vector2 Vec2Rotate(Vector2 v, float delta)
	{
		return new Vector2(
			v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
			v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
		);
	}

	//I do wonder if these should be on the level controller
	public flightGroup AddFlightGroup(Vector3 dropPoint, float spawnRadius, float formationStagger, float awakeTime, int enemyCount, int team)
	{
		flightGroup newFlight = new flightGroup();

		if (gameManager.Instance.bDebugEvents) { Debug.LogError("Triggering fighters Event"); }

		Vector3 startPoint = dropPoint + new Vector3(Random.Range(-spawnRadius, spawnRadius), Random.Range(-10, 10), Random.Range(-spawnRadius, spawnRadius)); //this is forward of the player, we're looking toward it I'd guess
																																								//This is Random for the moment
		float startAngle = Random.Range(0f, 6.2831f);
		Vector2 targetDir = Vec2Rotate(Vector2.right, Random.Range(0, startAngle));

		Quaternion startQuat = Quaternion.Euler(0, startAngle, 0); //set this going on an intercept course with the chosen point
		Quaternion transQuat = Quaternion.Euler(0, startAngle + 90, 0);

		Vector3 patrolOffset = Vector3.zero;

		//need to come up with some method to name the groups. At some stage we might be spawning multiple patrols out of this
		string groupTag = "wayGoal_Group_" + Time.time.ToString("f0");

		//so when we're putting enemies down it's in a triangle formation
		for (int i = 0; i < enemyCount; i++)
		{

			if (i != 0)
			{
				if (i % 2 == 0)
				{
					patrolOffset = transQuat * Vector3.forward * formationStagger - startQuat * Vector3.forward * formationStagger;
				}
				else
				{
					patrolOffset = transQuat * -Vector3.forward * formationStagger - startQuat * Vector3.forward * formationStagger;
				}
			}

			//Add this actor to our level controller so it'll show up on radar etc.
			actorWrapper newActor = ((LevelController)LevelControllerBase.Instance).addFighterActor(prefabManager.Instance.enemyPrefabList[0], team, startPoint + patrolOffset, startQuat, groupTag, null);
			//Assign our AI actions for this fighter
			((AI_Fighter)newActor.ourController).setPatrol(awakeTime); //set this fighter to a patrol for however many seconds. //.pattern="PATROL";
			newFlight.addActor(newActor);

		}

		return newFlight;
	}

	#endregion
}
