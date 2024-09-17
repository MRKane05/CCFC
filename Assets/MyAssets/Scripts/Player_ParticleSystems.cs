using UnityEngine;
using System.Collections;

public class Player_ParticleSystems : MonoBehaviour {

	public ParticleSystem atmosphere, speedStreaks;

	float atmosphereMin = 20f, atmosphereGrad = 30f, atmosphereRate = 3;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	float streaksMin = 4, streaksMax = 6, streaksRate = 10; //erm...?

	void Update () {
		//atmosphere emission needs tweaked according to the height we are at...
		if (atmosphere) {
			atmosphere.emissionRate = atmosphereRate * Mathf.Clamp01((PlayerController.Instance.mAltitude-atmosphereMin)/atmosphereGrad);
		}
		if (speedStreaks) {
			speedStreaks.emissionRate = streaksRate*Mathf.Clamp01((PlayerController.Instance.ourAircraft.getSpeed()-streaksMin)/(streaksMax/streaksMin));
			speedStreaks.startSpeed = PlayerController.Instance.ourAircraft.getSpeed ()*2f; //that'll do
		}
	}
}
