using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SecondaryWeaponAttachment
{
	public string SelectedSecondary = "";
	public GameObject AssociatedSecondary;
}

public class AircraftController : Actor {
	//our overall scale is 1unit=5m
	//kmph to mps conversion: 0.0555555555 aka 0.5/9


	//We need a tweak that bleeds speed off if we're going up, and increases it as we're going down.
	//public GUIText ourGUIText;
	[Header("Weapon Stuff")]
	public GameObject[] CannonEmpties; //these are the empties attached to our aircraft that denote cannons
	public AttachedGun[] ourGunMP;
	public List<SecondaryWeaponAttachment> AvaliableSecondaryWeapons = new List<SecondaryWeaponAttachment>();
	public SecondaryWeapon_Base attachedSecondary;
	public float cannon_refire = 0.2f;
	float lastCannonFireTime = 0;
	int cannon_current = 0;
	[Space]
	public ActorController ourPlayerController; //we get a feed from this sent through to the UpdateInput.
												//public AIController ourAIController; //we can also get information from this
												//public float pitch, roll, yaw, throttleControl; //treated differently depending on the control scheme
												//public bool bControlArcade=false;
												//these wil
	#region Aircraft Properties For Player Control
	public float mass = 7;
	public float OverdriveAirSpeed = 10f;
	public float MaxAirSpeed = 6f;
	public float SlowAirSpeed = 2f;

	public float StallSpeed = 1.5F;
	//	public float turnspeed = 1.5F;		//Used by the AI for doing turns
	//	public float turnspeed = 1.5F;
	//	public float turnspeed = 1.5F;		//Used by the AI for doing turns
	public float throttlespeed = 0.5F;
	#endregion

	float throttle = 0.75F, gravity = 7, thrustPower = 30f, throttleAcceleration = 0.5F; //this is a consideration on our AirSpeed, this is what we should be tending toward given that the likes of gravity etc. aren't acting upon us...
	float thrust = 8;
	float enginePitchMin = 0.6f, enginePitchMax = 1.4f;
	Engine[] engines; //well this is either as simple or as complicated as we want to make it really...

	//d_ functions (death)
	float d_time = float.MaxValue; //the point where we'll go bang (as set when damage is taken)

	public GameObject AircraftModel, AircraftMesh, FiringMarker;
	//int FireState;
	public bool isNPC = true;
	GameObject ourCameraObject;
	Vector3 GravityVector;

	Material ourMat;

	Vector3 impactJolt; //we've hit something and this will affect our velocity
	float impactSpin;
	float joltDecay = 3; //how much does this jolt "bleed out" after impact

	//AI end stuff
	Quaternion Rotation;

	//public Collider groundCollider; //our ground plane that we could be contacting with.

	//public float health=100F, maxHealth = 100F;
	//really this should be at the actor level
	public override void gradeSkill(float factor) {
		health *= factor;
		maxHealth *= factor;

		foreach (AttachedGun thisGun in ourGunMP) {
			if (bIsPlayerVehicle)
            {
				thisGun.damage = gameManager.Instance.SelectedAircraft.AttachedCannons.cannons_damage;  //Set our cannons damage as based on what the player selected
				thisGun.spread = gameManager.Instance.SelectedAircraft.AttachedCannons.cannons_spread;
				thisGun.autoAimAngle = gameManager.Instance.SelectedAircraft.AttachedCannons.cannons_autoaim_angle;
			}

			thisGun.damage *= factor;
		}
	}

	
	//Not sure about the "after effect" here as it needs to fade or sink into the ground or something so we
	//don't chew up too much memory
	void doDitch() { //is called when our aircraft ditches
					 //Need to strip out anything that'd be using process, and leave the main system.
		foreach (GameObject thisCannon in CannonEmpties)
			Destroy(thisCannon); //remove our guns.

		//we'll need to tweak the smoke so that it's rising now instead of trailing.
		((LevelController)LevelControllerBase.Instance).removeSelf(gameObject, team); //For the moment I suppose
		Destroy(this); //remove this script.

		//we also need to remove from the 
	}

