using UnityEngine;
using System.Collections;

//Just what we need, to make another one of these...
public class CameraController : MonoBehaviour {
	public GameObject viewCenter, gunSight, ownerAircraft; //similar to target center.
	public float lookSpring=3, lookSnap=6;
	public float StartDistance=7; //should actually calculate this off the bounds size of what we're looking at
	public float viewElevation=1;
	private Quaternion lookRotation;
	// Use this for initialization

	int ChaseCameraType = 2;
	Vector3 lerpPosition = Vector3.zero; //Used for our locked camera

	void Start () {
		lerpPosition = gameObject.transform.position;
		UISettingsHandler.Instance.OnSettingsChanged.AddListener(UpdateInternalSettings);
		UpdateInternalSettings();
	}

	void UpdateInternalSettings()
	{
		lerpPosition = gameObject.transform.position;
		ChaseCameraType = UISettingsHandler.Instance.getSettingInt("flight_cameratype");
	}

	void OnDestroy()
    {
		UISettingsHandler.Instance.OnSettingsChanged.RemoveListener(UpdateInternalSettings);
	}

	public void kickCamera(float amount) { //add a kick to the camera in a random direction with degree amount

	}
	

	// Update is called once per frame
	void LateUpdate () {

		if (ownerAircraft)
		{

			switch (ChaseCameraType)
			{
				case 0:	//Chase camera
					//transform.eulerAngles = new Vector3(Mathf.LerpAngle(transform.eulerAngles[0], ownerAircraft.transform.eulerAngles[0]+viewAngleOffset, Time.deltaTime*lookSpring), Mathf.LerpAngle(transform.eulerAngles[1], ownerAircraft.transform.eulerAngles[1], Time.deltaTime*lookSpring), Mathf.LerpAngle(transform.eulerAngles[2], ownerAircraft.transform.eulerAngles[2], Time.deltaTime*lookSpring));
					//This is causing our jagged movement. Needs to be lerped also, or somehow smoothed
					lookRotation = transform.rotation;

					transform.LookAt(viewCenter.transform.position, ownerAircraft.transform.up); //always look at this target.

					transform.rotation = Quaternion.Lerp(lookRotation, transform.rotation, Time.deltaTime * lookSnap);
					//should do this as a lerp
					transform.position = Vector3.Lerp(transform.position, viewCenter.transform.position - ownerAircraft.transform.forward.normalized * StartDistance + transform.up * viewElevation, Time.deltaTime * lookSpring);
					break;
				case 1:	//Targetsight camera
					transform.position = Vector3.Lerp(transform.position, viewCenter.transform.position - ownerAircraft.transform.forward.normalized * StartDistance + transform.up * viewElevation, Time.deltaTime * lookSpring);
					if (gunSight)
					{
						transform.LookAt(gunSight.transform.position, transform.up);   //Oddly this isn't looking right at this point and there's some drift?
					} else
                    {
						transform.LookAt(viewCenter.transform.position, transform.up);
					}
					break;
				case 2:	//Locked camera
					Quaternion baseLookRotation = Quaternion.LookRotation(viewCenter.transform.position - ownerAircraft.transform.position, ownerAircraft.transform.up);
					Quaternion offsetLookRotation = Quaternion.AngleAxis(7, gameObject.transform.right) * baseLookRotation;

					//We need something to move in and out for our speed...
					lerpPosition = Vector3.Lerp(lerpPosition, viewCenter.transform.position - ownerAircraft.transform.forward.normalized * StartDistance + transform.up * viewElevation, Time.deltaTime * lookSpring);

					transform.position = viewCenter.transform.position - offsetLookRotation * Vector3.forward * Vector3.Distance(lerpPosition, viewCenter.transform.position);
					transform.LookAt(viewCenter.transform, transform.up);

					break;
			}
		}
		else if (PlayerController.Instance)
		{ //so this will setup if we're cold starting
			ownerAircraft = PlayerController.Instance.ourAircraft.gameObject;
			viewCenter = PlayerController.Instance.ourAircraft.viewCenter;
		}
	}
}
