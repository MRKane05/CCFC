using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AckAckExplosion : MonoBehaviour {

	public ParticleSystem[] ChildParticleSystems;

	public bool bIsActive = false;
	public Vector3 targetLocation = Vector3.zero;
	//Move speed will be constant for readability
	float moveSpeed = 40f;

	// Use this for initialization
	void Start () {
		ChildParticleSystems = gameObject.GetComponentsInChildren<ParticleSystem>();
	}
	
	public void SetupAckAck(Vector3 targetPoint, float triggerTime)
    {
		gameObject.transform.position = targetPoint - new Vector3(Random.Range(-2f, 2f), Random.Range(5f, 8f), Random.Range(-2f, 2f));	//Set our start location as below and to the side of where we'll explode
		targetLocation = targetPoint;
		StartCoroutine(DoAckAckBehavior(triggerTime));
    }

	IEnumerator DoAckAckBehavior(float detonateDelay)
    {
		float detonateTime = Time.time + detonateDelay;
		Vector3 startLocation = gameObject.transform.position;
		while (Time.time < detonateTime)
        {
			//Move to location
			gameObject.transform.position = Vector3.Lerp(targetLocation, startLocation, (detonateTime - Time.time) / detonateDelay);    //I guess?
			yield return null;
        }

		foreach (ParticleSystem thisEmitter in ChildParticleSystems)
        {
			thisEmitter.Play();
        }
		AudioSource audio = gameObject.GetComponentInChildren<AudioSource>();
		audio.Play();
		Destroy(gameObject, 3.5f);	//Remove our effect
    }
}