	//we're getting a collision called from our base classes, handle accordingly
	//make sure we're not colliding with a target sphere!
	public override void doCollision(Collider withThis) {

		//Lets see if we've got a pickup! Because then we'll know what to do with it!
		PickupBase potentialPickup = withThis.gameObject.GetComponent<PickupBase>();
		if (potentialPickup)
        {
			DoCollectPickup(withThis.gameObject);
			return; //Don't do anything else here
		}

		//Do a jolt from the collision which will be added to the velocity
		impactJolt = (transform.position - withThis.gameObject.transform.position); //get our impact offset
																					//impactJolt -= transform.forward;
																					//impactJolt = impactJolt;

		//from the two there we need the quotents from transform.up and transform.right.
		Vector3 resultingJolt = transform.right * Vector3.Dot(transform.right, impactJolt);
		resultingJolt += transform.up * Vector3.Dot(transform.up, impactJolt);


		float joltHit = mass; //so for the moment that'll work it I suppose, but we need to consider the thing we've hit also

		impactJolt = resultingJolt.normalized * joltHit;

		//check if this is a ground collider and if it is then put more punch up.
		LayerMask groundLayer = LayerMask.NameToLayer("Ground");
		if (withThis.gameObject.layer == 14)
		{
			Debug.LogError("Hit Ground");
			//need to make sure that this is tangental to the ground.
			Ray ray = new Ray(transform.position, -Vector3.up); //shoot this ray down to see where we contact

			RaycastHit hit;

			//float contactOffset = gameObject.collider.bounds.center.y + gameObject.collider.bounds.extents.y;
			//Debug.Log ("Contact offset: " + gameObject.collider.bounds.min);

			if (withThis.Raycast(ray, out hit, 5)) {
				transform.position = new Vector3(transform.position[0], Mathf.Max(hit.point[1] + AircraftModel.GetComponent<Collider>().bounds.extents.y, transform.position[1]), transform.position[2]);
				impactJolt = hit.normal * mass; //jolt up at the angle of the terrain

				//I'm not sure the above ditch rule is fair enough...
				if (bIsDead) { //handle what happens when we hit.
					if (Vector3.Dot(hit.normal, AircraftModel.transform.up) > 0.75F)
						doDitch();
					else
						doExplode(0f);  //Die on contact
				}
				else {
					if (Vector3.Dot(hit.normal, AircraftModel.transform.up) > 0.9F) { //check to see that we're tangential to our ground (as in not a crash)
																					  //this is a collision with our gear down. Don't take damage, and if we're shot down this is a ditch

					}
					else { //this collision should cause damage to our vehicle
						   //not sure if it should be constant, or graded according to angle stuff, or what, for the moment...
						takeDamage(maxHealth / 7F, "COLLISION", gameObject, team, 0F); //do a collision with the ground.
					}
				}
			}
			else { //how the hell did we hit the ground? nevermind just put something in
				impactJolt = Vector3.Lerp(resultingJolt, -Vector3.up * mass, 0.5F);

				takeDamage(maxHealth / 7F, "COLLISION", gameObject, team, 0F);
			}

			//Do our ground collision suff. Damage etc...

		}
		//Debug.Log (impactJolt);

		if (impactJolt.magnitude < 0.5F) //what happens when we strike square on?
			impactJolt = (transform.right * Random.Range(0F, 1F) + transform.up * Random.Range(0F, 1F)) * joltHit;

		//We need to spin this fighter a little
		//So how to translate this into spin?
		//Need to look at positional stuff to see if it's left or right
		impactSpin = Vector3.Dot(transform.right, impactJolt) * 1.25F; //This works well

		//we need to sort out the damage side of things here...

	}

	public override void DoImpactJolt(Vector3 centroid, float velocity)
    {
		impactJolt = (transform.position - centroid); //get our impact offset

																					//from the two there we need the quotents from transform.up and transform.right.
		Vector3 resultingJolt = transform.right * Vector3.Dot(transform.right, impactJolt);
		resultingJolt += transform.up * Vector3.Dot(transform.up, impactJolt);

		impactJolt = resultingJolt.normalized * velocity;

		//We need to spin this fighter a little
		//So how to translate this into spin?
		//Need to look at positional stuff to see if it's left or right
		impactSpin = Vector3.Dot(transform.right, impactJolt) * 1.25F; //This works well
	}

