using UnityEngine;
using System.Collections;

public class AI_Fighter : ActorController {
	
	//public GameObject target;
	//public Actor targetController;
	
	AnimationCurve aircraftRollCurve;
	float aircraftYaw;
	
	Vector3 targetLead; //will be set by seek target.
	
	public float taticalTime, patternTime, patternDuration; //whenever we do a tatical manuver this gets set for when we should do the next one.
	public float[] taticalRange; //time period between when we'll do another tatical move
	public float evasionRange=30; //Our ranges for evasion turns

	public float shootDistance = 40; //don't shoot at anything father than this.

	Quaternion targetRotation, returnRotation;
	float targetSpeed=1; //how much throttle should we have?
	
	#region AItimers
	//we've got some magic numbers here
	float timerVariance=0.5F; //Plus Minus to our variables
	float attackBreakTime=0.7F; //How long do we break away for between fire bursts
	public float attackDuration=1.2F; //How long will we fire for?
	float attackSetupTime=2F; //how long do we take to setup?
	

	public Range evadeDuration = new Range(3f, 7f);
	public Range escapeDuration = new Range(5f, 9f);

	#endregion
	
	public string pattern="ATTACK", patternStage="SETUP"; //what pattern are we in? attack etc. and what stage are we in the pattern?
	public string preferredAttackMethod="", attackMethod="FORWARD";
	public float attackReleaseTime=0; //, attackPatternTime=0;
	public string[] attackPatternArray;
	
	//Memory stuff...
	float damageThreshold =10; //how much do we take before we change a state?
	float damageTaken=0; //how much damage have we taken?
	GameObject lastDamager; //who hit us last? We're simple like that...
	
	//Firing functions
	bool bIsFiring = false, bWasFiring=false;
	float fireOngoingTime, fireEndTime, fireDuration=1.2F, triggerHoldTime=0.33F, triggerStartRand=1.2F; //because I think it's cool if fire systems have an overhold
	
	
	string targetMethod="AIM"; //ahead: pick a spot ahead and fire through it, ontarget: fire on the correct spot
	float targetTime, targetReset=0.7F; //the time we started the target method
	
	//Targetting stuff, not sure how well this is working actually.
	Vector3 targetSwing; //how much we're "sloppy" about this target. A higher swingUpdate will give us a sharper target.
	float targetUpdate=3F, swingUpdate=2;
	Quaternion lastRotation;
	float lastYaw;

	public Actor followTarg; //Target that we're following - is either what we're "protecting" or our wing leader
	public int formationNumber = -1; //assign this dynamically
	//ATTACK: ATTACK, SETUP
	//STOP: COLLABORATE AND LISTEN hehehe

	//sent through whenever an AI needs to send out a message to other AI - could also be used for team situations.
	public override void getNotification(string thisMessage, string thisTag) {
		//we could do with a small delay here...
		Debug.Log (gameObject.name + " Got Message: " + thisMessage);

		//we should have been tag filtered already.
		if (thisMessage == "PATROLBREAK") { //we've been called to break our patrol pattern (probably by another fighter being attacked
			if (pattern=="PATROL") { //do the standard patrol escape stuff.
				//do an escape panic or flat out attack...oh the choices!
				float randomDraw = Random.value;
				if (randomDraw < 0.33F) { //full out panic
					pattern = "DOEVASIVE";
					patternStage = "PICK";
				}
				else if (randomDraw < 0.66F) { 
					pattern="ATTACK";
					patternStage="SETUP"; //slight disorientation
				}
				else {
					pattern="ATTACK";
					patternStage="ATTACK"; //staight attack
				}
			}
		}

	}
	
	public Vector3 getTargetLead() { //used by the guns
		return targetLead;
	}
	
	public void setTarget(GameObject newTarget, Actor newController) {
		target = newTarget;
		targetController = newController;	
	}
	
