using UnityEngine;
using System.Collections;

public class Engine : MonoBehaviour {

	float pitchMin = 0.3f, pitchMax = 2.0f; //engine noise
	float currentPitch; //also used to spin the propellor
	public float propSpinSpeed=10f;

	AudioSource engineSound;

	void Start() {
		engineSound = GetComponent<AudioSource>();
	}

	//this will send through a float from the aircraft which is indicitive of what noise we should be making...
	public void setPitch(float newPitch) {
		engineSound.pitch = Mathf.Lerp (pitchMin, pitchMax, newPitch);
		currentPitch = Mathf.Lerp (pitchMin, pitchMax, newPitch);
	}

	void Update() {
		transform.localEulerAngles += Vector3.forward * propSpinSpeed * Time.deltaTime * currentPitch;
	}
}
