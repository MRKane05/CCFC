using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Something to control our player movement for the bombing aircraft
public class BombingAircraftHandler : MonoBehaviour {
	public float LimitX = 60;
	public Range LimitY = new Range(-40, 60);

	float rollShiftAmount = 20f;
	public float maxMoveSpeed = 30f;    //Again, I don't know!
	public float lerpHeaviness = 3f;	//How much the plane laggs with movement

	Vector3 playerVelocity = Vector3.zero;
	Vector3 targetPlayerVelocity = Vector3.zero;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		DoPlayerMove();
	}

	void DoPlayerMove()
    {
		//This isn't a good way of doing this, because we sill need to slow down...
		//This is option A...
		targetPlayerVelocity = Vector3.forward * Input.GetAxis("Left Stick Vertical") * maxMoveSpeed;
		targetPlayerVelocity += Vector3.right * Input.GetAxis("Left Stick Horizontal") * maxMoveSpeed;

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
		targetPlayerVelocity = Vector3.forward * YAxis * maxMoveSpeed;
		targetPlayerVelocity += Vector3.right * XAxis * maxMoveSpeed;

		//Set this up so that the plane feels "weighty" to control
		playerVelocity = Vector3.Lerp(playerVelocity, targetPlayerVelocity, Time.deltaTime * lerpHeaviness);

		//And lastly apply our velocity clamps
		playerVelocity = new Vector3(Mathf.Clamp(playerVelocity.x, -maxMoveSpeed, maxMoveSpeed), 0, Mathf.Clamp(playerVelocity.z, -maxMoveSpeed, maxMoveSpeed));

		//And apply our movement
		gameObject.transform.position += playerVelocity * Time.deltaTime;

		//Finally handle our roll :)
		gameObject.transform.eulerAngles = new Vector3(270+rollShiftAmount * playerVelocity.x / maxMoveSpeed, 90, 270); //This is weird because blender is right handed z up, and Unity is left handed Y up...because they wanted the world to burn I assume
	}
}