	// Use this for initialization
	void Start () {
		//Setup our roll curves for this particular aircraft
		if (aircraftRollCurve==null)
			aircraftRollCurve = new AnimationCurve(new Keyframe(5, 0), new Keyframe(25, 1F), new Keyframe(335, 1F), new Keyframe(355, 0F)); //this is a touch incomplete?
		
		//Setup our guns
		AttachedGun[] ourGuns = ourAircraft.gameObject.GetComponentsInChildren<AttachedGun>();
		foreach(AttachedGun thisGun in ourGuns) {
			thisGun.ourFighterAI = this;	
		}
	}
	//This is returned if we've called for a target.
	public override void targetCallback(Actor newTargetController, GameObject newTarget, int flightProcess) {

		target = newTarget;
		targetController = newTargetController;
		//Set our fighter to attack

		//this is causing issues with our Patrol call
		if (target) {
			pattern="ATTACK";
			patternStage="SETUP"; //needs to have the return stuff put in place here
			//we need to click out of this pattern
			targetSpeed=1;
			patternTime=Time.time;
			patternDuration = varValue(attackSetupTime, timerVariance);
			formationNumber = -1; //clear our formation information
		}
		else { //hopefully this covers "missing" also
			if (followTarg && formationNumber==-1) {
				//Need to request a number here...somehow...
				//Might actually be smarter to request it from the Level controller.
				formationNumber = LevelController.Instance.getFormationNumber(team, followTarg.gameObject);
				pattern="FOLLOW"; //go back to following our group lead.

			}
		}
	}
	
	
	// Update is called once per frame
	void Update () {
		DoFighterAI();
	}

	void handleTargets() { //to be used to see what we should be doing target wise.
		if ((!target || targetController.bIsDead) && pattern!="FOLLOW") {
			levelLink.requestTarget(this);
		}

		//will need to extend this a little for misbehaving players (killing pilots)
		if (targetController!=null && targetController.bIsDead) {//cycle on if we've shot this down
			levelLink.requestTarget(this);
		}
	}
	
	//We need to know who the instigator is...
	public override void TakeDamage(float thisDamage, string damageType, GameObject instigator, float delay) {
		
		damageTaken += thisDamage;
		lastDamager = instigator; //this will be evaluated in the AI center.
		
	}
	
	float targetAngle(GameObject thisTarget) { //is this in front of us?
		Vector3 targetDir = thisTarget.transform.position - transform.forward;
		float angle = Vector3.Angle(targetDir, transform.forward);
		return angle;	
	}
	
	//Does all the attack script stuff, including handing off behaviours to other functions
	
	public virtual void HandleDamage() {

		if (damageTaken > damageThreshold) {
			if (pattern =="ATTACK") {
				//Debug.Log("Attack damage Change");
				if (patternStage =="ATTACK") {
					//Check if we should be changing targets here or if we should keep on going
					targetSpeed = 1; //full throttle
					//For this small build we'll keep going
					damageTaken = 0;
				}
				else if (patternStage == "SETUP") {
					//Lets drop this into an evade...
					pattern = "EVADE";
					patternDuration = evadeDuration.GetRandom();
					patternTime = Time.time;
					damageTaken = 0;
				}				
			}
			else if (pattern == "EVADE") { //lets drop into an evasive
				Debug.Log("Evade damage Change");
				pattern = "DOEVASIVE";
				patternStage = "PICK";
				patternDuration = escapeDuration.GetRandom();
				patternTime = Time.time;
				damageTaken = 0;
			}
			else if (pattern == "DOEVASIVE") { //this is getting despirate!
				Debug.Log("Evasive damage Change");
				//Do...something!
				//This gus has a serious bone to pick with us. Probably should make it our target
				if (lastDamager) {
					target = lastDamager;
					targetController = lastDamager.GetComponent<Actor>();
				}
				pattern="ATTACK";
				patternStage="ATTACK";
				
				patternDuration = 5;
				patternTime = Time.time;
				damageTaken = 0;
			}
		}

		//check to see if we're in a patrol, and if so bullets flying over our system should be a key indicator to break
		if (pattern=="PATROL" || pattern=="FOLLOW") {

			//notify the other fighters in our group that we're being attacked
			LevelController.Instance.notifyWithTag(team, "PATROLBREAK", flightGroup); 

			//do an escape panic or flat out attack...oh the choices!
			float randomDraw = Random.value;
			if (randomDraw < 0.33F) { //full out panic
				pattern = "DOEVASIVE";
				patternStage = "PICK";
			}
			else if (randomDraw < 0.66F) { 
				pattern="ATTACK";
				patternStage="SETUP"; //slight disorientation
				targetSpeed=1;
				patternTime=Time.time;
				patternDuration = varValue(attackSetupTime, timerVariance); //set our ticker
			}
			else {
				pattern="ATTACK";
				patternStage="ATTACK"; //staight attack
			}
		}
	}
	
	public virtual void PatrolPattern() { //moves through pre-defined waypoints. Not sure how I'll do this yet
		
		
	}

