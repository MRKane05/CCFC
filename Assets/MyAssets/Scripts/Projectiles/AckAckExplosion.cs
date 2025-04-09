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
	public float BlastJolt = 10f;
	public float SparkleDamage = 1f;
	public float SparkleJolt = 3f;
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
		AreaDamage(gameObject.transform.position, BlastDamage, BlastJolt, 0.125f);
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
			AreaDamage(gameObject.transform.position, SparkleDamage, SparkleJolt, 0.5f);
        }

		bIsActive = false;
		gameObject.SetActive(false);
    }

	//I don't have this anywhere already?
	public void AreaDamage(Vector3 position, float damage, float jolt, float falloff = 0.2f)	//Falloff means that damage dropps off from the center
    {
		//While this is a good approach it's probably faster to go through and check our actors individually as we always know there'll be a limited number of them
		//And it'll provide us with more information

		float distanceSqr = BlastRadius * BlastRadius;
		foreach (actorWrapper thisActor in ((LevelController)LevelControllerBase.Instance).friendlyList)
        {
			float actorDistance = Vector3.SqrMagnitude(gameObject.transform.position - thisActor.vehicle.transform.position);
			if (actorDistance < distanceSqr)
            {
				float radiusFalloff = Mathf.Clamp01(Mathf.Sqrt(actorDistance) / BlastRadius);
				float localDamage = Mathf.Lerp(damage, damage * falloff, radiusFalloff);
				thisActor.actor.takeDamage(damage, "FLAK", gameObject, Team, 0f);
				thisActor.actor.DoImpactJolt(position, jolt * radiusFalloff);
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
