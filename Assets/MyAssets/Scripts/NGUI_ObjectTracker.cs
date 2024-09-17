using UnityEngine;
using System.Collections;

//we're extending this a little to be able to handle anything that's positioned on screen
public class NGUI_ObjectTracker : MonoBehaviour {
	
	public Camera NGUICamera, gameCamera;
	public GameObject trackingObject; //should be private I suppose...

	public virtual GameObject trackObject {
		get { return trackingObject; }
		set { trackingObject = value; }
	}

	//float width=480, height = 360;
	// Update is called once per frame

	void LateUpdate () {
		if (trackingObject!=null)
			positionObject();
		else { //we need to annull this objects visibility
			foreach (Transform thisTran in transform)
				thisTran.gameObject.SetActive(false);
		}
	}
	
	
	//position our sight visually.
	public void positionObject() {
		// Get screen location of node
		Vector3 screenPos = gameCamera.WorldToScreenPoint(trackingObject.transform.position);


		foreach (Transform thisTran in transform)
			thisTran.gameObject.SetActive(screenPos.z > 0);

		screenPos.z = 0F;

		// Move to node
		transform.position = NGUICamera.ScreenToWorldPoint(screenPos);
	}
}