	//also need something for formation holding with this AI so that it'll keep formation on the player as required
	
	
	//Used with our timer stuff for the random
	float varValue(float keyDuration, float keyVariance) {
		return Mathf.Abs(keyDuration + Random.Range(-keyVariance, keyVariance));
	}
	
	public virtual void AttackPattern() {
		//Our breakers...
		//This function can cause an issue as it'll prevent a break if the player tails the target
		if (patternStage=="ATTACK" && target!=null) {
			if ((transform.position-target.transform.position).magnitude < 10) {//then we should break
				//First up we should be looking to attack our target.
				DoTaticalManuver (out returnRotation, out aircraftYaw); //Evade something.
				
				//At this stage we'll have to decide what we'll do about damage. And that relates to other fighters also.
				patternStage="SETUP";
				targetSpeed=1;
				patternTime=Time.time;
				patternDuration = varValue(attackSetupTime, timerVariance);

				//call our check stuff to see if we should stop careening off wildly...
				CheckSituation();
				
			} else if (target!=null) {
			
				SeekTarget(out returnRotation, out aircraftYaw);
				
				if (bIsFiring && !bWasFiring) { //we've just begun firing on our target
					patternTime = Time.time;
					patternDuration = 2; //varValue(attackDuration, timerVariance);
					
				}
				
				bWasFiring = bIsFiring; //keep this up to speed.

				if (Time.time - patternTime > patternDuration) { //we need to do a fire break
					patternStage = "PAUSE";
					patternTime = Time.time;
					patternDuration = varValue(attackBreakTime, timerVariance);
					
				}
			}
		} else if (patternStage=="ATTACK" && target==null) { //we really should get a target...
			CheckSituation ();
		}

		if (patternStage=="PAUSE") { //we're having a breather in the action
			DoWeave(out returnRotation, out aircraftYaw, 10); //that will do for now
			if (Time.time - patternTime > patternDuration) {
				patternStage="ATTACK";
				//Clear our firing flags
				bWasFiring=false;
				patternDuration=evadeDuration.GetRandom(); //this should be cleared by the master section of this pattern (but may sometimes huff)
			}
		}
		if (patternStage=="SETUP") {
			FleeTarget(out returnRotation, out aircraftYaw); //Get away
			
			//Check our breakers
			if (Time.time-patternTime > patternDuration) {
				patternStage="ATTACK";
				targetSpeed = 1;
			}
		}
	}
	
	public virtual void EvadePattern() {
		if (pattern=="EVADE") { //Also useful for a mid-fight pattern. Aka scramble patterns before going into a long engage.
			//Generally do our escape stuff...
			DoTaticalManuver (out returnRotation, out aircraftYaw);
			if (Time.time - patternTime > patternDuration) {
				//Need to check and see if we're in a state to attack or if we should do an evasive manuver...
				if ((transform.position-target.transform.position).magnitude < 10) { //do an evasive
					pattern = "DOEVASIVE";
					patternStage = "PICK";
					patternTime = Time.time;
					patternDuration = escapeDuration.GetRandom();
				}
				else {
					pattern="ATTACK";
					patternStage="ATTACK";
					patternTime = Time.time;
					patternDuration = attackDuration;
				}
			}
		}
		
		if (pattern=="DOEVASIVE") {
			if (patternStage =="PICK") {
				float rndVal = Random.Range(0, 2);
				if (rndVal < 1)
					patternStage="LOOP";
				else if (rndVal < 2)
					patternStage="WEAVE";
				
			}
			//things like loop, immulman, barrel roll etc.
			//Not really sure how I'll programme this in given the AI methods...
			//Even though the factor can be whatever we want, at the end of the day we're only as manuveriable as our aircraft
			//Measured functions
			if (patternStage=="LOOP") {
				DoLoop (out returnRotation, out aircraftYaw, 20);
				targetSpeed = 1;
				CheckBreak();
			}
			if (patternStage=="WEAVE") {
				DoWeave(out returnRotation, out aircraftYaw, 20);
				//this is to be a little more entropic
				targetSpeed = 0.75F + Mathf.Sin(Time.time) * 0.25F; //vary our time, although this needs a better ticker
				CheckBreak();
			}
		}	
	}
	
	//Really the AI has about two different base states, shooting at, and being shot at.
	//In both cases we need it to be as interesting and as exciting as possible
	//When shooting at the player it needs to shoot in timed bursts
	//Upon getting too close to the player it needs to break away and come around for another attack
	