	// Use this for initialization, but it wipes out what we did at the base level...
	public override void DoStart()
	{
		base.DoStart();
		doVehicleSetup();
		if (ourPlayerController)
			ourPlayerController.AddHealth(health);

		if (bIsPlayerVehicle) 
			GrabPlayerVehicleStats();
	}

	public void GrabPlayerVehicleStats()
    {
		//PROBLEM: This setup is a mess an sorely needs refactored

		//Apply our modified stats to our aircraft for play
		//This will also have to go through selecting the fighter model, assigning the cannons, and assigning the secondary weapon/setting up the UI
		//For the moment...
		maxHealth = gameManager.Instance.SelectedAircraft.armor_current;
		health = maxHealth; //of course for missions that are played concessively this won't fly...
		turnspeed = gameManager.Instance.SelectedAircraft.agility_current;
		thrustPower = gameManager.Instance.SelectedAircraft.accel_current;
		MaxAirSpeed = gameManager.Instance.SelectedAircraft.speed_current;
		OverdriveAirSpeed = MaxAirSpeed * 10f / 6f; //Stand in constant from previously

		cannon_refire = gameManager.Instance.SelectedAircraft.AttachedCannons.cannons_refire_time;

		attachedSecondary = null; //Clear this ahead of setting it correctly
		//PROBLEM: This needs to be a much cleaner implementation, but does open up the possibility of changing secondaries mid-game
		//Handle the secondary weapon
		if (gameManager.Instance.SelectedAircraft.secondary_weapon_name.Length > 4)	//Because "null" is 4 letters!
        {
			for (int i= 0; i< AvaliableSecondaryWeapons.Count; i++)
            {
				if (AvaliableSecondaryWeapons[i].SelectedSecondary == gameManager.Instance.SelectedAircraft.secondary_weapon_name)
                {
					AvaliableSecondaryWeapons[i].AssociatedSecondary.SetActive(true);
					attachedSecondary = AvaliableSecondaryWeapons[i].AssociatedSecondary.GetComponent<SecondaryWeapon_Base>();
				} else
                {
					AvaliableSecondaryWeapons[i].AssociatedSecondary.SetActive(false);
				}
            }
        }
	}

	public virtual void doVehicleSetup() {

		//because the parent script stuff isn't working...?
		hitEffect = gameObject.GetComponentInChildren<Emitter_Hit>();
		smokeEffects = gameObject.GetComponentsInChildren<Emitter_Smoke>();

		//thrust should be calculated as the maximum angle the aircraft can climb without loosing altitude...
		thrust = Mathf.Sin(thrustPower * Mathf.Deg2Rad) * gravity; //although this is also a measure of the "power" of an engine...
		speed = MaxAirSpeed * throttle; //get this setup for the kickoff.
		ourCameraObject = Camera.main.gameObject; //used for arcade controls.

		ourGunMP = new AttachedGun[CannonEmpties.Length];
		for (int i=0; i<ourGunMP.Length; i++) {
			ourGunMP[i]=CannonEmpties[i].GetComponent<AttachedGun>();
			ourGunMP[i].baseVehicle = gameObject;
			ourGunMP[i].setController(this);
		}

		engines = gameObject.GetComponentsInChildren<Engine>(); //gets all of these...

		//set up our stuff for the textures etc.
		//really we should have only one material...
		if (AircraftMesh)
			ourMat = AircraftMesh.GetComponent<Renderer>().material; //and make a dupe of it
	}

