using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Because a gunner mission is different from a standard objective-based mission we need a different handler for enemies which will be spawned in waves
public class GunnerGameModeHandler : MonoBehaviour {

	protected float dropRadius = 20; //might need to modulate this...
									 //I expect we can handle almost everything from here actually
	float levelStartTime = 0;
	public float levelDuration = 180; //How long until we enter the bombing run (in seconds)

	bool bNextLevelLoading = false;

	int nextMinSpawnCount = 2;	//At what number of planes remaining will we spawn more?

	IEnumerator Start()
    {
		nextMinSpawnCount = Random.Range(1, 3);
		levelStartTime = Time.time;
		//We could do with giving the player some escort wingmen
		yield return null;

		//This isn't working because of speeds. It's going to need a little extra AI logic I think
		addWingman(0, prefabManager.Instance.getFriendlyFighter(0F, 1F), PlayerController.Instance.ourAircraft.transform.position + Vector3.back * 7f + Vector3.right * 5f, Quaternion.identity, "PLAYER", 1);
		addWingman(0, prefabManager.Instance.getFriendlyFighter(0F, 1F), PlayerController.Instance.ourAircraft.transform.position + Vector3.back * 7f - Vector3.right * 5f, Quaternion.identity, "PLAYER", 0);
	}
	
	public void AddEnemyFlightGroup(int enemyCount, int team)
	{

		if (gameManager.Instance.bDebugEvents) { Debug.LogError("Triggering fighters Event"); }
		//So much of this will depend on where our player fighter is, and how it's presented for the "best play" option...
		Vector3 playerLocation = PlayerController.Instance.ourAircraft.transform.position; //spwan around this.
		Vector3 playerRotation = PlayerController.Instance.ourAircraft.transform.eulerAngles;
		Quaternion playerQuat = PlayerController.Instance.ourAircraft.transform.rotation; 


		Vector3 targetPoint = playerLocation + new Vector3(Random.Range(-dropRadius, dropRadius), Random.Range(-10, 10), Random.Range(-dropRadius, dropRadius)); //this is forward of the player, we're looking toward it I'd guess

		//NGUI_Base.Instance.setGameMessage("A group of Enemies!");

		float randomDirection = Random.Range(0F, 360F * Mathf.Deg2Rad);

		Vector3 patrolStart = playerLocation + new Vector3(Mathf.Sin(randomDirection) * dropRadius, Random.Range(-dropRadius / 4F, dropRadius / 4F), Mathf.Cos(randomDirection) * dropRadius);

		//need to calc what our directional stuff is for this group
		Vector2 targetDir = new Vector2(targetPoint[0] - patrolStart[0], targetPoint[2] - patrolStart[2]);

		float startAngle = Mathf.Acos(targetDir[1] / targetDir.magnitude) * Mathf.Rad2Deg;

		Quaternion startQuat = Quaternion.Euler(0, startAngle, 0); //set this going on an intercept course with the chosen point

		Quaternion transQuat = Quaternion.Euler(0, startAngle + 90, 0);

		//LevelController.Instance.waypointCallbackEnemy(enemyCount);

		float waypointStagger = 3; //how far apart are our fighters?

		Vector3 patrolOffset = Vector3.zero;

		//need to come up with some method to name the groups. At some stage we might be spawning multiple patrols out of this
		string groupTag = "wayGoal_Group_" + Time.time.ToString("f0");

		//groupActors = new actorWrapper[enemyCount];

		//so when we're putting enemies down it's in a triangle formation
		for (int i = 0; i < enemyCount; i++)
		{

			if (i != 0)
			{
				if (i % 2 == 0)
				{ //was that right?
					patrolOffset = transQuat * Vector3.forward * waypointStagger - startQuat * Vector3.forward * waypointStagger;
				}
				else
				{
					patrolOffset = transQuat * -Vector3.forward * waypointStagger - startQuat * Vector3.forward * waypointStagger;
				}
			}

			//LevelController.Instance.waypointCallbackEnemy(prefabManager.Instance.getEnemyFighter(0F, 1F), patrolStart + patrolOffset, startQuat, groupTag);
			//actorWrapper newActor = LevelController.Instance.waypointCallbackEnemy(prefabManager.Instance.getEnemyFighter(0F, 1F), patrolStart + patrolOffset, startQuat, groupTag);
			//Add this actor to our level controller so it'll show up on radar etc.
			actorWrapper newActor = LevelController.Instance.addFighterActor(prefabManager.Instance.enemyPrefabList[0], team, patrolStart + patrolOffset, startQuat, groupTag, null);
			//Assign our AI actions for this fighter
			((AI_Fighter)newActor.ourController).setPatrol(2f); //set this fighter to a patrol for however many seconds. //.pattern="PATROL";
			//groupActors.Add(newActor);
		}
	}

	//The fact that there's so much copying and pasting going on right now suggests that I really need to get in some sort of library system
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
		((AI_Fighter)newActor.ourController).pattern = "PATROL";

		//((AI_Fighter)newActor.ourController).followTarg = PlayerController.Instance.ourAircraft; //follow this
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

	void Update()
    {

		//Check to see if we should keep adding enemies into the level to harass the player
		if (LevelController.Instance.enemyList.Count < nextMinSpawnCount && Time.time-levelStartTime > 5f)	//Make sure that we give everything a breath before we're into it. This might be modified for storytelling reasons
        {
			AddEnemyFlightGroup(Random.Range(1, 4), 1);
        }

		if (Time.time-levelStartTime > levelDuration && !bNextLevelLoading)
        {
			bNextLevelLoading = true;	//PROBLEM: I really need a better way to handle this because this is very hacky
			LevelController.Instance.finishMatch(false);
        }
    }
}
