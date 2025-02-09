using UnityEngine;
using System.Collections;

//this is any gun attached to the aircraft.
public class AttachedGun : MonoBehaviour {
	public AI_Fighter ourFighterAI; //we're assigning this if we've got one.
	public GameObject baseVehicle;
	public Actor ourBaseActor;


	public GameObject gunFlare; //not sure what I'll need built into this.

	public bool bSynced=false;
	protected float forwardStep=-0.8F;
	public AudioClip firingSound;
	public GameObject bulletPrefab;
	public int team = 0;

	//the other option is putting a particle system on the gun itself, and having this system call it for whenever we want
	//to do a hit effect...
	//public GameObject hitPrefab; //this is a costy way of doing this really. Surely there's a cheaper way?

	public float RefireRate = 0.1F, Calibre = 0.303F, BulletSpeed=50, BulletLife=2; //refire speed of the bullets, and the size (hence damage) of them.
	public bool bFiring=false;
	protected bool bfirstFire=true;
	public int FireState=0;
	//public something GunFireEffect; 
	protected float ReFireTime;
	protected GameObject latestBullet;
	public float shotDistance=100F, damage=3F, spread;
	public float autoAimAngle = 7; //within 14 deg should be more than forgiving!
	protected Bullet latestBulletMP;


	float flareLifespan = 0.07F, flareDieTime;

	//public int targetLayer = 1<<9;
	public int targetLayer = 9; //only hit things on this layer.
								// Use this for initialization

	AudioSource ourAudio;

	ParticleEmitter ourHitEffect;
	void Start () {
		ourAudio = gameObject.GetComponent<AudioSource>();
		ourHitEffect = gameObject.GetComponentInChildren<ParticleEmitter>();
		//Flip target layer to bitmask
		targetLayer = 1<<targetLayer;

		if (firingSound)
			GetComponent<AudioSource>().clip = firingSound;
	}

	public void setController(Actor newBaseActor)
    {
		ourBaseActor = newBaseActor;
    }
	
	// Update is called once per frame
	void Update () {

		//we want to bleed down our muzzle flare.
		if (Time.time > flareDieTime && gunFlare)
			gunFlare.SetActive(false); //annull this.

		if (bFiring && bfirstFire && bSynced) { //then set forward our refire time
			ReFireTime = Time.time + RefireRate/2;
			bfirstFire = false;
		}
		else if (!bFiring)
			bfirstFire = true;
		
		if (bFiring && Time.time > ReFireTime) { //then lets launch another bullet!
			//Play the fire sound
			//Do our fire effect (which can be overwritten by other classes)
			doFireEffect();
		}
	}

	//this gunflare probably needs to get a little transparent because it is seriously obscuring the view!
	public void triggerMuzzleFlare() {
		gunFlare.SetActive(true); //make our flare shown
		gunFlare.transform.localScale = Vector3.one*Random.Range (0.8F, 1F); //Randomize size a little
		gunFlare.transform.localEulerAngles = new Vector3(0, 0, Random.Range (0, 360)); //spin offset it.

		flareDieTime = Time.time + flareLifespan;
	}

	public virtual void doFireEffect() {
		if (gunFlare)
			triggerMuzzleFlare(); //do the shot effect here also

		//make a bang!
		if (firingSound) {
			ourAudio.pitch = Random.Range(0.6f, 1f);
			ourAudio.PlayOneShot(firingSound);
		}

		float closestHit = float.MaxValue;
		Actor closestActor = null;
		
		Vector3 shotVector = (transform.forward + new Vector3(Random.Range(-spread, spread), Random.Range(-spread, spread), Random.Range(-spread, spread))).normalized;
		
		//assume for now that the player is allied
		//Over liberal auto-aim system
		if (((LevelController)LevelControllerBase.Instance).enemyList.Count > 0) {
			foreach (actorWrapper thisActor in ((LevelController)LevelControllerBase.Instance).enemyList) {
				Vector3 targetLead = thisActor.vehicle.transform.position + thisActor.vehicle.transform.forward*(transform.position - thisActor.vehicle.transform.position).magnitude *thisActor.actor.getSpeed()/BulletSpeed;
				
				float shotMagnitude = (transform.position - targetLead).magnitude;
				Vector3 hitPosition = transform.position + shotVector.normalized*shotMagnitude; //Basic vektor math...
				Debug.DrawLine(transform.position, hitPosition);
				//Debug.Log("Hit Disparity: " + (hitPosition-ourFighterAI.getTargetLead()).sqrMagnitude);
				//Check to see if we're hitting within this objects bounds.
				if (Vector3.Angle (transform.position-hitPosition, transform.position- thisActor.vehicle.gameObject.transform.position) < autoAimAngle || (hitPosition - targetLead).sqrMagnitude < thisActor.actor.hitSphere*thisActor.actor.hitSphere) { //then this is a hit!
					if (shotMagnitude < closestHit) {
						closestActor = thisActor.actor;
						closestHit = shotMagnitude;
					}
				}				
			}
		}

		//putting the damage stuff here removes the errors from the above array changing
		if (closestActor!=null && closestHit < shotDistance) {//do our damage here having sorted our hits
			closestActor.takeDamage(damage, "NORMAL", baseVehicle, team, closestHit/BulletSpeed);
			/*
			if (ourHitEffect) {
				Particle thisHit = new Particle();
				thisHit.position = closestActor.transform.position;
				ourHitEffect.Emit(thisHit);
			}
			*/

		}

		//Make the fire graphic
		//We could do with making this into an array that gets read from and bullets recycled - I thought this was already in the game but apparently it's not...
		
		latestBullet = Instantiate(bulletPrefab, transform.position+transform.forward.normalized*forwardStep, transform.rotation) as GameObject;
		latestBulletMP = latestBullet.GetComponent<Bullet>();
		//latestBulletMP.Movement = transform.forward*BulletSpeed;
		//SetBulletAction (Vector3 nMovement, GameObject nOwner, float nDamage, string nDamageType, string nTeam) {

		//we kind of need the shot vector here...
		latestBulletMP.SetBulletAction(shotVector*BulletSpeed, gameObject, 3F, "BULLET", "TEAM");
		
		
		Destroy(latestBullet, BulletLife); //this is equivilent to range I suppose
		
		ReFireTime = Time.time+RefireRate;
	
	}
	

}
