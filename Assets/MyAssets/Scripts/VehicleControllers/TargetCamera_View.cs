using UnityEngine;
using System.Collections;

//This camera script tracks a target for us.
//It's seriously cool but might be too consumptive?
public class TargetCamera_View : MonoBehaviour {
	//it'll have to be just a little bit lazy
	public GameObject selectedTarget;
	public float fovFactor = 1;
	float targetLerp=0; //pan to this target

	public void setTarget(GameObject thisTarget) {
		selectedTarget = thisTarget; //this is what we want to follow
		targetLerp = 1; //so that we can guide to the next one
	}

	// Update is called once per frame
	void LateUpdate () {

		if (PlayerController.Instance!=null) {
			transform.position = PlayerController.Instance.ourAircraft.transform.position; //set this location so that camera functions
		}

		//our tracking script:
		if (selectedTarget) {
			transform.LookAt(selectedTarget.transform.position);
			float viewDistance = selectedTarget.GetComponent<Actor>().targetViewDistance;
			//finally our fov stuff
			//GetComponent<Camera>().fieldOfView = fovFactor/(selectedTarget.transform.position - transform.position).magnitude;
			gameObject.transform.position = selectedTarget.transform.position - transform.forward * viewDistance;
		}
	}
}
