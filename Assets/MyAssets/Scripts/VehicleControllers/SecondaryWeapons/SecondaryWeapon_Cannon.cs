using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondaryWeapon_Cannon : SecondaryWeapon_Base {
	public AttachedGun cannonGun;
	// Use this for initialization

	public override void DoFire()
	{
		if (Time.time > nextRefireTime)
        {
			if (UseAmmo(1))
			{
				nextRefireTime = Time.time + refireRate;
				if (cannonGun)
				{
					cannonGun.doFireEffect();
					//Increment our ammo accordingly
				}
			}
        }
	}
}
