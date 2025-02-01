using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MEV_Balloons : MissionEventObject {

	protected float dropRadius = 50; //might need to modulate this...
	public override void doTrigger()
	{
		base.doTrigger();
		if (gameManager.Instance.bDebugEvents)
		{
			Debug.LogError("Triggering Balloons Event");
		}
		//So much of this will depend on where our player fighter is, and how it's presented for the "best play" option...
		/*
		Vector3 playerLocation = PlayerController.Instance.ourAircraft.transform.position; //spwan around this.
		Vector3 playerRotation = PlayerController.Instance.ourAircraft.transform.eulerAngles;
		Quaternion playerQuat = PlayerController.Instance.ourAircraft.transform.rotation;


		Vector3 targetPoint = playerLocation + playerQuat * Vector3.forward * dropRadius / 3f; //this is forward of the player, we're looking toward it I'd guess
		
		

		float randomDirection = Random.Range(0F, 360F * Mathf.Deg2Rad);

		Vector3 patrolStart = playerLocation + new Vector3(Mathf.Sin(randomDirection) * dropRadius, Random.Range(-dropRadius / 4F, dropRadius / 4F), Mathf.Cos(randomDirection) * dropRadius);

		//need to calc what our directional stuff is for this group
		Vector2 targetDir = new Vector2(targetPoint[0] - patrolStart[0], targetPoint[2] - patrolStart[2]);

		float startAngle = Mathf.Acos(targetDir[1] / targetDir.magnitude) * Mathf.Rad2Deg;

		Quaternion startQuat = Quaternion.Euler(0, startAngle, 0); //set this going on an intercept course with the chosen point

		Quaternion transQuat = Quaternion.Euler(0, startAngle + 90, 0);

		//((LevelController)LevelControllerBase.Instance).waypointCallbackEnemy(enemyCount);

		float waypointStagger = 3; //how far apart are our fighters?

		Vector3 patrolOffset = Vector3.zero;

		//need to come up with some method to name the groups. At some stage we might be spawning multiple patrols out of this
		string groupTag = "wayGoal_Group_" + Time.time.ToString("f0");

		//groupActors = new actorWrapper[enemyCount];
		*/
		//need to come up with some method to name the groups. At some stage we might be spawning multiple patrols out of this
		string groupTag = "wayGoal_Group_" + Time.time.ToString("f0");
		//so when we're putting enemies down it's in a triangle formation
		for (int i = 0; i < ourMissionEvent.spawnObject.Count; i++)
		{
			//((LevelController)LevelControllerBase.Instance).waypointCallbackEnemy(prefabManager.Instance.getEnemyFighter(0F, 1F), patrolStart + patrolOffset, startQuat, groupTag);
			//actorWrapper newActor = ((LevelController)LevelControllerBase.Instance).waypointCallbackEnemy(prefabManager.Instance.getEnemyFighter(0F, 1F), patrolStart + patrolOffset, startQuat, groupTag);
			//Add this actor to our level controller so it'll show up on radar etc.
			actorWrapper newActor = ((LevelController)LevelControllerBase.Instance).addFighterActor(ourMissionEvent.spawnObject[i], ourMissionEvent.eventTeam, ourMissionEvent.spawnLocations[i], Quaternion.identity, groupTag, this);
			//Assign our AI actions for this fighter
			//Is there AI for balloons?
			//((AI_Fighter)newActor.ourController).setPatrol(10f); //set this fighter to a patrol for however many seconds. //.pattern="PATROL";
			groupActors.Add(newActor);
		}
	}
}