	float PlayerSpeedChange(float targetSpeed, bool bDoingBoost)
	{
		//Debug.Log("Speed: " + speed + " targetSpeed: " + targetSpeed);
		//thrust, gravity
		GravityVector = transform.forward.normalized;
		float SpeedChange = 0;
		if (bDoingBoost)	//Ignore gravity while we're boosting
		{
			SpeedChange = speed < OverdriveAirSpeed ? thrust * 3f : -thrust * 3f;
		}
		else
		{
			//This needs to take into account overdrive and drag
			if (speed > targetSpeed)
			{ //then we're wanting to go slower.
				SpeedChange = (-thrust*2f - Mathf.Abs(GravityVector.y) * GravityVector.y * gravity) / mass;
			}
			else
			{ //So I suppose this is to accellerate...
				SpeedChange = (thrust - Mathf.Abs(GravityVector.y) * GravityVector.y * gravity) / mass;
			}
		}

		return SpeedChange * 4f;	//Just try multiplying it to see if it'll be more responsive
	}

	float SpeedChange() {
		//thrust, gravity
		GravityVector = transform.forward.normalized;

		//this does mean that we can over power our system...
		if (speed > MaxAirSpeed*throttle) { //then we're wanting to go slower.
			return (-thrust-Mathf.Abs(GravityVector.y)*GravityVector.y*gravity)/mass;
		}
		else { //So I suppose this is to accellerate...
			return (thrust-Mathf.Abs(GravityVector.y)*GravityVector.y*gravity)/mass;
		}
		return 0;
	}
	
	//I really need to refactor this class...
	public override void takeDamage(float thisDamage, string damageType, GameObject instigator, int team, float delay) {
		//This needs a super...
		if (owner)	//Need to figure out why this isn't being set
			owner.actorTakingDamage(owner, this, health / maxHealth);
		//Make this invincible for the moment...
		if (!isInvincible) {
			health -= thisDamage;
			if (health<0 && !bIsDead) {
				//need to check and see if this is the player, and if it is then add the score up.
				if (instigator == PlayerController.Instance.ourAircraft.gameObject)
					gameManager.Instance.addKill(vehicleType, vehicleScore); //rack it up!

				//doExplode(); //whack this.
				bIsDead=true;
				applyShotDown();
			}
		}
		
		//Need to tell our controller that we're taking damage...
		if (ourPlayerController) //well this is a moot statement isn't it?
			ourPlayerController.TakeDamage(thisDamage, damageType, instigator, delay);

		checkSmokeSystem(health/maxHealth);

		//need to think about a different edge for this with collisions with the ground actually. Perhaps
		//the damage stuff should have a reccomended star thing or similar.
		if (hitEffect)
			hitEffect.addHit(delay); //for actually seeing that we're hit!
		
	}
	
