using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelControllerBomberMinigame : LevelControllerBase {

	public float GroundEndPoint = -37; //when our ground plane has moved below this point the level is considered complete
	public float totalScore = 0; //Because we want things that aren't targets to count against our bombing
	public GameObject groundObject;
	public List<GameObject> levelTargets = new List<GameObject>();  //What are our acutal targets?

	bool bLevelRunning = true;

	public int targetsSuccess = 0;

	public void ObjectDestroyed(GameObject thisObject, DestructableObject thisDestructable)
    {
		//PROBLEM: Need to add some sort of feedback for destroying this object
		if (levelTargets.Contains(thisObject))	//For a moment a quick checker to see if we're hitting the things that we need to hit
        {
			levelTargets.Remove(thisObject);
        }
    }

	void Update()
    {
		if (groundObject.transform.position.z < GroundEndPoint && bLevelRunning)
        {
			bLevelRunning = false;
			finishMatch(levelTargets.Count >= targetsSuccess); //because true is a fail
        }
    }
}
