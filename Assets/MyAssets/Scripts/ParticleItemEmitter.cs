using UnityEngine;
using System.Collections;

public class ParticleItemEmitter : MonoBehaviour {

	float lastEmitTime=-1;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		if (Time.time > lastEmitTime) {
			ParticleController.Instance.addParticle(transform);
			lastEmitTime = Time.time+1;
		}
	}
}