	// Update is called once per frame
	void Update () {

		if (ourMat) { //This is great apart from the target camera in which it looks a little odd...
			if (bIsTarget) {
				ourMat.SetColor ("_OutlineColor", Color.red);
			}
			else { //set the outline black...
				ourMat.SetColor ("_OutlineColor", Color.black);
			}
		}

		//Shoot our guns
		if (bFiring && Time.time > lastCannonFireTime + cannon_refire)
        {
			lastCannonFireTime = Time.time;
			ourGunMP[cannon_current].doFireEffect();
			cannon_current++;
			cannon_current = cannon_current % ourGunMP.Length;
        }

		if (isNPC)
		{
			//Adjust throttle...
			throttle = Mathf.Clamp01(throttle + throttleControl * throttlespeed * Time.deltaTime); //for the moment keep it conformed.
																								   //This calculates our speed through the air dependend upon our angles etc.
			speed = Mathf.Clamp(speed + SpeedChange() * Time.deltaTime, 0, MaxAirSpeed * 1.2F);
		} else
        {
			float targetSpeed = MaxAirSpeed; //we need something to get the speed of our target
			
			if (ourPlayerController.target != null)
            {
				float EngageDistance = 10f;
				if (Vector3.SqrMagnitude(gameObject.transform.position - ourPlayerController.target.transform.position) < EngageDistance * EngageDistance)
                {
					//Debug.Log("Within engage distance");
					Actor targetActor = ourPlayerController.target.GetComponent<Actor>();
					if (targetActor)
                    {
						targetSpeed = targetActor.getSpeed();
						if (targetSpeed == 0) {  //we're dealing with a stationary target
							targetSpeed = MaxAirSpeed;
						}
                    }
                }
            }

			if (throttleControl < -0.5f)
            {
				targetSpeed = SlowAirSpeed;
            }
			speed = Mathf.Clamp(speed + PlayerSpeedChange(targetSpeed, throttleControl > 0.5f) * Time.deltaTime, 0, OverdriveAirSpeed);	//Why do we have the 1.2f here?
		}
		

		for (int i=0; i< engines.Length; i++) {
			engines[i].setPitch(((speed-StallSpeed)/((MaxAirSpeed-StallSpeed))+throttle)*0.5f); //almost doesn't seem dynamic enough...
		}

		//Handle our aircraft control - ie turning.
		if (!isNPC) {
			PlayerUpdate();
		}
		else {
			NPCUpdate();			
		}
		
		if (speed<StallSpeed) {
			//this needs to be more natural
			if (transform.localRotation.eulerAngles.z < 90F || transform.localRotation.eulerAngles.z > 270)
				transform.RotateAround(transform.right, turnspeed*Time.deltaTime*((StallSpeed-speed)/StallSpeed)*2);
			else
				transform.RotateAround(transform.right, -turnspeed*Time.deltaTime*((StallSpeed-speed)/StallSpeed)*2);
		}
		//Debug.Log (transform.localRotation.eulerAngles);
		//Shoot our aircraft down...
		if (bIsDead) {
			AircraftDeathSpiral();			
		}
		
		impactJolt = Vector3.Lerp(impactJolt, Vector3.zero, Time.deltaTime*joltDecay);
		impactSpin = Mathf.Lerp (impactSpin, 0, Time.deltaTime*joltDecay);
		
		//and the direction of travel for the aircraft. Later this will need to be cut against gravity, but for the moment this'll work
		transform.position+=transform.forward*Time.deltaTime*speed;

		//handle our jolt stuff
		transform.position+=impactJolt*Time.deltaTime; //handle out hit.
		transform.RotateAround(transform.forward, -impactSpin*Time.deltaTime);

		/*
		if (groundCollider && bIsPlayerVehicle) //The new collisions system should handle this
			checkGroundContacts();*/
	
	}
	
	void NPCUpdate() {
		//Rotates us around the world origin...
		//Quaternion currentRotation = transform.rotation;
		//While it's awesome to do this we need to think about how the aircraft is going to behave...they bank while
		//turning and we need to emulate that here as this isn't a real sim...
			
		//Basically we want to always be facing "up" with the direction of turn.
			
		transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnspeed); 
		AircraftModel.transform.localEulerAngles = new Vector3(0,0,Mathf.LerpAngle(AircraftModel.transform.localEulerAngles[2], -roll*90, (turnspeed)*Time.deltaTime));
			
