using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrafingTurretHandler : DestructableObject {
	public GameObject turretHead;
	public GameObject projectilePrefab;
	public float projectileSpeed = 25f;
	public float projectileDamage = 7f;
	float refireTime = 0;
	public float refireRate = 1f;
	public float activeRange = 30;	//We don't shoot outside of this range


	// Update is called once per frame
	void Update () {
		if (turretHead && StrafingAircraftHandler.Instance)
        {
			TrackPlayer();
        }
	}

	void TrackPlayer()
    {
		if (Mathf.Abs(StrafingAircraftHandler.Instance.transform.position.z - gameObject.transform.position.z) < activeRange && !bDestroyed)
		{
			turretHead.transform.LookAt(StrafingAircraftHandler.Instance.transform, Vector3.up);
			if (Time.time > refireTime)
			{
				refireTime = Time.time + refireRate;
				SpawnProjectile();
			}
		}
    }

	void SpawnProjectile()
    {
		GameObject newProjectile = Instantiate(projectilePrefab) as GameObject;
		newProjectile.transform.position = turretHead.transform.position;
		ProjectileBase projectileScript = newProjectile.GetComponent<ProjectileBase>();
		projectileScript.SetupProjectile(gameObject, turretHead.transform.forward, projectileSpeed, projectileDamage);
		Destroy(newProjectile, 5f);	//Limit how long this can go on for
    }
}
