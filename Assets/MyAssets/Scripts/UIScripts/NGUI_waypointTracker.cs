using UnityEngine;
using System.Collections;

public class NGUI_waypointTracker : NGUI_ObjectTracker {
	void LateUpdate () {
		if (trackingObject!=null && PlayerController.Instance.target == null) {
			positionObject();
			//foreach (Transform thisTran in transform)
			//	thisTran.gameObject.SetActive(true);

		}
		else { //we need to annull this objects visibility
			foreach (Transform thisTran in transform)
				thisTran.gameObject.SetActive(false);
		}
	}
}
