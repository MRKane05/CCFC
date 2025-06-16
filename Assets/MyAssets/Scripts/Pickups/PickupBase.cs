using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PickupBase : MonoBehaviour {
	public enum enPickupType { NULL, HEALTH, AMMO, SECONDARYAMMO, DOUBLEDAMAGE, COOLDOWN, PHOTO }
	public enPickupType PickupType = enPickupType.HEALTH;


	public GameObject Instigator;   //What was the thing that dropped us?
	public float Lifespan = 15f;    //How long will we be alive for? If this is zero we're always present in the scene
	public float PickupValue = 10f;	//How much of whatever do we add?
	float DieTime = 0;
	public bool bIsParachute = true;    //Will this be behaving like something that pops up and then floats down?
	[Space]
	[Header("Scaling stuff for attention")]
	public bool bScaleWithDistance = false;
	public float scaleDistance = 15f;   //This will be the square distance for speed
	public Transform scaleTransform;

	float gravity = 10f;
	float fallspeed = 1f;   //What's our maximum falling speed?
	float initialSpeed = 5f;

	Vector3 moveVelocity = Vector3.zero;

	// Use this for initialization
	void Start () {
		//DoPickupStart();
		if (PickupType == enPickupType.PHOTO)
        {
			gameManager.Instance.addSpawn("Photo", 0.5f);
        }
	}

	public void DoPickupStart(GameObject newInstigator)
    {
		Instigator = newInstigator;
		DieTime = Time.time + Lifespan;
		moveVelocity = new Vector3(Random.Range(-1f, 1f), initialSpeed, Random.Range(-1f, 1f));	//Set our initial velocity
	}
	
	// Update is called once per frame
	void Update () {
		if (bIsParachute)
		{
			transform.position += moveVelocity * Time.deltaTime;
			moveVelocity -= Vector3.up * gravity * Time.deltaTime;
			if (moveVelocity.y < -fallspeed)
			{
				moveVelocity.y = -fallspeed;
			}
		}

		if (Time.time > DieTime && Lifespan > 0)
        {
			Destroy(gameObject);
        }

		if (bScaleWithDistance && scaleTransform)
        {
			scaleTransform.localScale = Vector3.one * (1f- Mathf.Clamp01(Vector3.SqrMagnitude(gameObject.transform.position - Camera.main.transform.position) / scaleDistance));
        }
	}
}
