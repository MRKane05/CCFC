using UnityEngine;
using System.Collections;

public class AircraftController : Actor {
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
    #region Aircraft Properties For Player Control
    public float mass = 7;
	public float MaxAirSpeed = 6;
	public float StallSpeed = 1.5F;
//	public float rollspeed = 1.5F;		//Used by the AI for doing turns
//	public float pitchspeed = 1.5F;
//	public float yawspeed = 1.5F;		//Used by the AI for doing turns
	public float throttlespeed=0.5F;
	#endregion

	float throttle =0.75F, gravity=7, thrust=8, thrustPower = 30f, throttleAcceleration=0.5F; //this is a consideration on our AirSpeed, this is what we should be tending toward given that the likes of gravity etc. aren't acting upon us...

	float enginePitchMin = 0.6f, enginePitchMax = 1.4f;
	Engine[] engines; //well this is either as simple or as complicated as we want to make it really...

	//d_ functions (death)
	float d_delay = 2f; //how many seconds before we go boom
	int d_parachutes = 1;
	float d_time = float.MaxValue; //the point where we'll go bang (as set when damage is taken)

	public GameObject AircraftModel, AircraftMesh, FiringMarker;
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

	public Collider groundCollider; //our ground plane that we could be contacting with.

	//public float health=100F, maxHealth = 100F;
	//really this should be at the actor level
	public override void gradeSkill(float factor) {
		health*=factor;
		maxHealth*=factor;

		foreach (AttachedGun thisGun in ourGunMP) {
			thisGun.damage*=factor;
		}
	}

	//stuff for the terrain actually.
	//this isn't responding as well as I'd like it to be...might have to make the casting continious actually
	/*
	public override void doContact(Collider withThis) {
		//transform.position = with
		Debug.LogError(withThis.name);
		if (withThis.name == "Terrain") { //or is there a better way?
			//do we want to use a Collider.ClosestPointOnBounds to somehow reverse calculate our height or
			//some thing different? I suppose it'll have a bounce if we're coming in at an odd angle...
			Vector3 terrainLocation = Vector3.zero;

			Ray ray = new Ray(transform.position, -Vector3.up); //shoot this ray down to see where we contact

			RaycastHit hit;

			if (withThis.Raycast (ray, out hit, 5)) {
				transform.position = new Vector3(transform.position[0], hit.point[1] + 0.64F, transform.position[2]);
			}

		}
	}
	*/

	//Not sure about the "after effect" here as it needs to fade or sink into the ground or something so we
	//don't chew up too much memory
	void doDitch() { //is called when our aircraft ditches
		//Need to strip out anything that'd be using process, and leave the main system.
		foreach (GameObject thisCannon in CannonEmpties)
			Destroy (thisCannon); //remove our guns.

		//we'll need to tweak the smoke so that it's rising now instead of trailing.
		((LevelController)LevelControllerBase.Instance).removeSelf(gameObject, team); //For the moment I suppose
		Destroy (this); //remove this script.

		//we also need to remove from the 
	}

	/*
	void doExplode() { //is called with an explosive crash or when we're shot up enough
		if (explosionEffect) {//can't go bang if we don't have a bang effect
			GameObject exp_Effect = Instantiate(explosionEffect, transform.position, transform.rotation) as GameObject;
			//exp_Effect.transform.parent = transform; //stick it to this for the duration...
			sfx_Explosion explosion = exp_Effect.GetComponent<sfx_Explosion>();
			explosion.target = gameObject;
		}

		//Explode and delete this in the process
		((LevelController)LevelControllerBase.Instance).removeSelf(gameObject, team); //For the moment I suppose
		Destroy (gameObject, d_delay); //destroy aircraft after small "effect" delay
	}*/

