using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This is for using particles for bullets instead of objects
//Really this should live on the player to assure range and positioning, but I don't know if that'll be necessary
public class BulletParticleSystem : MonoBehaviour {
	private static BulletParticleSystem instance = null;
	public static BulletParticleSystem Instance { get { return instance; } }
	ParticleSystem particleSystem;

	// Use this for initialization
	void Start () {
		instance = this;
		particleSystem = gameObject.GetComponent<ParticleSystem>();
	}

	public void addParticle(Vector3 startPos, Vector3 startDirection, float velocity, float size, float lifetime)
	{
		//For the moment
		UnityEngine.ParticleSystem.Particle newParticle = new UnityEngine.ParticleSystem.Particle();
		newParticle.remainingLifetime = 3;
		newParticle.startLifetime = 3;
		//newParticle.rotation = fromThis.rotation;


		particleSystem.Emit(startPos, startDirection * velocity, size*particleSystem.startSize, lifetime, Color.white);

	}
}