	//When being shot at we need to behave similar to xWing with simple turns and attempts to evade
	//Periodic turns to a new angle (that's within about 90deg of the players aspect). If the dot normal of the directions
	//hits a suitable threshold, or the aircraft gets to a point where it can about face and attack it does so
	
	//Headon runs are just like a standard attack run I'd assume
	
	//AI aircraft needs to avoid except in a few headon cases. Need to think of a method to collide, or alternatively
	//just turn it off
	
	//Patterns:
	//Attack run: AI needs to back away from the target for long enough to make the attack run. Distance will have to be
	//calculated somehow, then turn to do an attack run
	//When being tailed the AI will have to do an attack run, followed by a dodge, and another attack run.
	
	//Self-preservation functions
	
	
	//So here we need to break this down into the command parts...
	//SeekTarget (out returnRotation, out aircraftYaw);
	//DoTaticalManuver (out returnRotation, out aircraftYaw); //Basic twisting and turning stuff
	//FleeTarget (out returnRotation, out aircraftYaw);

	public override void setPatrol(float awakeTime) { //used to set this AI onto a patrol pattern
		pattern="PATROL";
		//Debug.LogError("Set Patrol Routine");
		//if (Time.time - patternTime > patternDuration)
		patternTime = Time.time;
		patternDuration = awakeTime; //this is how long it takes us to "snap out" if you will
	}

	//this is our main AI switching center
	void DoFighterAI() {
				
		//=====Our damage handling patterns...================
		//we really need a few different thresholds here

		if (damageTaken>damageThreshold) {
			HandleDamage();
		}

		//check our oblivious cases.
		if (pattern!="PATROL") {
			handleTargets();
		}

		//So our AI might also be doing a patrol (will be broken by check break)
		if (pattern=="PATROL") {
			//shouldn't need to do anything, at least for the moment, the AI will break as necessary
			//so here we should stay alert for seeing any enemy (but not always go for the enemy that we're looking at)
			if (LevelController.Instance.checkSight(this, 45F)) { //that's quite a tight FOV...
				pattern="ATTACK";
				patternStage="ATTACK";
				//Debug.LogError("Sight Break Patrol");
			}
			CheckBreak(); //this should get us out of the pattern stuff after we've been latent for long enough (set by the function)
		} else if (pattern=="FOLLOW") {
			followTarget(); //directly follows a target...doesn't have any outs.
			CheckBreak();
		} else { //our standard combat method
			//=====Our attack pattern=================
			if (pattern=="ATTACK") {
				AttackPattern();
			}

			if (pattern=="EVADE" || pattern=="DOEVASIVE") {
				EvadePattern();	
			}
			

			//Our AI to handle if we're taking damage here. We'll know as our instigator won't be cleared.
			if (lastDamager) {	
				lastDamager=null; //set this to nothing so that we're not doing it all the time	
			}
			

			if (pattern=="PULLUP") {
				//We're too close to the ground, and need to pull up! Not that I'd know why with a normal fighter...
			}

			//at this point we should probably put something in that'll prevent the fighter from crashing,
			//the rest of the system should sort itself out in due time.
		}
		//groundCollider should be a global.
		//PROBLEM: Need to fix the ground collider settings here
		/*
		if (ourAircraft.groundCollider) { //check for the possiblity of needing to evade

			Ray ray = new Ray(ourAircraft.transform.position, Vector3.Lerp(ourAircraft.transform.forward, -Vector3.up, 0.3F)); //shoot this ray down to see where we contact
			Debug.DrawRay(ray.origin, ray.direction, Color.yellow);
			RaycastHit hit;

			if (ourAircraft.groundCollider.Raycast (ray, out hit, 7)) { //not sure about the distance required here...
				//we want this fighter to turn in the direction most fitting for the evade - it'll be a
				//rotation around the least path to up...amazing how this is like the origional CC AI.
				//Debug.Log ("Ground Avoid");
				ourAircraft.AIUpdateInput(Quaternion.Euler(Vector3.up), aircraftYaw, targetSpeed, bIsFiring);
				return;
			}
		}
		*/
		//maybe not the best place?
		bIsFiring = bFireOnTarget(); //so this will return if we should be shooting at our target 


		/*
		bool bTempFiring = bFireOnTarget(); //set this which will be used.
		
		//Our over fire time should easily cover this over
		if (bTempFiring && !bIsFiring) { //we've just begun shooting, need to set our burst stuff
			fireEndTime = Time.time + fireDuration;	
		}
		
		bIsFiring = bTempFiring;
		*/
		
		ourAircraft.AIUpdateInput(returnRotation, aircraftYaw, targetSpeed, bIsFiring); //Turn to face a target
		//Need one for flying away from a target.
		
	}

