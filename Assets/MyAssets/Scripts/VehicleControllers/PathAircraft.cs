using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This is an actor (like a bomber) that follows a set path (in this case: forward)
public class PathAircraft : Actor {
    public float crusingSpeed = 4;
    void Update()
    {
        speed = crusingSpeed;
        transform.position += transform.forward * Time.deltaTime * speed;   //Simply move this vehicle forward
    }

	public override void takeDamage(float thisDamage, string damageType, GameObject instigator, int damagingTeam, float delay)
	{
        base.takeDamage(thisDamage, damageType, instigator, damagingTeam, delay);
        //PROBLEM: We need to update the players UI just for this particular type of aircraft (this is going to get screwy when we add more than one, but for the moment it's getting things on the ground)
        float damageProp =health / maxHealth;

        //update our health bar
        NGUI_Base.Instance.assignHealth(damageProp);
    }
}