		//for our ground stuff...

	}

	//this works fine for doing player ground contact but it's a bit much for the AI I think.
	public void checkGroundContacts() {
		//transform.position = with
		//Debug.LogError("Terrain Check");
		//if (withThis.name == "Terrain") { //or is there a better way?
		//do we want to use a Collider.ClosestPointOnBounds to somehow reverse calculate our height or
		//some thing different? I suppose it'll have a bounce if we're coming in at an odd angle...
		//Vector3 terrainLocation = Vector3.zero;
		
		Ray ray = new Ray(transform.position + Vector3.up * 10f, -Vector3.up); //shoot this ray down to see where we contact
		
		RaycastHit hit;

		//float contactOffset = gameObject.collider.bounds.center.y + gameObject.collider.bounds.extents.y;
		//Debug.Log ("Contact offset: " + gameObject.collider.bounds.min);
		//not sure about ditching here, but for the moment this should work
		/*
		if (groundCollider.Raycast (ray, out hit, 5)) {
			//don't update the transform position if the normal from the ground is too dissimilar to the
			//normal from the player aircraft's up vector
			//Debug.Log ("Dot Normals: " + Vector3.Dot (hit.normal, AircraftModel.transform.up));
			if (Vector3.Dot (hit.normal, AircraftModel.transform.up) > 0.9F) { //check to see that we're tangential to our ground (as in not a crash)
				transform.position = new Vector3(transform.position[0], Mathf.Max (hit.point[1] + AircraftModel.GetComponent<Collider>().bounds.extents.y, transform.position[1]), transform.position[2]);
				Debug.Log("Doing Player Bump Reposition");
			}
		}*/
		//}

	}

	//returns (unrealistically) our altitude above the current terrain
	
	public float checkAltitude() {
		//This needs to be a raycast down against the ground channel
		/*
		if (!groundCollider) { return 100;  }
		Ray ray = new Ray(transform.position, -Vector3.up); //shoot this ray down to see where we contact
		
		RaycastHit hit;

		if (groundCollider.Raycast(ray, out hit, float.MaxValue)) {
			return hit.distance;
		}
		*/
		return -1f; //no update...
	}

	void PlayerUpdate() {
		pitch = Mathf.Lerp(pitch, targetPitch, Time.deltaTime * controlResponce);
		roll = Mathf.Lerp(roll, targetRoll, Time.deltaTime * controlResponce);
		yaw = Mathf.Lerp(yaw, targetYaw, Time.deltaTime * controlResponce);

		if (!ourPlayerController.bControlArcade) { //control the pitch, roll, yaw of the vehicle.
			//roll commands
			transform.RotateAroundLocal(transform.forward, -roll*turnspeed*Time.deltaTime);
			//pitch commands
			transform.RotateAroundLocal(transform.right, pitch*turnspeed*Time.deltaTime);
			//yaw commands
			transform.RotateAroundLocal(transform.up, yaw*turnspeed*Time.deltaTime);
			//throttle details
		}
		else { //we want to control relitive to where we think it should be going
			//this is a camera relitive control.
			transform.RotateAround(ourCameraObject.transform.up, roll*turnspeed*Time.deltaTime);
			transform.RotateAround(ourCameraObject.transform.right, pitch*turnspeed*Time.deltaTime);
			
			//command to roll us back up the proper way.
			transform.RotateAroundLocal(transform.forward, -yaw*turnspeed*Time.deltaTime);
			//Need to figure out what up is...
			
			//not sure what to do about yaw...

			//Aircraft interpertation section of this controller.
			//if we're pulling left or right we should rotate to the relitive 90...
			AircraftModel.transform.localEulerAngles = new Vector3(0,0,Mathf.LerpAngle(AircraftModel.transform.localEulerAngles[2], -roll*90, (turnspeed)*Time.deltaTime));
		}
	}

	public override GameObject getModel() {
		return AircraftModel; //it really is that simple
	}

	public override void DoUpdateInternalSettings()
	{
		//Mainly because I'll want to override for different vehicles
		//float controllerSoftnessValue = UISettingsHandler.Instance.getSettingFloat("flight_look_softness");
		//controllerSoftness = Mathf.Lerp(70f, 10f, controllerSoftnessValue);

		yAxisBias = UISettingsHandler.Instance.getSettingInt("flight_look_inversion") == 0 ? -1 : 1;

		bStickControlRight = UISettingsHandler.Instance.getSettingInt("flight_control_handedness") == 0;

		bTriggerControlRight = UISettingsHandler.Instance.getSettingInt("flight_trigger_handedness") == 0;
	}

	public override void DoCollectPickup(GameObject targetPickup)
	{
		PickupBase thisPickup = targetPickup.GetComponent<PickupBase>();
		if (thisPickup.Instigator == gameObject){ return; }
		switch (thisPickup.PickupType)
        {
			case PickupBase.enPickupType.HEALTH:
				health = Mathf.Clamp(health + thisPickup.PickupValue, 0f, maxHealth);
				if (ourPlayerController)
					ourPlayerController.AddHealth(health);
				//Some sound
				//Some effect
				break;
        }
		Destroy(targetPickup);
    }

	public override void TapSecondary() {
		if (attachedSecondary)
        {
			attachedSecondary.DoTapSecondary();
        }
	}
	public override void HoldDownSecondary() {
		if (attachedSecondary)
        {
			attachedSecondary.DoHoldSecondary();
        }
	}
}
