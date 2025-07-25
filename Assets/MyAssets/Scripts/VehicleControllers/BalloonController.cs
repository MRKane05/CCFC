﻿using UnityEngine;
using System.Collections;

public class BalloonController : Actor {
	//our overall scale is 1unit=5m
	//kmph to mps conversion: 0.0555555555 aka 0.5/9
	
	
	//We need a tweak that bleeds speed off if we're going up, and increases it as we're going down.
	//public GUIText ourGUIText;
	
	public GameObject[] CannonEmpties; //these are the empties attached to our aircraft that denote cannons
	public AttachedGun[] ourGunMP;
	public ActorController ourPlayerController; //we get a feed from this sent through to the UpdateInput.
	//public AIController ourAIController; //we can also get information from this
	//public float pitch, roll, yaw, throttleControl; //treated differently depending on the control scheme
	//public bool bControlArcade=false;
	//these wil
	float mass=7, MaxAirSpeed=6, StallSpeed=1.5F, throttlespeed=0.5F;
	float throttle=0.75F, gravity=10, thrust=4, throttleAcceleration=0.5F; //this is a consideration on our AirSpeed, this is what we should be tending toward given that the likes of gravity etc. aren't acting upon us...
	public GameObject BalloonModel, AircraftMesh, FiringMarker;
	//int FireState;
	public bool isNPC=true;
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
		health*=factor;
		maxHealth*=factor;
		
		foreach (AttachedGun thisGun in ourGunMP) {
			thisGun.damage*=factor;
		}
	}
	
	void doExplode() { //is called with an explosive crash or when we're shot up enough
		if (explosionEffect) //can't go bang if we don't have a bang effect
			Instantiate(explosionEffect, transform.position, Quaternion.identity);
		
		//Explode and delete this in the process
		Destroy (gameObject); //just whack it for the moment
	}
	
	//we're getting a collision called from our base classes, handle accordingly
	//make sure we're not colliding with a target sphere!
	public override void doCollision(Collider withThis) {
		//Debug.LogError("Adding Impact Jolt");
		return; 
	}
	
	// Use this for initialization, but it wipes out what we did at the base level...
	void Start() {  //This could go onto the base actor class
		if (team == 1)  //We don't scare about friendlies
		{
			gameManager.Instance.addSpawn(vehicleType, vehicleScore);
		}

		//because the parent script stuff isn't working...?
		hitEffect = gameObject.GetComponentInChildren<Emitter_Hit>();
		smokeEffects = gameObject.GetComponentsInChildren<Emitter_Smoke>();
		
		//thrust should be calculated as the maximum angle the aircraft can climb without loosing 
		thrust = Mathf.Sin(10*Mathf.Deg2Rad) * gravity;
		speed = 0;// MaxAirSpeed*throttle; //get this setup for the kickoff.
		ourCameraObject=Camera.main.gameObject; //used for arcade controls.
		
		ourGunMP = new AttachedGun[CannonEmpties.Length];
		for (int i=0; i<ourGunMP.Length; i++) {
			ourGunMP[i]=CannonEmpties[i].GetComponent<AttachedGun>();
			ourGunMP[i].baseVehicle = gameObject;
		}
		
		//set up our stuff for the textures etc.
		//really we should have only one material...
		if (AircraftMesh)
			ourMat = AircraftMesh.GetComponent<Renderer>().material; //and make a dupe of it
	}
	public override void takeDamage(float thisDamage, string damageType, GameObject instigator, int damagingTeam, float delay) {
		
		thisDamage = getDamageMod(thisDamage);
		//Make this invincible for the moment...
		if (!isInvincible) {
			health -= thisDamage;
			if (health<0 && !bIsDead) {
				
				//doExplode(); //whack this.
				bIsDead=true;
				applyShotDown();
				dropPickups();
				callbackDie();
				CheckPlayerKill(instigator);
				bIsDead = true;
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
	
	//probably won't need this function anymore
	void applyShotDown() {
		//I'd love to have a "burn" effect for the balloon while it slowly floats down, but that'd require work...
		doExplode(0f);
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
		
		//Debug.Log (transform.localRotation.eulerAngles);
		//Shoot our aircraft down...
		if (bIsDead) {
			//roll commands
			//transform.RotateAroundLocal(transform.forward, -roll*turnspeed*Time.deltaTime);
			//pitch commands
			
			//We want to have this one pointing down...
			//transform.RotateAroundLocal(transform.right, pitch*turnspeed*Time.deltaTime);
			//Make this tend toward "down" as a general direction
			//Need to graduate this so that the force increases as damage is dealt - that way we can actually fight to keep the aircraft flying
			
			//back off on the force here so that the AI can ditch
			/*
			if (transform.localRotation.eulerAngles.z < 90F || transform.localRotation.eulerAngles.z > 270)
				transform.RotateAround(transform.right, turnspeed*Time.deltaTime*0.5F);
			else
				transform.RotateAround(transform.right, -turnspeed*Time.deltaTime*0.5F);
				*/
			//yaw commands
			//transform.RotateAroundLocal(transform.up, yaw*turnspeed*Time.deltaTime);
			//throttle details
			//basically this needs to start dropping after it's been shotup
			transform.position += Vector3.up * -1 * Time.deltaTime;
			
		}
		
		//impactJolt = Vector3.Lerp(impactJolt, Vector3.zero, Time.deltaTime*joltDecay);
		//impactSpin = Mathf.Lerp (impactSpin, 0, Time.deltaTime*joltDecay);
		
		//and the direction of travel for the aircraft. Later this will need to be cut against gravity, but for the moment this'll work
		/*
		transform.position+=transform.forward*Time.deltaTime*speed;

		//handle our jolt stuff
		transform.position+=impactJolt*Time.deltaTime; //handle out hit.
		transform.RotateAround(transform.forward, -impactSpin*Time.deltaTime);
		
		if (groundCollider && bIsPlayerVehicle) //more only suitable for the player as I wouldn't want it updating continiously for the AI
			checkGroundContacts();
			*/
		
	}

	//this works fine for doing player ground contact but it's a bit much for the AI I think.
	public void checkGroundContacts() {
		
	}
		
	public override GameObject getModel() {
		return BalloonModel; //it really is that simple
	}
}
