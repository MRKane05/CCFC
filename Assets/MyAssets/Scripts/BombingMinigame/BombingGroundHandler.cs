using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Eventually this class will also be responsible for assembling the ground underneath us that we'll see while going up for this run. The idea is that
//the ground will move, but the camera/plane will remain locked in place (which could become problematic, I dunno)

public class BombingGroundHandler : MonoBehaviour {
	public float panSpeed = 3f;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		gameObject.transform.position -= Vector3.forward * panSpeed * Time.deltaTime;
	}
}