	//called while we're in a tatical situation, intended for target evaluation stuff
	void CheckSituation() {
		//so our calls will come through when we change state etc.
		//lets try something simple here
		//Debug.Log ("Checking Situation");
		LevelController.Instance.requestTarget(this);


	}

	//This function will forcibly change state if we're set to break. Prevents huffing while evading and with other things like Patrol
	void CheckBreak() {

		//We have a flight check system when we're following
		if (pattern=="FOLLOW" && (Time.time - patternTime > patternDuration)) {
			levelLink.requestTarget(this);
			patternTime = Time.time;
			patternDuration=1f; //one second intervals (for the alert wingman)
		}

		if (Time.time - patternTime > patternDuration) { //Our usual breaker?
			if (pattern=="PATROL") {
				if (gameManager.Instance.bDebugEvents) {Debug.LogError("Broke Patrol"); }
			}
			pattern="ATTACK";
			patternStage="ATTACK";
			patternTime = Time.time;
			patternDuration = 1f; //this is our attack ticker.
		}
	}



	//====Follow patterns - generally just seeks a location by an aircraft========================================
	//Presently isn't rolling to the left...?
	void followTarget() {
		//DoLoop (out returnRotation, out aircraftYaw, 20);
		//===========Target Seeking Functions...======================
		//if (false) {
		//This needs to be leading the target to be proper
		//if (targetController==null) { //For non-AI stuff we're dealing with our target lead methods...

		//we need to know which position we're assigned to as a wingman...
		if (formationNumber == 0)
			transform.LookAt(followTarg.transform.position - followTarg.transform.right*1.5f,Vector3.up); //we can have our GameObject do this as it's a child of the aircraft
		else 
			transform.LookAt(followTarg.transform.position + followTarg.transform.right*1.5f,Vector3.up);

		//}

		//Figure out what our turn angles should be doing...

		float newYaw = 1.0f;

		if (transform.localEulerAngles[1] > 180f) {
			newYaw = -1.0F;
		}

		//Debug.Log ("Roll: " + aircraftRollCurve.Evaluate(Mathf.Abs(transform.localEulerAngles[1])) + " eul: " + transform.localEulerAngles[1]);


		//We should put a delay on this so that the AI is always playing a bit of catchup.
		//newYaw *= Mathf.Lerp (lastYaw, aircraftRollCurve.Evaluate(Mathf.Abs(transform.localEulerAngles[1])), Time.deltaTime*targetUpdate);	
		newYaw *= aircraftRollCurve.Evaluate(Mathf.Abs(transform.localEulerAngles[1]));	

		lastYaw = newYaw;

		aircraftYaw = newYaw;
		
		returnRotation = Quaternion.Lerp (lastRotation, transform.rotation, Time.deltaTime*targetUpdate);
		lastRotation = returnRotation;

		targetSpeed = followTarg.getSpeed()/((AircraftController)ourAircraft).MaxAirSpeed;
		//Debug.Log("target Speed: " + targetSpeed);
		//now we need to look at lerping this fighter into position...
		//public void NPClerp (Vector3 targetLoc, float pullSpeed) {
		//this is working, but lacks a little soul at the moment
		if (formationNumber ==0) {	
			ourAircraft.NPClerp(followTarg.transform.position - followTarg.transform.right*1.5f - followTarg.transform.forward*0.7f, 3f);
		} else {
			ourAircraft.NPClerp(followTarg.transform.position + followTarg.transform.right*1.5f - followTarg.transform.forward*0.7f, 3f);
		}
	}
	
	//====Manuvers...can probably be mixed with things============================================================
	//Manuvers should be mixed with function to add some severe variance to flight
	//The basic loop. There's no function to check when we're done however.
	public void DoLoop(out Quaternion newRotation, out float newYaw, float factor) {
		transform.rotation = ourAircraft.gameObject.transform.rotation; //reset this sucker.
		
		transform.RotateAround(transform.right, -factor); //Well I'll buy that for a dollar despite it not working
		newRotation = transform.rotation;
		newYaw = 0;
		
	}
	
