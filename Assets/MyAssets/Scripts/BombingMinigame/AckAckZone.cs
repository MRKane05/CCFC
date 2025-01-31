using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AckAckZone : MonoBehaviour {
	//Our zone will need a lifespawn and behavior, but for the moment lets just get it moving, and maybe hitting things
	public float panSpeed = 20f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		gameObject.transform.position -= Vector3.forward * panSpeed * Time.deltaTime;
	}
}
