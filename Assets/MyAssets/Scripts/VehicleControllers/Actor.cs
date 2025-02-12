using UnityEngine;
using System.Collections;

//The base class for anything that's...well shootable
public class Actor : MonoBehaviour {
	public MissionEventObject owner;
	public bool bIsPlayerVehicle=false;
	public int team = 0; //0 friendly 1 enemy (for this anyway)
	protected float speed=6;
	public float hitSphere = 1.2F; //what is our hit area?
	public bool bIsTarget=false; //is the player targeting this?
	protected bool bFiring, bLastFiring; //Our firing mechs
	protected Quaternion targetRotation;
	protected float roll, yaw, pitch, throttleControl;
	protected int FireState;
	public float health=100F, maxHealth = 100F;
	public GameObject ourRadarPrefab; //What blip do we make?
	public GameObject viewCenter, gunSight; //What the camera system will look at
	
	//this system handles the smoke stuff so that the player knows how much of a "dint" they're making
	//public ParticleEmitter ourSmokeEmitter; //should this be protected or public? Potentially public if we need to link up more than one.
	//protected ParticleAnimator ourSmokeAnimator; //used for the...well the animation of the smoke colors!

	protected Emitter_Hit hitEffect; //usually we've got a hit effect or so
	protected Emitter_Smoke smokeEffect; //because most things will smoke

	public GameObject[] popEffects;
	public GameObject explosionEffect; //because this'll be different depending on which vehicle we are

	public bool bIsDead=false; //Should we be applying the forces etc. this aircraft is on it's way out
	public bool isInvincible = false;

	//Actors need to fade in when they begin...
	protected float inFadeStart, inFadeTime = 2f;

	//And the movement variables :)
	public float rollspeed = 1.5F;      //Used by the AI for doing turns
	public float pitchspeed = 1.5F;
	public float yawspeed = 1.5F;       //Used by the AI for doing turns

	public float inFade {
		get { return Mathf.Clamp01((Time.time-inFadeStart)/inFadeTime); }
	}

	void Start() {
		hitEffect = gameObject.GetComponentInChildren<Emitter_Hit>();
		smokeEffect = gameObject.GetComponentInChildren<Emitter_Smoke>();

		inFadeStart = Time.deltaTime;
		DoStart();
	}

	public virtual void DoStart()
    {

    }


	public virtual void gradeSkill(float factor) {
		health*=factor;
		maxHealth*=factor;

	}

	public float getSpeed() {
		return speed; //So we can forward predict pretty much everything
	}
	
	public int getTeam() {
		return team;
	}
	
	public void setTeam(int newTeam) {
		team = newTeam;
	}

	void OnTriggerEnter(Collider withThis) {
		//Debug.LogError ("Hit " + withThis.gameObject.name);
		doCollision(withThis);
	}

	void OnTriggerStay(Collider withThis) { //we're in contact with something
		//is used with the terrain contact stuff (although this is getting messy)
		doContact(withThis);
	}

	void OnTriggerExit(Collider withThis) { //we just left contact here
		//not of much importance.
	}

	public virtual void doContact(Collider withThis) {}

	public virtual void doCollision(Collider withThis) {
		//handle the collision stuff
	}

	//These need to be here so that our control can be applied to any vehicle. Really this is a float class
	public void AIUpdateInput(Quaternion newTargetRotation, float newRoll, float newSpeed, bool bIsFiring) {
		bFiring = bIsFiring;
		targetRotation = newTargetRotation;
		roll = newRoll;
	}
	
	public void UpdateInput(float newPitch, float newRoll, float newYaw, float newThrottle, bool bIsFiring, int iFireState) {
		//print ("Called Update");
		pitch=newPitch; //positive is down
		roll=newRoll; //Positive is left
		yaw=newYaw;
		throttleControl=newThrottle;
		bFiring=bIsFiring;
		FireState = iFireState;
	}

	protected int popStage = 0;

	public virtual void checkSmokeSystem(float newHealthRatio) { //pinged with take Damage
		if (!smokeEffect)
			return; //we've got nothing to smoke
		/*
		if (!ourSmokeAnimator) {
			ourSmokeAnimator = ourSmokeEmitter.gameObject.GetComponent<ParticleAnimator>();
		}
		*/
		//so what are our levels for the system?
		//check our brackets!
		if (newHealthRatio > 0.33F && newHealthRatio < 0.66F && popStage!=1) {

			smokeEffect.setEmitState(true, Color.gray);

			popStage = 1;

			if (popEffects.Length > 1)
				Instantiate(popEffects[0], transform.position, transform.rotation);

		}
		else if (newHealthRatio < 0.33F && popStage != 2) {
			//ourSmokeEmitter.emit = true; //set this to emit!
			smokeEffect.setEmitState(true, Color.black);

			popStage = 2;

			if (popEffects.Length > 2)
				Instantiate(popEffects[1], transform.position, transform.rotation);
		}
	}

