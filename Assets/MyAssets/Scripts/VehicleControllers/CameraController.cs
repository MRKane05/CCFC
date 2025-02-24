using UnityEngine;
using System.Collections;

//Just what we need, to make another one of these...
public class CameraController : MonoBehaviour {
	public GameObject viewCenter, ownerAircraft; //similar to target center.
	public float lookSpring=3, lookSnap=6;
	public float StartDistance=7; //should actually calculate this off the bounds size of what we're looking at
	public float viewElevation=1;
	private Quaternion lookRotation;
	// Use this for initialization
	
	void Start () {
		//this is fine apart from the fact that it's not good for iterative setups...
		//StartDistance = (transform.position-viewCenter.transform.position).magnitude;
	}

	public void kickCamera(float amount) { //add a kick to the camera in a random direction with degree amount

	}
	
	// Update is called once per frame
	void Update () {
		
		if (ownerAircraft) {
			//transform.eulerAngles = new Vector3(Mathf.LerpAngle(transform.eulerAngles[0], ownerAircraft.transform.eulerAngles[0]+viewAngleOffset, Time.deltaTime*lookSpring), Mathf.LerpAngle(transform.eulerAngles[1], ownerAircraft.transform.eulerAngles[1], Time.deltaTime*lookSpring), Mathf.LerpAngle(transform.eulerAngles[2], ownerAircraft.transform.eulerAngles[2], Time.deltaTime*lookSpring));
			//This is causing our jagged movement. Needs to be lerped also, or somehow smoothed
			lookRotation = transform.rotation;
			transform.LookAt(viewCenter.transform.position, ownerAircraft.transform.up); //always look at this target.
	
			transform.rotation = Quaternion.Lerp(lookRotation, transform.rotation, Time.deltaTime*lookSnap);
			//should do this as a lerp
			transform.position = Vector3.Lerp(transform.position, viewCenter.transform.position - ownerAircraft.transform.forward.normalized*StartDistance + transform.up*viewElevation, Time.deltaTime*lookSpring);
		}
		else if (PlayerController.Instance) { //so this will setup if we're cold starting
			ownerAircraft = PlayerController.Instance.ourAircraft.gameObject;
			viewCenter = PlayerController.Instance.ourAircraft.viewCenter;
		}
	}
}