	//Weave is a feedback pattern around a rotational axis - it serves no function but provides an interesting mechanic.
	public void DoWeave(out Quaternion newRotation, out float newYaw, float factor) { //this is a delightful little messup.
		transform.rotation = ourAircraft.gameObject.transform.rotation; //reset this sucker.
		
		transform.RotateAroundLocal(transform.right, factor); //Well I'll buy that for a dollar despite it not working
		newRotation = transform.rotation;
		newYaw = 0;
		
	}
	
	//===========Attack Function==============================
	bool bFireOnTarget() {
		if (fireOngoingTime + triggerHoldTime > Time.time) { //we're still firing away happily
			return true;	
		}

		if (target) {
			//dupe this command to keep it clean...
			if (fireOngoingTime + triggerHoldTime > Time.time) { //we're still firing away happily
				return true;	
			}
			else if ((target.transform.position-ourAircraft.gameObject.transform.position).sqrMagnitude > shootDistance*shootDistance)
				return false; //don't shoot...we're out of range
			else if (patternStage=="ATTACK" && pattern=="ATTACK") { //only do this if we're attacking, no chance fire etc.
				//Check to see what our angle vrs the angle to is.
				if (target) { //Might need closer checks if we're flying formation, but that's for later I suppose
					//Figure out our attack pattern stuff
					if (attackReleaseTime>Time.time) { //choose another attack pattern at Random
						
						
						
						
					}
					
					
					float toTargetAngle=100;
					if (targetController) {
						toTargetAngle=Vector3.Angle ((targetLead-ourAircraft.gameObject.transform.position), ourAircraft.gameObject.transform.forward);
					}
					else {
						toTargetAngle=Vector3.Angle (ourAircraft.gameObject.transform.forward, (target.transform.position - targetLead-ourAircraft.gameObject.transform.position));
					}
					
					//Debug.Log ("To Target Angle: " + toTargetAngle);
					if (toTargetAngle < 17F) {
						fireOngoingTime = Time.time;
						//Debug.Log("AI Firing");
						return true; //We're firing on this target
					}
				}
			}
		}
		return false;
	}
	
	//Need to build in some base stuff with the aircraft control:
	//Fire at Point (so we lock at a point and don't move off until a set amount of time)
	//Seek forward (so players see shots firing over them) before it drops back
	//Swing through (how would I do this?)
	
	
	//We can attack in a variety of ways, this staggers things out a bit, and resets target times
	//Assuming that the method itself doesn't do it.
	void selectAttackMethod() {
		if (targetMethod=="AHEAD") { //we want to break to something that's more follow up. Just put on the timer and go to normal
			targetTime = Time.time + 2; //two seconds should do it	
			targetMethod="AIM"; //Could also be some sort of additive swing. but for now, swing is controlled by a setting
		}
		if (targetMethod=="AIM") {
			targetMethod = "AHEAD"; //go back to shooting forward
			targetTime = Time.time; //Keep active update.
			
		}
		
		
	}
	
