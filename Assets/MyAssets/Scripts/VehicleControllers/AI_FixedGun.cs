using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

//A fixed gun that'll shoot at enemy targets
public class AI_FixedGun : MonoBehaviour {
	public GameObject baseVehicle;

	public int team = 0;
	public float damage = 3;
	public actorWrapper target;
	public float range = 30f;
	public float BulletSpeed = 50f;

	float targetNext = 0;
	float targetTrackTime = 3f;

	float nextShotTime = 0;
	public float refireTime = 0.5f;

	public Range upDot_Range = new Range(0f, 1f);   //What's our range of fire limitation for the vector up dot
	public Range forwardDot_Range = new Range(0.3f, 1f); //What our range of avaliable fire on the forward dot?

	public GameObject muzzleFlare;
	public float spread = 0.03f;    //This needs to be keyed in

	public GameObject bulletPrefab;
	GameObject latestBullet;
	float forwardStep = 0.5f; //For emitting prefab shots ahead of our emitter

	AudioSource ourAudio;
	public AudioClip shotSound;

	// Use this for initialization
	void Start () {
		ourAudio = gameObject.GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
		if (!target.actor.gameObject)
		{
			targetNext = Time.time + targetTrackTime;	//Apply a timer for how long we'll track a target
			target = ((LevelController)LevelControllerBase.Instance).requestRangedTarget(team, range, gameObject.transform.position);   //Of course if there are no targets this could really chew up process
		}
		else
        {
			TrackTarget();
        }
		/*
		if (Vector3.SqrMagnitude(target.transform.position - gameObject.transform.position) > range*range || Time.time > targetNext)
        {
			target = null; //Clear our target as it's out of range
        }*/
	}

	public virtual void TrackTarget()
    {
		//This could be complicated. For the moment it won't be
		//transform.LookAt(target.transform, Vector3.up);
		Quaternion lookAtAngle = Quaternion.LookRotation(target.actor.gameObject.transform.position - gameObject.transform.position, gameObject.transform.up);
		float upDot = Vector3.Dot(transform.up, lookAtAngle * Vector3.forward);
		float forwardDot = Vector3.Dot(transform.forward, lookAtAngle * Vector3.forward);
		//Debug.Log("upDot: " + upDot + " forwardDot: " + forwardDot);
		if (upDot_Range.ValueWithin(upDot) && forwardDot_Range.ValueWithin(forwardDot))
		{
			Debug.DrawLine(gameObject.transform.position, gameObject.transform.position + lookAtAngle * Vector3.forward * 10f, Color.green);
			if (Time.time > nextShotTime)
            {
				nextShotTime = Time.time + refireTime;
				DoShot(lookAtAngle);
            }
		}
		else
		{
			Debug.DrawLine(gameObject.transform.position, gameObject.transform.position + lookAtAngle * Vector3.forward * 10f, Color.red);
		}
		//Do our look at angle, I'd love to have the firing solution sweep across a target
		//Need to limit the angle that our turret can have
		//Handle shooting

    }

	void DoShot(Quaternion lookAtAngle)
    {
		//We'll need to do the muzzle flare thing
		if (muzzleFlare)
        {
			//we need to rotate this to face our shooting direction...
			muzzleFlare.SetActive(true);
			muzzleFlare.transform.rotation = lookAtAngle;
			muzzleFlare.transform.localScale = Vector3.one * Random.Range(0.75f, 1.1f); //Random size
			Sequence muzzleSequence = DOTween.Sequence();	//try a different pause and show approach
			muzzleSequence.AppendInterval(0.125f).OnComplete(() => { muzzleFlare.SetActive(false); });  //Not sure how long this muzzle flash should be on for
        }
		//Fire a bullet
		//Do a trace

		//Bit of bulk copy and pasting from the aircraft firing systems (could this be regrouped?)
		if (true)
		{
			Vector3 shotVector = (lookAtAngle * Vector3.forward + new Vector3(Random.Range(-spread, spread), Random.Range(-spread, spread), Random.Range(-spread, spread))).normalized;

			//Take the distance from here to the targets percieved point, and then multiply, then check the distance to see if we've got a hit
			//Unlike the insainely optimised code for hit checking on aircraft we might have to raycast this one

			latestBullet = Instantiate(bulletPrefab, transform.position + transform.forward.normalized * forwardStep, transform.rotation) as GameObject;
			latestBullet.transform.rotation = lookAtAngle;
			Bullet latestBulletMP = latestBullet.GetComponent<Bullet>();
			//latestBulletMP.Movement = transform.forward*BulletSpeed;
			//SetBulletAction (Vector3 nMovement, GameObject nOwner, float nDamage, string nDamageType, string nTeam) {
			latestBulletMP.SetBulletAction(shotVector * BulletSpeed, gameObject, 3F, "BULLET", "TEAM");


			Destroy(latestBullet, 3); //this is equivilent to range I suppose


			if (true) //ourFighterAI.targetController != null)
			{
				float targetDistance = Vector3.Magnitude(transform.position - target.actor.gameObject.transform.position);
				Vector3 targetLead = target.actor.gameObject.transform.position + target.actor.gameObject.transform.forward * (targetDistance * target.actor.getSpeed() / 50F);
				float shotMagnitude = (transform.position - targetLead).magnitude;
				Vector3 hitPosition = transform.position + shotVector.normalized * shotMagnitude; //Basic vektor math...
				Debug.DrawLine(transform.position, hitPosition);
				//Debug.Log("Hit Disparity: " + (hitPosition-ourFighterAI.getTargetLead()).sqrMagnitude);
				//Check to see if we're hitting within this objects bounds.
				if ((hitPosition - targetLead).sqrMagnitude < target.actor.hitSphere * target.actor.hitSphere)
				{
					target.actor.takeDamage(damage, "NORMAL", baseVehicle, team, shotMagnitude / BulletSpeed);
					//ourFighterAI.targetController.takeDamage(damage, "NORMAL", ourFighterAI.ourAircraft.gameObject, ourBaseActor.getTeam(), shotMagnitude / BulletSpeed);
				}
			}
		}

		ourAudio.PlayOneShot(shotSound);
	}
}
