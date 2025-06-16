using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//All this script does it rotate something around
public class SimpleRotator : MonoBehaviour {
	public Vector3 RotateRate = Vector3.zero;

	// Update is called once per frame
	void Update () {
		gameObject.transform.localEulerAngles += RotateRate * Time.deltaTime;
	}
}
