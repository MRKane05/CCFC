using UnityEngine;
using System.Collections;

public class AttachedGun_AI : AttachedGun {
	protected Vector3 shotVector;
	
	//We need to know our AI controller to know what we're shooting at.
	
	//I think we need a "target information" class which has details for what we're shooting at.
	public override void doFireEffect() {
		if (ourFighterAI) { //Only if we've got a fighter attached to this.
			if (gunFlare)
				triggerMuzzleFlare(); //do the shot effect here also

			if (firingSound) {
				GetComponent<AudioSource>().pitch = Random.Range (0.4f, 0.9f); //vary the bullet shot a little
				GetComponent<AudioSource>().Play();
			}

			//Only do this if we've got a target.
			if (ourFighterAI.target != null || ourFighterAI.targetController!=null) {
				shotVector = (transform.forward + new Vector3(Random.Range(-spread, spread), Random.Range(-spread, spread), Random.Range(-spread, spread))).normalized;
			
				//Take the distance from here to the targets percieved point, and then multiply, then check the distance to see if we've got a hit
				if (ourFighterAI.targetController!=null) {
					float shotMagnitude = (transform.position - ourFighterAI.getTargetLead()).magnitude;
					Vector3 hitPosition = transform.position + shotVector.normalized*shotMagnitude; //Basic vektor math...
					Debug.DrawLine(transform.position, hitPosition);
					//Debug.Log("Hit Disparity: " + (hitPosition-ourFighterAI.getTargetLead()).sqrMagnitude);
					//Check to see if we're hitting within this objects bounds.
					if ((hitPosition-ourFighterAI.getTargetLead()).sqrMagnitude < ourFighterAI.targetController.hitSphere*ourFighterAI.targetController.hitSphere) {
						
						ourFighterAI.targetController.takeDamage(damage, "NORMAL", ourFighterAI.ourAircraft.gameObject, ourBaseActor.getTeam(), shotMagnitude/BulletSpeed);
					}
				}			
			}
			else if (ourFighterAI.targetController!=null) {
				//should really be checking for "overkill" here I guess...

			}
		}
				
		//Make the fire graphic
		latestBullet = Instantiate(bulletPrefab, transform.position+transform.forward.normalized*forwardStep, transform.rotation) as GameObject;
		latestBulletMP = latestBullet.GetComponent<Bullet>();
		//latestBulletMP.Movement = transform.forward*BulletSpeed;
		//SetBulletAction (Vector3 nMovement, GameObject nOwner, float nDamage, string nDamageType, string nTeam) {
		latestBulletMP.SetBulletAction(shotVector*BulletSpeed, gameObject, 3F, "BULLET", "TEAM");
		
		
		Destroy(latestBullet, BulletLife); //this is equivilent to range I suppose
		ReFireTime = Time.time+RefireRate;
	}
	/*
	public virtual void doFireEffect() {
					
		//Get our shot vector and factor in the accuracy of the shot.
		Vector3 shotVector = (transform.forward + new Vector3(Random.Range(-spread, spread), Random.Range(-spread, spread), Random.Range(-spread, spread))).normalized;
		
		//Try doing a fire raycast as we're slightly shifting the system approach.
		RaycastHit hit;
		//Physics.Raycast(Ray, distance, LayerMask);
		Debug.DrawLine(transform.position, transform.position+shotVector*shotDistance);
		Ray ourRay = new Ray(transform.position, shotVector);
		
		//Sort this out so that we can only hit things on this layer
		//This is all very well until we're working with an AI...
		if (Physics.Raycast(ourRay, out hit, shotDistance, targetLayer)) {
			//Debug.Log ("Hit: " + hit.collider.gameObject.name);
			//So now we need to find the AircraftController attached to this object.
			AircraftController thisAircraftMP = hit.collider.gameObject.transform.parent.gameObject.GetComponent<AircraftController>() as AircraftController;
			thisAircraftMP.takeDamage(damage, "NORMAL", transform.parent.parent.gameObject); //return our parent, but this is a messy process...
			
		}
		
    	//if (Physics.Raycast(transform.position, shotVector*shotDistance, out hit)) {
		//	Debug.Log ("Hit: " + hit.collider.gameObject.name);
			
		
		//}
        	
		
		
		//Make the fire graphic
		latestBullet = Instantiate(bulletPrefab, transform.position+transform.forward.normalized*forwardStep, transform.rotation) as GameObject;
		latestBulletMP = latestBullet.GetComponent<Bullet>();
		//latestBulletMP.Movement = transform.forward*BulletSpeed;
		//SetBulletAction (Vector3 nMovement, GameObject nOwner, float nDamage, string nDamageType, string nTeam) {
		latestBulletMP.SetBulletAction(shotVector*BulletSpeed, gameObject, 3F, "BULLET", "TEAM");
		
		
		Destroy(latestBullet, BulletLife); //this is equivilent to range I suppose
		ReFireTime = Time.time+RefireRate;
	
	}
	*/
}
