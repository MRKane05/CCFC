using UnityEngine;
using System.Collections;

public class wayGoal_Balloon : wayGoal {

	//Some special details for balloons
	public float maxHeight = 300f, minHeight = 100f; //I really must look this up!

	//Also need to instigate the balloon tethers as well as an ack ack zone.

	//public override void callWaypoint(float playerDist) {
	//we want to spawn balloons when this waypoint is called from the prior list complete
	public override void orderCallWaypoint() { 
		//this one is a little special. We need to spawn a balloon (or group of balloons if that may be) somewhere where the
		//player can't see them.

		waypointState = enWaypointState.Active;


		//our waypoint distance from the end waypoint through to the player...
		Vector3 endPoint = LevelController.Instance.getMissionEndGoal.transform.position;
		Vector3 playerPoint = PlayerController.Instance.ourAircraft.transform.position;

		//so along the above vector find our start point for these balloons...
		Vector3 direction = (endPoint - playerPoint).normalized;


		Vector3 clusterPoint = endPoint - direction*checkDistance; //this is (theoretically) where the balloons should go...

		Debug.Log ("cluster mag: " + checkDistance/(endPoint-playerPoint).magnitude); //is somehow always spawning at zero - what have I mucked up?

		//So from that we've got to put these somewhere "offscreen" and limit the height.
		clusterPoint = new Vector3(clusterPoint[0], Mathf.Clamp(clusterPoint[1], minHeight, maxHeight), clusterPoint[2]);
		//so we'd like to put this down somewhere that's not necessariarally on the beaten track that the player should be drawn towards...
		//in other words: out to the side(s)

		Quaternion angleRot = Quaternion.Euler(0, Random.Range (70f, 110f), 0); //something to rotate the above direction by...

		Vector3 offsetDirection = angleRot * direction;

		if (Random.value > 0.5f) { //to the right
			clusterPoint += offsetDirection * Random.Range (40f, 70f);
		} else {
			clusterPoint -= offsetDirection * Random.Range (40f, 70f);
		}

		/*
		//Find a point outside of the view bounds (on the horozontal plane, and within the height of a barrage balloon)
		float direction = PlayerController.Instance.ourAircraft.transform.eulerAngles[1] + 60 + Random.Range (0f, 240f);

		//offset to limits of radar

		//need to also consider the distance we're meant to be away from the waypoint
		Vector3 clusterPoint = PlayerController.Instance.ourAircraft.transform.position + Quaternion.Euler(0, direction, 0)*Vector3.forward * 100f; //outside radar limits...
		*/
		wayActors = new actorWrapper[enemyCount];
		//spawn cluster of balloons around this mark as necessary
		for (int i=0; i< enemyCount; i++) {
			//we want to scatter these balloons around in a semi-correct pattern (whatever that is...)

			//for the moment lets just put them in a line...
			Vector3 balloonPoint = clusterPoint + Vector3.forward * i * 10f; //in a line!

			wayActors[i] = LevelController.Instance.addFighterActor(prefabManager.Instance.getEnemyBalloon(0F, 1F), 1, balloonPoint, Quaternion.identity, "", null);

		}

	}

	//Effectively nulled as we don't need this function for this class...
	public override void callWaypoint(float playerDist) {
		
	}

}
