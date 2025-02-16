using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The bomb behaviour doesn't need to be complicated. I'd say that a simple translate will work in all situations
public class PointLerpBomb : MonoBehaviour {

	public LayerMask TargetObjects;
	public float Fallspeed = 15f;
	public float GravityAccel = 10f;
	Vector3 DestinationPosition = Vector3.zero;

	public GameObject explosionEffect;
	public float GroundPanSpeed = 10f;
	GameObject groundObject;

	public float Damage = 750f; //I've no idea what this should be really
	public float ExplosionRadius = 10f;

	

	public void SetupBomb(Vector3 newDestinationPosition, GameObject newGroundObject)
    {
		DestinationPosition = newDestinationPosition;
		groundObject = newGroundObject;
    }
	
	// Update is called once per frame
	void Update () {
		//PROBLEM: Am unhappy with bomber bomb motion, but it works
		gameObject.transform.localPosition = Vector3.MoveTowards(gameObject.transform.localPosition, DestinationPosition, Time.deltaTime * Fallspeed);

		//Make our fallspeed act like gravity
		Fallspeed += Time.deltaTime * GravityAccel;

		
		if (Vector3.SqrMagnitude(gameObject.transform.localPosition-DestinationPosition) < 1f)	//We're close enough to our target
        {
			DoExplode();
        }
	}

	void DoExplode()
    {

		Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, ExplosionRadius);
		foreach (var hitCollider in hitColliders)
		{
			//hitCollider.SendMessage("AddDamage");
			DestructableObject thisObject = hitCollider.gameObject.GetComponent<DestructableObject>();
			if (thisObject)
            {
				thisObject.TakeDamage(Damage);	//This should really be changed according to the distance to the collision point?
            }
		}


		if (explosionEffect)
		{//can't go bang if we don't have a bang effect
			GameObject exp_Effect = Instantiate(explosionEffect, transform.position, transform.rotation) as GameObject;
			//exp_Effect.transform.parent = transform; //stick it to this for the duration...
			sfx_Explosion explosion = exp_Effect.GetComponent<sfx_Explosion>();
			exp_Effect.transform.SetParent(transform.parent);	//Make sure we're on the ground

			explosion.target = gameObject;
		}

		//PROBLEM: Need to add in our damage/decals etc
		Destroy(gameObject); //remove our bomb now that it's complete
	}
}
