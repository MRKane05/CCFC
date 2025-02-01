using UnityEngine;
using System.Collections;



//This class spawns enemies when we go into it. It'll need a little refining.
public class waypoint_Enemy : waypoint {

	public enum enDropType {Rand, Patrol, Ambush};
	public enDropType dropType = enDropType.Patrol; //so we'll spawn a patrol of fighters.
	
	public int enemyCount = 2;

	float dropRadius = 70F;

	void Start() {
		if (dropType == enDropType.Rand) { //we need to assign a randome drop type to this
			int enLength = System.Enum.GetValues (typeof(enDropType)).Length; //enDropType.GetValues().Length; //System.Enum.GetValues(enDropType).Length;
			int randomDrop = Mathf.RoundToInt(Random.Range (1, enLength)); //enDropType.GetValues().Length));
			dropType = (enDropType)(randomDrop);
		}

	}

	//this is really all we need to worry about
	public override void triggerWaypoint() {
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
		if (dropType == enDropType.Ambush) { //we're spawning fighters in from every direction around the player
			//Technically these fighters would be coming in pairs but I'm not too sure we'll be doing that here

			NGUI_Base.Instance.setGameMessage("It's an Ambush!");

			for (int i=0; i<enemyCount; i++) {
				float randomDirection = Random.Range(0F, 360F*Mathf.Deg2Rad);

				//so we're putting a fighter down around the player with the constraint that the player is always at the bottom of the stack because it's...well it's an ambush!
				Vector3 ambushPoint = playerLocation + new Vector3(Mathf.Sin (randomDirection)*dropRadius, Random.Range (0, dropRadius/3F), Mathf.Cos(randomDirection)*dropRadius);

				//We're aiming at the player here.
				Vector2 targetDir = new Vector2(playerLocation[0] - ambushPoint[0], playerLocation[2] - ambushPoint[2]);

				float startAngle = Mathf.Acos(targetDir[1]/targetDir.magnitude)*Mathf.Rad2Deg;

				Quaternion startQuat = Quaternion.Euler(0, startAngle, 0);

				string groupTag = gameObject.name;

	((LevelController)LevelControllerBase.Instance).addFighterActor(prefabManager.Instance.getEnemyFighter(0F, 1F), 1, ambushPoint, startQuat, groupTag, null);

			}
		}
		if (dropType == enDropType.Patrol) {

			NGUI_Base.Instance.setGameMessage("You spy an Enemy Patrol");

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
			string groupTag = gameObject.name;

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

	((LevelController)LevelControllerBase.Instance).addFighterActor(prefabManager.Instance.getEnemyFighter(0F, 1F), 1, patrolStart + patrolOffset, startQuat, groupTag, null);
			
			}
		}
		waypointActive=false; //turn this off.
	}
//0279504893
}