	//kind of explains why this isn't getting shot now doesn't it?
	public virtual void takeDamage(float thisDamage, string damageType, GameObject instigator, int damagingTeam, float delay) {
		//Debug.LogError("Getting Shot");

		//Make this invincible for the moment...
		if (!isInvincible) {
			health -= thisDamage;
			if (health<0 && !bIsDead) {
				//need to check and see if this is the player, and if it is then add the score up.
				if (instigator == PlayerController.Instance.ourAircraft.gameObject)
		((LevelController)LevelControllerBase.Instance).addKill(); //rack it up!
				
				//doExplode(); //whack this.
				bIsDead=true;
				//applyShotDown();
				//we can't remove this as it'll mean the player can't shoot it...
				//((LevelController)LevelControllerBase.Instance).removeSelf(gameObject, team); //but our targeting system needs to know I suppose
				//Destroy (gameObject); //Just destroy it for now
			}
			if (health<-maxHealth/2) { //then destroy this aircraft fully for whatever reason
				if (instigator == PlayerController.Instance.ourAircraft.gameObject)
		((LevelController)LevelControllerBase.Instance).addMurder(); //not a good thing actually, and we're counting it
				
	((LevelController)LevelControllerBase.Instance).removeSelf(gameObject, team); //For the moment I suppose
				Destroy (gameObject);
			}
		}
		if (owner)
		{
			owner.actorTakingDamage(owner, this, health / maxHealth);
		}
	}

	public virtual GameObject getModel() { //returns the model attached to this controller
		return gameObject; //plain entry
	}
	float explosionDelay = 2f; //how many seconds before we go boom
	int d_parachutes = 1;
	//do our shot down setup stuff...
	protected void applyShotDown()
	{
		StartCoroutine(shotDownProcess(0.5f, explosionDelay));
		callbackDie();  //Callback for this actor dying
	}

	public virtual void callbackDie()	//Called when we die and is responsible for some housekeeping
    {
		//So at this stage we've got to look at our group, and perhaps message back to our controller, which will then message back through the system
		//to complete any events that hinge on this group/object dying
		if (owner)
		{
			owner.actorCallbackDie(this);
		}
    }

	public virtual void doExplode(float d_delay)
	{ //is called with an explosive crash or when we're shot up enough
		if (explosionEffect)
		{//can't go bang if we don't have a bang effect
			GameObject exp_Effect = Instantiate(explosionEffect, transform.position, transform.rotation) as GameObject;
			//exp_Effect.transform.parent = transform; //stick it to this for the duration...
			sfx_Explosion explosion = exp_Effect.GetComponent<sfx_Explosion>();
			
			explosion.target = gameObject;
		}

		//Explode and delete this in the process
		((LevelController)LevelControllerBase.Instance).removeSelf(gameObject, team); //For the moment I suppose
		Destroy(gameObject, d_delay); //destroy aircraft after small "effect" delay
	}

	public void NPClerp(Vector3 targetLoc, float pullSpeed)
	{

		//the pullSpeed needs graded according to the distance to/from where we want to be...

		pullSpeed *= Mathf.Clamp01((transform.position - targetLoc).magnitude); //should be an easy switch

		//Basically gives the AI a helping hand to catch up to it's designated target, and keeps a status quo when it's at the cofrect point
		transform.position = Vector3.MoveTowards(transform.position, targetLoc, pullSpeed * Time.deltaTime); //pull this according to speed

	}

	public IEnumerator shotDownProcess(float parachuteDelay, float explosionDelay)
	{
		yield return new WaitForSeconds(parachuteDelay);
		doExplode(explosionDelay); //blow this fighter up
	}

	protected virtual void AircraftDeathSpiral()
    {
		if (transform.localRotation.eulerAngles.z < 90F || transform.localRotation.eulerAngles.z > 270)
			transform.RotateAround(transform.right, rollspeed * Time.deltaTime * 2f);
		else
			transform.RotateAround(transform.right, -rollspeed * Time.deltaTime * 2f);
	}
}
