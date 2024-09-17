using UnityEngine;
using System.Collections;

//base class for wayGoals, with some prompt functionality
public class wayGoal : waypoint {

	protected int enemyCount = 3; //but this will need to be set somewhere else
	protected float dropRadius = 50; //might need to modulate this...

	//need to remember our fighters that we've added so that we can call the next waypoint once they're gone.
	protected actorWrapper[] wayActors;


	#region generalFunctions

	public override void orderCallWaypoint() { }

	public override void callWaypoint(float playerDist) { }
	#endregion

	void Update() {

		PromptedUpdate();
	}

	protected bool bAllDown = false; //we want to see this dowstream...
	
	//used for all the end checking stuff etc.
	public virtual void PromptedUpdate() {

		bAllDown = true;
		
		if (waypointState == enWaypointState.Active) {
			for (int i=0; i<wayActors.Length; i++) {
				if (!wayActors[i].actor.bIsDead)
					bAllDown = false;
			}
			
			if (bAllDown) { //we've cleared this node
				waypointState = enWaypointState.Done;
				
				//call back to our main system and tell it that this is done...
				LevelController.Instance.waypointCallback(gameObject);
			}
		}
	}

}