	//we're getting a collision called from our base classes, handle accordingly
	//make sure we're not colliding with a target sphere!
	public override void doCollision(Collider withThis) {
		//Debug.LogError("Adding Impact Jolt");

		if (withThis.name == "Terrain")
			Debug.Log ("Hit Terrain");
			//return; //cancel this for the moment, we're working on contact stuff

		//Do a jolt from the collision which will be added to the velocity
		impactJolt = (transform.position-withThis.gameObject.transform.position); //get our impact offset
		//impactJolt -= transform.forward;
		//impactJolt = impactJolt;

		//from the two there we need the quotents from transform.up and transform.right.
		Vector3 resultingJolt = transform.right*Vector3.Dot(transform.right, impactJolt);
		resultingJolt += transform.up*Vector3.Dot(transform.up, impactJolt);


		float joltHit = mass; //so for the moment that'll work it I suppose, but we need to consider the thing we've hit also

		impactJolt = resultingJolt.normalized*joltHit;

		//check if this is a ground collider and if it is then put more punch up.
		if (groundCollider && withThis == groundCollider) {
			//need to make sure that this is tangental to the ground.
			Ray ray = new Ray(transform.position, -Vector3.up); //shoot this ray down to see where we contact
			
			RaycastHit hit;
			
			//float contactOffset = gameObject.collider.bounds.center.y + gameObject.collider.bounds.extents.y;
			//Debug.Log ("Contact offset: " + gameObject.collider.bounds.min);
			
			if (groundCollider.Raycast (ray, out hit, 5)) {
				transform.position = new Vector3(transform.position[0], Mathf.Max (hit.point[1] + AircraftModel.GetComponent<Collider>().bounds.extents.y, transform.position[1]), transform.position[2]);
				impactJolt = hit.normal*mass; //jolt up at the angle of the terrain

				//I'm not sure the above ditch rule is fair enough...
				if (bIsDead) { //handle what happens when we hit.
					if (Vector3.Dot (hit.normal, AircraftModel.transform.up) > 0.75F)
						doDitch();
					else
						doExplode(0f);	//Die on contact
				}
				else {
					if (Vector3.Dot (hit.normal, AircraftModel.transform.up) > 0.9F) { //check to see that we're tangential to our ground (as in not a crash)
						//this is a collision with our gear down. Don't take damage, and if we're shot down this is a ditch

					}
					else { //this collision should cause damage to our vehicle
						//not sure if it should be constant, or graded according to angle stuff, or what, for the moment...
						takeDamage(maxHealth/7F, "COLLISION", gameObject, team, 0F); //do a collision with the ground.
					}
				}
			}
			else { //how the hell did we hit the ground? nevermind just put something in
				impactJolt = Vector3.Lerp (resultingJolt, -Vector3.up*mass, 0.5F);

				takeDamage(maxHealth/7F, "COLLISION", gameObject, team, 0F);
			}

			//Do our ground collision suff. Damage etc...

		}
		//Debug.Log (impactJolt);

		if (impactJolt.magnitude < 0.5F) //what happens when we strike square on?
			impactJolt = (transform.right*Random.Range(0F,1F) + transform.up*Random.Range(0F,1F))*joltHit;

		//We need to spin this fighter a little
		//So how to translate this into spin?
		//Need to look at positional stuff to see if it's left or right
		impactSpin = Vector3.Dot(transform.right, impactJolt)*1.25F; //This works well

		//we need to sort out the damage side of things here...
	}

	// Use this for initialization, but it wipes out what we did at the base level...
	void Start() {
		doVehicleSetup();
	}