	//===========Flight Function==============================
	void SeekTarget(out Quaternion newRotation, out float newYaw) {
		//===========Target Seeking Functions...======================
		//if (false) {
		//This needs to be leading the target to be proper
		if (targetController==null) { //For non-AI stuff we're dealing with our target lead methods...
			transform.LookAt(target.transform.position,Vector3.up); //we can have our GameObject do this as it's a child of the aircraft
		}
		else { //we've got a controller so lead it
			//targetSphere.transform.position = transform.position + transform.forward*(playerAircraft.transform.position-transform.position).magnitude*ourSpeed/bulletSpeed;
			//This is where we can put in the suff for our fire style
			
			float targetDistance = (ourAircraft.gameObject.transform.position-target.transform.position).magnitude;
			
			//Amongst all the styles:
			//Ahead: Pick a point within realms and lock onto that point while firing
			//Heavy: AI over steers and is sloppy with the controls effectively blanketing the area
			//Before this stage we know what our old target lead was.
			
			/*
			if (targetMethod=="AHEAD") { //we want to seek in front of our target
				//At a particular point this needs to stop for a beat and only cycle on after we've waited a tic.
				if (Time.time > targetTime) { //we're looking to reset
					targetLead = target.transform.position + target.transform.forward*targetDistance*2F*targetController.getSpeed()/50F;
					//We need to lock down on the points
					float toTargetAngle=Vector3.Angle ((targetLead-ourAircraft.gameObject.transform.position), ourAircraft.gameObject.transform.forward);
					
					if (toTargetAngle < 5F) { //lockdown
						Debug.Log("LockToTarget: " + toTargetAngle);
						targetTime = Time.time + targetReset;	
					}
				}	
			}
			else { //Fire directly on the target
				*/
				targetLead = target.transform.position + target.transform.forward*targetDistance*targetController.getSpeed()/50F;
			//}
			
			//Calculate our swing:
			
			/*
			Vector3 aircraftPoint = ourAircraft.gameObject.transform.position + ourAircraft.gameObject.transform.forward*targetDistance;
			targetSwing += (targetLead-aircraftPoint);
			targetSwing = Vector3.Lerp (targetSwing, Vector3.zero, Time.deltaTime*swingUpdate);
			
			targetLead += targetSwing; //our offset
			*/
			transform.LookAt(targetLead,Vector3.up); //we can have our GameObject do this as it's a child of the aircraft
		
		}
		//Figure out what our turn angles should be doing...
		newYaw = 1.0f;
		if (transform.localEulerAngles[1] > 180)
			newYaw = -1F;
		
		//We should put a delay on this so that the AI is always playing a bit of catchup.
		//newYaw *= Mathf.Lerp (lastYaw, aircraftRollCurve.Evaluate(Mathf.Abs(transform.localEulerAngles[1])), Time.deltaTime*targetUpdate);	
		newYaw *= aircraftRollCurve.Evaluate(Mathf.Abs(transform.localEulerAngles[1]));

		newYaw = Mathf.Lerp (lastYaw, newYaw, Time.deltaTime*targetUpdate);

		lastYaw = newYaw;



		newRotation = Quaternion.Lerp (lastRotation, transform.rotation, Time.deltaTime*targetUpdate);
		lastRotation = newRotation;

	}
	
	//This flies away but we stay at the same vertical level as our target...
	//Almost need to have something in here that'll make us flee up, or down, or something other than horizontal.
	void FleeTarget(out Quaternion newRotation, out float newYaw) {
		//===========Target Seeking Functions...in reverse...======================
		//if (false) { 
			transform.LookAt(target.transform.position,Vector3.up); //we can have our GameObject do this as it's a child of the aircraft
			//And the opposite direction to this isn't the inverse...
			
			transform.eulerAngles += new Vector3(0, 180, 0);
			//Figure out what our turn angles should be doing...
			newYaw = 1.0f;
			if (transform.localEulerAngles[1] > 180)
				newYaw = -1F;
			
			
			newYaw *= aircraftRollCurve.Evaluate(Mathf.Abs(transform.localEulerAngles[1]));	
			newRotation = transform.rotation;
			//ourAircraft.AIUpdateInput(transform.rotation, newYaw);
		//}
	}
	
	
	public void DoTaticalManuver(out Quaternion newRotation, out float newYaw) {
		//===========Target Avoidance/Dodge Functions...====================
		//These would be selecting a different angle and then turning to it for a few beats, then shake dry and repeat...
		//This works as expected but it lacks soul if you get what I mean...
		
		//This process needs some more vertical manuvers and twists and turns.
		
		//Need to make it so that these movements become larger as we continue to try to evade?
		if (Time.time > taticalTime) { 
			taticalTime = Time.time + Random.Range(taticalRange[0], taticalRange[1]);
			transform.rotation = ourAircraft.transform.rotation; //Set our transform direction to where the aircraft is going
			//Adjust it according to our dodge paramaters...
			targetRotation = transform.rotation; //Probably don't need this double cast...
			targetRotation.eulerAngles += new Vector3(Random.Range(-evasionRange, evasionRange), Random.Range(-evasionRange, evasionRange),0);
		}
		transform.rotation = targetRotation; //So that this can be used for our rotational details later...
		
		//Add in our tilt details while we're moving so that this looks a bit more realistic.
		newYaw = 1.0f;
		if (transform.localEulerAngles[1] > 180)
			newYaw = -1F;
		
		newYaw *= aircraftRollCurve.Evaluate(Mathf.Abs(transform.localEulerAngles[1]));
		
		newYaw = Mathf.Lerp (lastYaw, newYaw, Time.deltaTime*targetUpdate);

		newRotation = targetRotation;
		//ourAircraft.AIUpdateInput(targetRotation, newYaw);
	}
}
