using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AckAckZone : MonoBehaviour {
	//Our zone will need a lifespawn and behavior, but for the moment lets just get it moving, and maybe hitting things
	public float panSpeed = 20f;
	public float damageFrequency = 0.5f;
	public float damagePerTick = 5f;
	float nextDamageTime = 0;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		gameObject.transform.position -= Vector3.forward * panSpeed * Time.deltaTime;
	}

	void OnTriggerStay(Collider other)
	{
		BombingAircraftHandler targetHandler = other.gameObject.GetComponent<BombingAircraftHandler>();
		if (targetHandler)
        {
			//We need to do a DPS to this somehow, like one damage ever 0.5s
			if (Time.time > nextDamageTime)
            {
				nextDamageTime = Time.time + damageFrequency;
				targetHandler.TakeDamage(10);
            }
        }
	}
}
