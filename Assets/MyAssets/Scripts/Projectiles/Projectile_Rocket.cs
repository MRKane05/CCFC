using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile_Rocket : ProjectileBase {
    public float ExplosionRadius = 7f;

    public GameObject explosionPrefab;

    public int Team = 0;    //So that we don't damage friendlies

    public Collider ourCollider;

    public float safeSquareDistance = 2.25f; //How far away from our Instigator do we activate our collider
    
    public override void DoUpdate()
    {
        base.DoUpdate();
        if (Vector3.SqrMagnitude(gameObject.transform.position-Instigator.transform.position) > safeSquareDistance)
        {
            ourCollider.enabled = true; //turn our collider on
        }
    }

    public override void DoOnTriggerEnter(Collider other)
    {
        if (other.gameObject)
        {
            //First out list of nullchecks
            if (other.gameObject == Instigator) { return; } //Don't hit our player who launched this rocket


            //Terrain thisTerrain = other.gameObject.GetComponent<Terrain>();
            if (other.gameObject.layer == 14 || other.gameObject.layer == 0) //We've hit the ground, or a Default object
            {
                //Do the explosion effect
                GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);

                //Do the explosion checking
                Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, ExplosionRadius);
                foreach (var hitCollider in hitColliders)
                {
                    if (hitCollider.gameObject != gameObject)
                    {
                        //hitCollider.SendMessage("AddDamage");
                        //I suppose we could be shooting at buildings...
                        DestructableObject thisObject = hitCollider.gameObject.GetComponent<DestructableObject>();
                        if (thisObject)
                        {
                            //Debug.Log("Taking Damage: " + damage);
                            thisObject.TakeDamage(damage);  //This should really be changed according to the distance to the collision point?
                        }
                        if (!thisObject)
                        {
                            Actor thisActor = hitCollider.gameObject.GetComponent<Actor>();
                            if (thisActor)
                            {
                                thisActor.takeDamage(damage, "EXP", Instigator, Team, 0f);
                            }
                        }
                    }
                }

                Destroy(gameObject);    //Kill this bomb
            }

        }
    }
}
