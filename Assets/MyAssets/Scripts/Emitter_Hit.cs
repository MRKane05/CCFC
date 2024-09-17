using UnityEngine;
using System.Collections;

//handles all the hit effects that happen with this aircraft
public class Emitter_Hit : MonoBehaviour {
	public UnityEngine.ParticleSystem particleEmitter;
	//ParticleEmitter

	public void Start() { //might need to expand this to make it allow for multiple systems somehow?	
		particleEmitter = gameObject.GetComponent<UnityEngine.ParticleSystem>();
	}

	//while I'd like to be having it forward guess the hit location, for the moment we'll just do it actually
	public void addHit(float timeOffset) {
		StartCoroutine(waitForHit (timeOffset));
	}

	IEnumerator waitForHit(float delay) {
		yield return new WaitForSeconds(delay);
	
		//so this is where we should check between our present location and our expected location...
		//UnityEngine.ParticleSystem.Particle hitParticle = new UnityEngine.ParticleSystem.Particle();
		//hitParticle.position = transform.position;

		//particleEmitter.Emit(hitParticle); //emit this particle for our hit
		particleEmitter.Emit(1); //it's cheap and it works I suppose.

		yield return null; //end our coroutine
	}
}
