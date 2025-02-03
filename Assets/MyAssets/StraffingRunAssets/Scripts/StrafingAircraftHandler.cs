using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MuzzleFlareType
{
	public GameObject MuzzleFlare;
	public float OffTime = 0;
	public void ShowMuzzleFlare()
    {
		OffTime = Time.time + 0.25f; //How long until our flare blinks off
		MuzzleFlare.transform.localEulerAngles = new Vector3(90, Random.Range(0, 360), 90);
		MuzzleFlare.transform.localScale = Vector3.one * Random.Range(0.75f, 1.1f);
		MuzzleFlare.SetActive(true);
    }
}

//Something to control our player movement for the bombing aircraft
public class StrafingAircraftHandler : MonoBehaviour {
	public LayerMask hitLayers;

	public Range LimitX = new Range(-20, 7);
	public Range LimitYClose = new Range(-40, 60);
	public Range LimitYFar = new Range(-40, 60);
	public float aircraftHeight = 12;


	float rollShiftAmount = 20f;
	public float maxMoveSpeed = 30f;    //Again, I don't know!
	public float lerpHeaviness = 3f;	//How much the plane laggs with movement

	Vector3 playerVelocity = Vector3.zero;
	Vector3 targetPlayerVelocity = Vector3.zero;

	public float PlayerMaxHealth = 700f;
	public float PlayerHealth = 700f; //Why 700? I dunno

	public List<MuzzleFlareType> MuzzleFlares = new List<MuzzleFlareType>();
	public List<GameObject> GunTargetPoints = new List<GameObject>();

	bool bFireLeft = true;
	float refireTime = 0.15f;
	float nextFireTime = 0;

	public AudioClip fireSound;
	AudioSource ourAudio;

	public GameObject groundPlane;
	public GameObject bulletHitDirtEffect;
	public float damageAmount = 10;

	// Use this for initialization
	void Start () {
		ourAudio = gameObject.GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
		DoPlayerMove();
		//And do shooting stuff!
		if (Input.GetButton("Right Shoulder") || Input.GetKey(KeyCode.Space))
        {
			if (Time.time > nextFireTime)
            {
				bFireLeft = !bFireLeft;
				nextFireTime = Time.time + refireTime;
				doShot(bFireLeft ? 1: 0);
            }
        }

		//Turn off our muzzle flare
		foreach (MuzzleFlareType thisMuzzleFlare in MuzzleFlares)
        {
			if (Time.time > thisMuzzleFlare.OffTime)
            {
				thisMuzzleFlare.MuzzleFlare.SetActive(false);
            }
        }
	}

	void doShot(int cannonEntry)
    {
		ourAudio.PlayOneShot(fireSound);
		MuzzleFlares[cannonEntry].ShowMuzzleFlare();

		RaycastHit hit;
		Vector3 RayDir = Vector3.Normalize(GunTargetPoints[cannonEntry].transform.position- MuzzleFlares[cannonEntry].MuzzleFlare.transform.position);
		// Does the ray intersect any objects excluding the player layer

		if (Physics.Raycast(MuzzleFlares[cannonEntry].MuzzleFlare.transform.position, RayDir, out hit, Mathf.Infinity, hitLayers))

		{
			Debug.DrawRay(MuzzleFlares[cannonEntry].MuzzleFlare.transform.position, RayDir * hit.distance, Color.yellow);
			//Debug.Log("Did Hit");

			//Check to see if we've hit something
			DestructableObject thisDestructable = hit.collider.gameObject.GetComponent<DestructableObject>();
			if (thisDestructable)
            {
				thisDestructable.TakeDamage(damageAmount);
            }

			//Check to see if this is dirt, but for the moment who cares, lets just put a particle effect in here
			GameObject hitEffect = Instantiate(bulletHitDirtEffect) as GameObject;
			hitEffect.transform.position = hit.point;
			hitEffect.transform.SetParent(groundPlane.transform);
			Destroy(hitEffect, 1f);	//And kill our particle when we're finished
		}
		else
		{
			Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white);
			Debug.Log("Did not Hit");
		}
	}

	void DoPlayerMove()
    {
		//This isn't a good way of doing this, because we sill need to slow down...
		//This is option A...
		targetPlayerVelocity = -Vector3.right * Input.GetAxis("Left Stick Vertical") * maxMoveSpeed;
		targetPlayerVelocity += Vector3.forward * Input.GetAxis("Left Stick Horizontal") * maxMoveSpeed;

#if UNITY_EDITOR
		float XAxis = 0;
		float YAxis = 0;
		//But we need to be able to test this on the computer, so:
		if (Input.GetKey(KeyCode.LeftArrow))
		{
			XAxis = -1;
		}
		else if (Input.GetKey(KeyCode.RightArrow))
		{
			XAxis = 1;
		}

		if (Input.GetKey(KeyCode.UpArrow))
		{
			YAxis = 1;
		}
		else if (Input.GetKey(KeyCode.DownArrow))
		{
			YAxis = -1;
		}
		targetPlayerVelocity = Vector3.forward * XAxis * maxMoveSpeed;
		targetPlayerVelocity -= Vector3.right * YAxis * maxMoveSpeed;
#endif

		//Set this up so that the plane feels "weighty" to control
		playerVelocity = Vector3.Lerp(playerVelocity, targetPlayerVelocity, Time.deltaTime * lerpHeaviness);

		//And lastly apply our velocity clamps
		playerVelocity = new Vector3(Mathf.Clamp(playerVelocity.x, -maxMoveSpeed, maxMoveSpeed), 0, Mathf.Clamp(playerVelocity.z, -maxMoveSpeed, maxMoveSpeed));

		//And apply our movement
		gameObject.transform.position += playerVelocity * Time.deltaTime;

		//Compound lerp to make sure that the plane positions itself within the screenspace
		float inverseScreenPos = Mathf.InverseLerp(LimitX.Min, LimitX.Max, gameObject.transform.position.x);
		//And clamp our poisition so that we can't exit the bounds of the screen
		gameObject.transform.position = new Vector3(Mathf.Clamp(gameObject.transform.position.x, LimitX.Min, LimitX.Max), aircraftHeight,
			Mathf.Clamp(gameObject.transform.position.z, Mathf.Lerp(LimitYFar.Min, LimitYClose.Min, inverseScreenPos), Mathf.Lerp(LimitYFar.Max, LimitYClose.Max, inverseScreenPos)));

		//Finally handle our roll :)
		gameObject.transform.eulerAngles = new Vector3(270+rollShiftAmount * playerVelocity.x / maxMoveSpeed, 90, 270); //This is weird because blender is right handed z up, and Unity is left handed Y up...because they wanted the world to burn I assume
	}

	public void TakeDamage(float damageAmount)
    {
		//Do some effect, play some noise, but for the moment
		//Debug.LogError("Player taking Damage");
		PlayerHealth -= damageAmount;

		float healthFraction = PlayerHealth / PlayerMaxHealth;
		NGUI_Base.Instance.assignHealth(healthFraction);

		if (PlayerHealth <= 0)
        {
			((LevelControllerBomberMinigame)LevelControllerBase.Instance).finishMatch(true);	//Do our fail state
        }
		//PROBLEM: This needs to play a sound and apply some sort of visual effect so that the player knows we're taking damage
    }
}
