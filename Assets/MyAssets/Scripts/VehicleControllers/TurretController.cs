using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//The controller for the turret that'll be attached to a bomber, first for the player to control, and then under AI control (or something like it I guess)
//This seems to have to extend from Actor, which means that it might end up being something that can be damaged/take hits instead of the base aircraft
public class TurretController : Actor {

	public GameObject[] CannonEmpties; //these are the empties attached to our aircraft that denote cannons
	public AttachedGun[] ourGunMP;
	public ActorController ourPlayerController; //we get a feed from this sent through to the UpdateInput.

	public float cannon_refire = 0.2f;
	float lastCannonFireTime = 0;
	int cannon_current = 0;

	GameObject ourCameraObject;

	public float crusingSpeed = 3;      //It's important that this is the same as the aircraft we're riding on as the AI uses the player object as a reference
										//public float turnspeed = 1.5F;      //Used by the AI for doing turns
										//public float turnspeed = 1.5F;
										//public float turnspeed = 1.5F;       //Used by the AI for doing turns



	// Use this for initialization
	public override void DoStart()
	{
		base.DoStart();
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
	}

	private float ClampAngle(float angle, float min, float max)
	{
		angle = (angle > 180f) ? angle - 360f : angle;
		return Mathf.Clamp(angle, min, max);
	}

	bool bCanFireCannons = true;

	void PlayerUpdate()
	{
		pitch = Mathf.Lerp(pitch, targetPitch, Time.deltaTime * controlResponce);
		roll = Mathf.Lerp(roll, targetRoll, Time.deltaTime * controlResponce);
		yaw = Mathf.Lerp(yaw, targetYaw, Time.deltaTime * controlResponce);


		transform.RotateAround(transform.parent.transform.up, roll * turnspeed * Time.deltaTime);
		transform.RotateAround(ourCameraObject.transform.right, pitch * turnspeed * Time.deltaTime);

		//Problematically we're dealing with that 360 problem again
		transform.localEulerAngles = new Vector3(ClampAngle(transform.localEulerAngles.x, -60, 30), Mathf.Clamp(transform.localEulerAngles.y, 10, 230f), transform.localEulerAngles.z);

		//And our firing logic (this broke!)
		if (bFiring && Time.time > lastCannonFireTime + cannon_refire && bCanFireCannons)
		{
			lastCannonFireTime = Time.time;
			ourGunMP[cannon_current].doFireEffect();
			cannon_current++;
			cannon_current = cannon_current % ourGunMP.Length;
		}
	}

	public override void DoUpdateInternalSettings()
	{
		//Mainly because I'll want to override for different vehicles
		//float controllerSoftnessValue = UISettingsHandler.Instance.getSettingFloat("turret_look_softness");
		//controllerSoftness = Mathf.Lerp(70f, 10f, controllerSoftnessValue);
		//Debug.Log("Controller Softness: " + controllerSoftness);
		yAxisBias = UISettingsHandler.Instance.getSettingInt("turret_look_inversion") == 0 ? -1 : 1;

		bStickControlRight = UISettingsHandler.Instance.getSettingInt("turret_control_handedness") == 0;

		bTriggerControlRight = UISettingsHandler.Instance.getSettingInt("turret_trigger_handedness") == 0;
	}
}
