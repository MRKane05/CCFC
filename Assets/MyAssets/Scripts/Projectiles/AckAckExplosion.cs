using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AckAckExplosion : MonoBehaviour {

	public ParticleSystem[] ChildParticleSystems;

	public int Team = 0;	//What is our team? This is used to control difficulty

	public bool bIsActive = false;
	public Vector3 targetLocation = Vector3.zero;
	//Move speed will be constant for readability
	public float EffectDuration = 4f;
	public float BlastRadius = 4f;
	public float BlastDamage = 7f;
	public float SparkleDamage = 1f;
	public float SparkleFrequency = 0.5f;
	float SparkleStart = 0;


	// Use this for initialization
	void Start () {
		ChildParticleSystems = gameObject.GetComponentsInChildren<ParticleSystem>();
	}
	
	public void SetupAckAck(Vector3 targetPoint, float triggerTime, int newTeam)
    {
		Team = newTeam;
		gameObject.transform.position = targetPoint - new Vector3(Random.Range(-2f, 2f), Random.Range(5f, 8f), Random.Range(-2f, 2f));	//Set our start location as below and to the side of where we'll explode
		targetLocation = targetPoint;
		StartCoroutine(DoAckAckBehavior(triggerTime));
    }

	IEnumerator DoAckAckBehavior(float detonateDelay)
    {
		bIsActive = true;
		float detonateTime = Time.time + detonateDelay;
		Vector3 startLocation = gameObject.transform.position;
		while (Time.time < detonateTime)
        {
			//Move to location
			gameObject.transform.position = Vector3.Lerp(targetLocation, startLocation, (detonateTime - Time.time) / detonateDelay);    //I guess?
			yield return null;
        }
		//Do our explosion
		AreaDamage(gameObject.transform.position, BlastDamage);
		foreach (ParticleSystem thisEmitter in ChildParticleSystems)
        {
			thisEmitter.Play();
        }
		AudioSource audio = gameObject.GetComponentInChildren<AudioSource>();
		audio.Play();

		//Handle the damage from our sparkles
		float sparkleTime = Time.time + EffectDuration;
		while (Time.time < sparkleTime)
        {
			yield return new WaitForSeconds(SparkleFrequency);
			AreaDamage(gameObject.transform.position, SparkleDamage);
        }

		bIsActive = false;
		gameObject.SetActive(false);
    }

	//I don't have this anywhere already?
	public void AreaDamage(Vector3 position, float damage)
    {
		//While this is a good approach it's probably faster to go through and check our actors individually as we always know there'll be a limited number of them
		//And it'll provide us with more information
		/*
		Collider[] hitColliders = Physics.OverlapSphere(position, BlastRadius);
		foreach (Collider hitCollider in hitColliders)
		{
			Actor hitActor = hitCollider.gameObject.GetComponent<Actor>();
			if (hitActor)
            {
				hitActor.takeDamage(damage, "FLAK", gameObject, Team, 0f);
            }
			//hitCollider.SendMessage("AddDamage");
			Debug.Log("Flak Hit: " + gameObject.name);
		}
		*/
		float distanceSqr = BlastRadius * BlastRadius;
		foreach (actorWrapper thisActor in ((LevelController)LevelControllerBase.Instance).friendlyList)
        {
			if (Vector3.SqrMagnitude(gameObject.transform.position - thisActor.vehicle.transform.position) < distanceSqr)
            {
				thisActor.actor.takeDamage(damage, "FLAK", gameObject, Team, 0f);
			}
        }

		foreach (actorWrapper thisActor in ((LevelController)LevelControllerBase.Instance).enemyList)
		{
			if (Vector3.SqrMagnitude(gameObject.transform.position - thisActor.vehicle.transform.position) < distanceSqr)
			{
				thisActor.actor.takeDamage(damage, "FLAK", gameObject, Team, 0f);
			}
		}
	}
}
