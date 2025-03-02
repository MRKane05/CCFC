using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A bomb to be dropped by one of the game bombers in the flight combat section
public class Bomber_Bomb : ProjectileBase {
    public float gravity = 10f;
    public float ExplosionRadius = 7f;

    public GameObject explosionPrefab;

    public override void DoUpdate()
    {
        base.DoUpdate();
        moveDir += -Vector3.up * gravity * Time.deltaTime;  //Accelerate due to gravity
    }

    public override void DoOnTriggerEnter(Collider other)
    {
        if (other.gameObject)
        {
            //Terrain thisTerrain = other.gameObject.GetComponent<Terrain>();
            if (other.gameObject.layer == 14) //Have we hit something that is on the ground layer?
            {
                //Do the explosion effect
                GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);

                //Do the explosion checking
                Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, ExplosionRadius);
                foreach (var hitCollider in hitColliders)
                {
                    //hitCollider.SendMessage("AddDamage");
                    DestructableObject thisObject = hitCollider.gameObject.GetComponent<DestructableObject>();
                    if (thisObject)
                    {
                        //Debug.Log("Taking Damage: " + damage);
                        thisObject.TakeDamage(damage);  //This should really be changed according to the distance to the collision point?
                    }
                }

                Destroy(gameObject);    //Kill this bomb
            }

        }
	}
}
