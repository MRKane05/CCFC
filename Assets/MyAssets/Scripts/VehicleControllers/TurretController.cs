using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//The controller for the turret that'll be attached to a bomber, first for the player to control, and then under AI control (or something like it I guess)
//This seems to have to extend from Actor, which means that it might end up being something that can be damaged/take hits instead of the base aircraft
public class TurretController : Actor {

	public GameObject[] CannonEmpties; //these are the empties attached to our aircraft that denote cannons
	public AttachedGun[] ourGunMP;
	public ActorController ourPlayerController; //we get a feed from this sent through to the UpdateInput.

	GameObject ourCameraObject;

	public float crusingSpeed = 3;		//It's important that this is the same as the aircraft we're riding on as the AI uses the player object as a reference
	//public float rollspeed = 1.5F;      //Used by the AI for doing turns
	//public float pitchspeed = 1.5F;
	//public float yawspeed = 1.5F;       //Used by the AI for doing turns

	

	// Use this for initialization
	void Start () {
		doTurretSetup();
		speed = crusingSpeed;
	}

	void doTurretSetup()
    {
		ourCameraObject = Camera.main.gameObject; //used for arcade controls.
	}
	
	// Update is called once per frame
	void Update () {
		PlayerUpdate();
		if (bFiring != bLastFiring)
		{
			bLastFiring = bFiring;
			for (int i = 0; i < ourGunMP.Length; i++)
			{
				if (ourGunMP[i] != null)
				{
					ourGunMP[i].bFiring = bFiring;
					ourGunMP[i].FireState = FireState; //Will probably take over from bFiring
				}
			}
		}
	}

	private float ClampAngle(float angle, float min, float max)
	{
		angle = (angle > 180f) ? angle - 360f : angle;
		return Mathf.Clamp(angle, min, max);
	}

	void PlayerUpdate()
	{

		transform.RotateAround(transform.parent.transform.up, roll * rollspeed * Time.deltaTime);
		transform.RotateAround(ourCameraObject.transform.right, pitch * pitchspeed * Time.deltaTime);

		//Problematically we're dealing with that 360 problem again
		transform.localEulerAngles = new Vector3(ClampAngle(transform.localEulerAngles.x, -60, 30), Mathf.Clamp(transform.localEulerAngles.y, 10, 230f), transform.localEulerAngles.z);

	}
}