	public virtual void doVehicleSetup() {
		groundCollider = GameObject.Find("Terrain").GetComponent<Collider>(); //this is used for our ground contact

		//because the parent script stuff isn't working...?
		hitEffect = gameObject.GetComponentInChildren<Emitter_Hit>();
		smokeEffect = gameObject.GetComponentInChildren<Emitter_Smoke>();

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

	
	float SpeedChange() {
		//thrust, gravity
		GravityVector = transform.forward.normalized;
		//print (GravityVector);
		
		//some way of knowing if we need to speed up or slow down?
		// thrust-(expectedSpeed) ..which will mean we speed up or slow down according
		//expected speed = MaxSpeed*throttle. if we're below this we need to speed up, otherwise we need to slow down.
		//if we're going too slow we need to apply the speed of accelleration
		//check to see if we're going down here...
		
		//need to do an engine thing and recovery curve etc?
		//if we're pointing down we should increase speed reguardless of our throttle.
		//But up until we hit Maximum.

		//All of this system is very well but I'm inclined to think that it's not quite what I want for an "arcade" control...

		/*
		if (GravityVector.y < 0) { //then we're going downhill
			return (thrust-Mathf.Abs(GravityVector.y)*GravityVector.y*gravity)/mass; //we'll be increasing regardless	
		}
		else{
		*/
		//this does mean that we can over power our system...
		if (speed > MaxAirSpeed*throttle) { //then we're wanting to go slower.
			return (-thrust-Mathf.Abs(GravityVector.y)*GravityVector.y*gravity)/mass;
		}
		else { //So I suppose this is to accellerate...
			return (thrust-Mathf.Abs(GravityVector.y)*GravityVector.y*gravity)/mass;
		}
		//}



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
		((LevelController)LevelControllerBase.Instance).addKill(); //rack it up!

				//doExplode(); //whack this.
				bIsDead=true;
				applyShotDown();
				//we can't remove this as it'll mean the player can't shoot it...
				//((LevelController)LevelControllerBase.Instance).removeSelf(gameObject, team); //but our targeting system needs to know I suppose
				//Destroy (gameObject); //Just destroy it for now
			}
			/*
			if (health<-maxHealth/2) { //then destroy this aircraft fully for whatever reason
				if (instigator == PlayerController.Instance.ourAircraft.gameObject)
		((LevelController)LevelControllerBase.Instance).addMurder(); //not a good thing actually, and we're counting it

	((LevelController)LevelControllerBase.Instance).removeSelf(gameObject, team); //For the moment I suppose
				Destroy (gameObject);
			}
			*/
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

	//do our shot down setup stuff...
	void applyShotDown() {
		StartCoroutine(shotDownProcess(0.5f, d_delay));
		callbackDie();	//Callback for this actor dying
	}

	public IEnumerator shotDownProcess(float parachuteDelay, float explosionDelay) {
		yield return new WaitForSeconds(parachuteDelay);
		doExplode(d_delay); //blow this fighter up
	}
	
	// Update is called once per frame
	void Update () {
		
		//if (ourGUIText) {
		//	ourGUIText.text = "Speed: " + speed;


		if (ourMat) { //This is great apart from the target camera in which it looks a little odd...
			if (bIsTarget) {
				ourMat.SetColor ("_OutlineColor", Color.red);
			}
			else { //set the outline black...
				ourMat.SetColor ("_OutlineColor", Color.black);
			}
		}
			
		//}
		//speed = Airspeed; //really this should be += (CompfDifference(throttle*Airspeed, speed, Gravity()) * Time.deltaTime;
		//Really only need to set on and off for these...
		//Solved by having a mainBlind and targetBlind, but I think the connection between the two (with highlite) is actually pretty good...
		if (bFiring!=bLastFiring) {
			bLastFiring=bFiring;
			for (int i=0; i<ourGunMP.Length; i++) {
				if (ourGunMP[i]!=null) {
					ourGunMP[i].bFiring = bFiring;
					ourGunMP[i].FireState = FireState; //Will probably take over from bFiring
				}
			}
		}

		//Adjust throttle...
		throttle = Mathf.Clamp01(throttle+throttleControl*throttlespeed*Time.deltaTime); //for the moment keep it conformed.
		
		//This calculates our speed through the air dependend upon our angles etc.
		speed = Mathf.Clamp(speed + SpeedChange() * Time.deltaTime, 0, MaxAirSpeed*1.2F);
		//speed += SpeedChange()*Time.deltaTime; //we don't need any variables as they're part of the class.

		//first try setting engine pitch based off the speed of the aircraft...
		//Debug.Log ("Throttle: " + throttle);

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
				transform.RotateAround(transform.right, rollspeed*Time.deltaTime*((StallSpeed-speed)/StallSpeed)*2);
			else
				transform.RotateAround(transform.right, -rollspeed*Time.deltaTime*((StallSpeed-speed)/StallSpeed)*2);
		}
		//Debug.Log (transform.localRotation.eulerAngles);
		//Shoot our aircraft down...
		if (bIsDead) {
			//roll commands
			//transform.RotateAroundLocal(transform.forward, -roll*rollspeed*Time.deltaTime);
			//pitch commands

			//We want to have this one pointing down...
			//transform.RotateAroundLocal(transform.right, pitch*pitchspeed*Time.deltaTime);
			//Make this tend toward "down" as a general direction
			//Need to graduate this so that the force increases as damage is dealt - that way we can actually fight to keep the aircraft flying

			//back off on the force here so that the AI can ditch
			/*
			if (transform.localRotation.eulerAngles.z < 90F || transform.localRotation.eulerAngles.z > 270)
				transform.RotateAround(transform.right, c*Time.deltaTime*02f);
			else
				transform.RotateAround(transform.right, -rollspeed*Time.deltaTime*02f);
			*/
			AircraftDeathSpiral();
			//yaw commands
			//transform.RotateAroundLocal(transform.up, yaw*yawspeed*Time.deltaTime);
			//throttle details
			
		}
		
		impactJolt = Vector3.Lerp(impactJolt, Vector3.zero, Time.deltaTime*joltDecay);
		impactSpin = Mathf.Lerp (impactSpin, 0, Time.deltaTime*joltDecay);
		
		//and the direction of travel for the aircraft. Later this will need to be cut against gravity, but for the moment this'll work
		transform.position+=transform.forward*Time.deltaTime*speed;

		//handle our jolt stuff
		transform.position+=impactJolt*Time.deltaTime; //handle out hit.
		transform.RotateAround(transform.forward, -impactSpin*Time.deltaTime);

		if (groundCollider && bIsPlayerVehicle) //more only suitable for the player as I wouldn't want it updating continiously for the AI
			checkGroundContacts();
	
	}
	
