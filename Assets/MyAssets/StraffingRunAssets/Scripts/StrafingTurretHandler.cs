using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrafingTurretHandler : DestructableObject {
	public GameObject turretHead;
	public ParticleController ourParticleController;


	// Update is called once per frame
	void Update () {
		if (turretHead && StrafingAircraftHandler.Instance)
        {
			TrackPlayer();
        }
	}

	void TrackPlayer()
    {
		turretHead.transform.LookAt(StrafingAircraftHandler.Instance.transform, Vector3.up);
    }
}
