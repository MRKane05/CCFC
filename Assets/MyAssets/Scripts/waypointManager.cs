using UnityEngine;
using System.Collections;

[System.Serializable]
public class wayGoalWrapper {
	public GameObject wayPrefab;
	public float incidence = 10f; //what are our odds of drawing this?
}

public class waypointManager : MonoBehaviour {
	private static waypointManager instance = null;
	public static waypointManager Instance {get {return instance;}}

	public wayGoalWrapper[] wayGoals; //our list of possible wayGoals which will be accessed when creating them.

	float groupIncidence=0;

	void Awake() {
		if (instance)
		{
			Debug.Log("Duplicate attempt to create waypointManager");
			Destroy(this);
			return;
		}
		
		instance = this;

		//sum up our Incidence values...
		foreach(wayGoalWrapper thisGoal in wayGoals) {
			groupIncidence += thisGoal.incidence;
		}
	}

	public GameObject requestWayGoal(float difficulty) { //not sure how the difficulty will affect things, but anyway...
		//the wayGoals should never be less than 0...

		float incidentRandom = Random.Range (0, groupIncidence);

		for (int i=0; i< wayGoals.Length; i++) {
			if (incidentRandom < wayGoals[i].incidence || i == wayGoals.Length-1) { //if we're in the range, or this is the last entry, return
				return wayGoals[i].wayPrefab;
				break; //we're done here
			}

			incidentRandom -= wayGoals[i].incidence;

		}

		return null; //this should never happen.
	}
}
