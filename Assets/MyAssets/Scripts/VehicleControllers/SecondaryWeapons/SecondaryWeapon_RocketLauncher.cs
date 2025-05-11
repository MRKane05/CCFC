using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondaryWeapon_RocketLauncher : SecondaryWeapon_Base {

	public GameObject rocketPrefab;

	public override void DoFire()
	{
		if (Time.time > nextRefireTime)
		{
			if (UseAmmo(1))
			{
				nextRefireTime = Time.time + refireRate;

				GameObject newRocket = Instantiate(rocketPrefab, transform.position, transform.rotation) as GameObject;

				ProjectileBase rocketScript = newRocket.GetComponent<ProjectileBase>();
				rocketScript.SetupProjectile(weaponOwner, transform.forward, -1, -1);
				Destroy(newRocket, 3f); //Give this a kill timer
				if (ourAudio)
				{
					ourAudio.PlayOneShot(ourAudio.clip);
				}
			}
		}
	}
}
