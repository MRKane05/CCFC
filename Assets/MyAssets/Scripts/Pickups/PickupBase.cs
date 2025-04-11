using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PickupBase : MonoBehaviour {
	public enum enPickupType { NULL, HEALTH, AMMO, DOUBLEDAMAGE, COOLDOWN }
	public enPickupType PickupType = enPickupType.HEALTH;



	public GameObject Instigator;   //What was the thing that dropped us?
	public float Lifespan = 30f;    //How long will we be alive for?
	public float PickupValue = 10f;	//How much of whatever do we add?
	float DieTime = 0;
	public bool bIsParachute = true;	//Will this be behaving like something that pops up and then floats down?

	// Use this for initialization
	void Start () {
		DieTime = Time.time + Lifespan;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
