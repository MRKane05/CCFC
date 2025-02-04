using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
//base class for all things particle in nature
public class ParticleController : MonoBehaviour {
	private static ParticleController instance = null;
	public static ParticleController Instance {get {return instance;}}

	private ParticleSystem particleSystem;	
	private UnityEngine.ParticleSystem.Particle[] particles, tempSystem; //HA! It worked! This deserves a comment
	
	int particleCount;
	
	//Prepare our particle system
	protected void Awake () {
		particleSystem = gameObject.GetComponent<ParticleSystem>();

		if (instance) {
			Debug.Log("Duplicate attempt to create ParticleController");
			Destroy(this);
			return;
		}
		
		instance = this;
	}
	
	public void addParticle(Transform fromThis, float velocity, float size, float lifetime) {
		//For the moment
		UnityEngine.ParticleSystem.Particle newParticle = new UnityEngine.ParticleSystem.Particle();
		newParticle.remainingLifetime=3;
		newParticle.startLifetime=3;
		//newParticle.rotation = fromThis.rotation;


		particleSystem.Emit(fromThis.position, fromThis.forward*velocity, size, lifetime, Color.white);
			
	}
}
