using UnityEngine;
using System.Collections;

//puts down a group of aircraft without an intercept requirement.
public class wayGoal_Group : wayGoal {

	//int enemyCount = 3; //but this will need to be set somewhere else
	//float dropRadius = 100;

	//actorWrapper[] wayActors;

	//return; //don't enable this to activate
	public override void callWaypoint(float playerDist) {

		waypointState = enWaypointState.Active; //set this to active...

		//So much of this will depend on where our player fighter is, and how it's presented for the "best play" option...
		Vector3 playerLocation = PlayerController.Instance.ourAircraft.transform.position; //spwan around this.
		Vector3 playerRotation = PlayerController.Instance.ourAircraft.transform.eulerAngles;
		Quaternion playerQuat = PlayerController.Instance.ourAircraft.transform.rotation;
	
		
		Vector3 targetPoint = playerLocation + playerQuat*Vector3.forward * dropRadius/3f ; //this is forward of the player, we're looking toward it I'd guess

		NGUI_Base.Instance.setGameMessage("A group of Enemies!");
		
		float randomDirection = Random.Range(0F, 360F*Mathf.Deg2Rad);
		
		Vector3 patrolStart = playerLocation + new Vector3(Mathf.Sin(randomDirection)*dropRadius, Random.Range(-dropRadius/4F, dropRadius/4F), Mathf.Cos(randomDirection)*dropRadius);
		
		//need to calc what our directional stuff is for this group
		Vector2 targetDir = new Vector2(targetPoint[0] - patrolStart[0], targetPoint[2] - patrolStart[2]);
		
		float startAngle = Mathf.Acos(targetDir[1]/targetDir.magnitude)*Mathf.Rad2Deg;
		
		Quaternion startQuat = Quaternion.Euler(0, startAngle, 0); //set this going on an intercept course with the chosen point
		
		Quaternion transQuat = Quaternion.Euler (0, startAngle + 90, 0);
		
		//((LevelController)LevelControllerBase.Instance).waypointCallbackEnemy(enemyCount);
		
		float waypointStagger = 3; //how far apart are our fighters?
		
		Vector3 patrolOffset = Vector3.zero;
		
		//need to come up with some method to name the groups. At some stage we might be spawning multiple patrols out of this
		string groupTag = "wayGoal_Group_" + Time.time.ToString("f0");

		wayActors = new actorWrapper[enemyCount];

		//so when we're putting enemies down it's in a triangle formation
		for (int i =0; i<enemyCount; i++) {
			
			if (i!=0) {
				if (i%2 == 0) { //was that right?
					patrolOffset = transQuat*Vector3.forward*waypointStagger - startQuat*Vector3.forward*waypointStagger;
				}
				else {
					patrolOffset = transQuat*-Vector3.forward*waypointStagger - startQuat*Vector3.forward*waypointStagger;
				}
			}
			
			//((LevelController)LevelControllerBase.Instance).waypointCallbackEnemy(prefabManager.Instance.getEnemyFighter(0F, 1F), patrolStart + patrolOffset, startQuat, groupTag);
			wayActors[i] = ((LevelController)LevelControllerBase.Instance).addFighterActor(prefabManager.Instance.getEnemyFighter(0F, 1F), 1, patrolStart + patrolOffset, startQuat, groupTag, null);

			((AI_Fighter)wayActors[i].ourController).setPatrol(10f); //set this fighter to a patrol for however many seconds. //.pattern="PATROL";
		}
	}

	/*
	bool bAllDown = false;


	void Update() {
		bAllDown = true;
		
		if (waypointState == enWaypointState.Active) {
			for (int i=0; i<wayActors.Length; i++) {
				if (!wayActors[i].actor.bIsDead)
					bAllDown = false;
			}
			
			if (bAllDown) { //we've cleared this node
				waypointState = enWaypointState.Done;
				
				//call back to our main system and tell it that this is done...
	((LevelController)LevelControllerBase.Instance).waypointCallback(gameObject);
			}
		}
	}
	*/
}
