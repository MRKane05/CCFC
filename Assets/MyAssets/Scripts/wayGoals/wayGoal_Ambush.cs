using UnityEngine;
using System.Collections;

//Waygoals extend the waypoint class and are behaviours within themselves.
public class wayGoal_Ambush : wayGoal {
	//int enemyCount = 3; //but this will need to be set somewhere else
	//float dropRadius = 100;

	//need to remember our fighters that we've added so that we can call the next waypoint once they're gone.
	//actorWrapper[] wayActors;

	public override void callWaypoint(float playerDist) {

		waypointState = enWaypointState.Active;

		//in this case spawn a whole heap of fighters, and have the player shoot them up!
		//((LevelController)LevelControllerBase.Instance).triggerEnemies(); //with some details about what where etc.
		//((LevelController)LevelControllerBase.Instance).waypointCallback();
		
		//return; //don't enable this to activate
		
		//So much of this will depend on where our player fighter is, and how it's presented for the "best play" option...
		Vector3 playerLocation = PlayerController.Instance.ourAircraft.transform.position; //spwan around this.
		Vector3 playerRotation = PlayerController.Instance.ourAircraft.transform.eulerAngles;
		Quaternion playerQuat = PlayerController.Instance.ourAircraft.transform.rotation;
		
		//so we kind of need to use the Radar radius as a basis here? Or will it really matter?
		//If the fighter group is behind the player then it's much more tolerant then having it in front of the player, but
		//either way about 100 units is a good radius
		
		//really the fighters can be coming almost straight on, parallel, off to the side, but either way it needs to be from
		//a capable angle...
		
		//So if at any situation it's coming from the approach area through to the target point...
		
		Vector3 targetPoint = playerLocation + playerQuat*Vector3.forward * 30F ; //this is forward of the player, we're looking toward it I'd guess
		
		//So we will have to check that we're not putting fighters down under the ground at any point

		//Technically these fighters would be coming in pairs but I'm not too sure we'll be doing that here
		
		NGUI_Base.Instance.setGameMessage("It's an Ambush!");

		wayActors = new actorWrapper[enemyCount]; //use this for keeping our information about what there is here...

		for (int i=0; i<enemyCount; i++) {
			float randomDirection = Random.Range(0F, 360F*Mathf.Deg2Rad);
			
			//so we're putting a fighter down around the player with the constraint that the player is always at the bottom of the stack because it's...well it's an ambush!
			Vector3 ambushPoint = playerLocation + new Vector3(Mathf.Sin (randomDirection)*dropRadius, Random.Range (0, dropRadius/3F), Mathf.Cos(randomDirection)*dropRadius);
			
			//We're aiming at the player here.
			//..but it's not always working like that.
			Vector2 targetDir = new Vector2(playerLocation[0] - ambushPoint[0], playerLocation[2] - ambushPoint[2]);
			
			float startAngle = Mathf.Acos(-targetDir[1]/targetDir.magnitude)*Mathf.Rad2Deg;
			
			Quaternion startQuat = Quaternion.Euler(0, startAngle, 0);
			
			string groupTag = gameObject.name;

			//this adds the enemy, but we don't really have a reference to it...
			wayActors[i] = ((LevelController)LevelControllerBase.Instance).addFighterActor(prefabManager.Instance.getEnemyFighter(0F, 1F), 1, ambushPoint, startQuat, groupTag, null);
			
			//because this is an ambush don't be afraid to set these fighters into attack mode!
		
		}
	}
}
