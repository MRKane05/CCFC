using UnityEngine;
using System.Collections;

//Handles smoke that comes out of this aircraft. Setup so that we can have smoke sources etc.
public class Emitter_Smoke : MonoBehaviour {
	public UnityEngine.ParticleSystem particleEmitter;
	//ParticleEmitter
	
	public void Start() { //might need to expand this to make it allow for multiple systems somehow?	
		particleEmitter = gameObject.GetComponent<UnityEngine.ParticleSystem>();
		setEmitState(false, Color.white);
	}

	//need to sort out our colors for the levels.
	public void setEmitState(bool bIsOn, Color newColor) {
		if (particleEmitter) {
			particleEmitter.enableEmission = bIsOn; //and that's about it actually

			particleEmitter.startColor = newColor; //set the color of the smoke (handy for doing trails and fancy stuff like that)
		}
	}
}