	void NPCUpdate() {
		//Rotates us around the world origin...
		//Quaternion currentRotation = transform.rotation;
		//While it's awesome to do this we need to think about how the aircraft is going to behave...they bank while
		//turning and we need to emulate that here as this isn't a real sim...
			
		//Basically we want to always be facing "up" with the direction of turn.
			
		transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, yawspeed); 
		AircraftModel.transform.localEulerAngles = new Vector3(0,0,Mathf.LerpAngle(AircraftModel.transform.localEulerAngles[2], -roll*90, (rollspeed)*Time.deltaTime));
			
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
		
		Ray ray = new Ray(transform.position, -Vector3.up); //shoot this ray down to see where we contact
		
		RaycastHit hit;

	//float contactOffset = gameObject.collider.bounds.center.y + gameObject.collider.bounds.extents.y;
	//Debug.Log ("Contact offset: " + gameObject.collider.bounds.min);
		//not sure about ditching here, but for the moment this should work
		if (groundCollider.Raycast (ray, out hit, 5)) {
			//don't update the transform position if the normal from the ground is too dissimilar to the
			//normal from the player aircraft's up vector
			//Debug.Log ("Dot Normals: " + Vector3.Dot (hit.normal, AircraftModel.transform.up));
			if (Vector3.Dot (hit.normal, AircraftModel.transform.up) > 0.9F) { //check to see that we're tangential to our ground (as in not a crash)
				transform.position = new Vector3(transform.position[0], Mathf.Max (hit.point[1] + AircraftModel.GetComponent<Collider>().bounds.extents.y, transform.position[1]), transform.position[2]);
		
			}
		}
		//}

	}

	//returns (unrealistically) our altitude above the current terrain
	public float checkAltitude() {
		if (!groundCollider) { return 100;  }
		Ray ray = new Ray(transform.position, -Vector3.up); //shoot this ray down to see where we contact
		
		RaycastHit hit;

		if (groundCollider.Raycast(ray, out hit, float.MaxValue)) {
			return hit.distance;
		}

		return -1f; //no update...
	}

	void PlayerUpdate() {
		if (!ourPlayerController.bControlArcade) { //control the pitch, roll, yaw of the vehicle.
			//roll commands
			transform.RotateAroundLocal(transform.forward, -roll*rollspeed*Time.deltaTime);
			//pitch commands
			transform.RotateAroundLocal(transform.right, pitch*pitchspeed*Time.deltaTime);
			//yaw commands
			transform.RotateAroundLocal(transform.up, yaw*yawspeed*Time.deltaTime);
			//throttle details
		}
		else { //we want to control relitive to where we think it should be going
			//this is a camera relitive control.
			transform.RotateAround(ourCameraObject.transform.up, roll*rollspeed*Time.deltaTime);
			transform.RotateAround(ourCameraObject.transform.right, pitch*pitchspeed*Time.deltaTime);
			
			//command to roll us back up the proper way.
			transform.RotateAroundLocal(transform.forward, -yaw*rollspeed*Time.deltaTime);
			//Need to figure out what up is...
			
			//not sure what to do about yaw...

			//Aircraft interpertation section of this controller.
			//if we're pulling left or right we should rotate to the relitive 90...
			AircraftModel.transform.localEulerAngles = new Vector3(0,0,Mathf.LerpAngle(AircraftModel.transform.localEulerAngles[2], -roll*90, (rollspeed)*Time.deltaTime));
		}
	}

	public override GameObject getModel() {
		return AircraftModel; //it really is that simple
	}
}
