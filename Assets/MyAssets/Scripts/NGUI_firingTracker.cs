using UnityEngine;
using System.Collections;

public class NGUI_firingTracker : NGUI_ObjectTracker {

	GameObject playerAircraft;
	Actor targetActor;
	//Might want to check the player aircraft on startup and get an average bullet speed from the attached guns
	float bulletSpeed=50F; //this is becoming pretty uniform, but might be different with different guns (potentially)

	//there's a little bit of extra maths that goes on with this (potentially)
	public override GameObject trackObject {
		get { return trackingObject; }
		set { trackingObject = value;
			if (trackingObject!=null)
				targetActor = trackingObject.GetComponent<Actor>(); //I thik it'll always be here, although sometimes we pull an error here
			else
				targetActor = null;
		}
	}

	// Use this for initialization
	IEnumerator Start () {
		//playerAircraft = GameObject.Find("PlayerAircraft");
		while (PlayerController.Instance == null)
			yield return null;
		
		playerAircraft = PlayerController.Instance.ourAircraft.gameObject;
		
	}

	void LateUpdate () {
		if (trackingObject!=null && PlayerController.Instance!=null)
			targetPositionObject();
		else { //we need to annull this objects visibility
			foreach (Transform thisTran in transform)
				thisTran.gameObject.SetActive(false);
		}
	}


	Vector3 mTargetPosition = Vector3.zero;

	public Vector3 targetPosition { //our aiming forward vector position (on a good day...)
		get { return mTargetPosition; }
		set { mTargetPosition = value; }
	}
	
	//position our sight visually.
	public void targetPositionObject() {
		// Get screen location of node
		//get our location stuff

		if (targetActor!=null) { //sit down and sort out our position that we should be shooting at
			mTargetPosition = trackingObject.transform.position + trackingObject.transform.forward*(playerAircraft.transform.position-trackingObject.transform.position).magnitude*targetActor.getSpeed()/bulletSpeed;
		} else {
			mTargetPosition = trackingObject.transform.position; //For when we don't have an actor attached to the target.
		}

		Vector3 screenPos = gameCamera.WorldToScreenPoint(mTargetPosition);

		foreach (Transform thisTran in transform) {
			thisTran.gameObject.SetActive(screenPos.z > 0);
		}
		
		screenPos.z = 0F;
		
		// Move to node
		transform.position = NGUICamera.ScreenToWorldPoint(screenPos);
	}


}
