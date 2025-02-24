using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This should maybe be in a different place as it could apply to all parts of the game, but the bomber minigame is where it was made first
//I think that having this as an actor is over-complicating things, but we could make Actor into a class that extends off of this
public class DestructableObject : MonoBehaviour {

    public float Health = 500f; //I've no idea what this'll be
    public bool bDestroyed = false;
    public GameObject liveObject, destroyedObject;
    public GameObject explosionPrefab, burningPrefab;   //Our prefabs for effects

    void Start()
    {
        if (!liveObject || !destroyedObject)
        {
            //Debug.Log("GameObject: " + gameObject.name + " does not have live/destroyed meshes set");
        }
        //Set our necessary states
        if (liveObject && destroyedObject)
        {
            liveObject.SetActive(true);
            destroyedObject.SetActive(false);
        }
    }

    public virtual void TakeDamage(float damageAmount)
    {
        Health -= damageAmount;
        Debug.Log("Health: " + Health + " Damage: " + damageAmount);
        if (Health <=0 && !bDestroyed)
        {
            DoDestroy();
            //Destroy(gameObject); //remove this from the scene
            //PROBLEM: Need to add our scoring for this being a target or not to the levelController (or whatever is in charge of everything)
            if (((LevelControllerBomberMinigame)LevelControllerBase.Instance))
            {
                ((LevelControllerBomberMinigame)LevelControllerBase.Instance).ObjectDestroyed(gameObject, this);
            } //Else we'll use our 3D world levelcontroller. This isn't an eloquent way to solve this problem
        }
    }

    protected virtual void DoDestroy()
    {
        if (liveObject != null)
        {
            liveObject.SetActive(false);
        }
        if (destroyedObject != null)
        {
            destroyedObject.SetActive(true);
        }
        bDestroyed = true;

        //And our effects!
        if (explosionPrefab)
        {
            GameObject newExplosion = Instantiate(explosionPrefab) as GameObject;
            newExplosion.gameObject.transform.position = gameObject.transform.position;
            newExplosion.transform.SetParent(gameObject.transform);
            Destroy(newExplosion, 4f);//destroy our explosion after a limited amount of time
        }

        if (burningPrefab)
        {
            GameObject newBurning = Instantiate(burningPrefab) as GameObject;
            newBurning.gameObject.transform.position = gameObject.transform.position;
            newBurning.transform.SetParent(gameObject.transform);
            Destroy(newBurning, 15f); //Clear our burning, but later
        }

        if (!destroyedObject && !liveObject)
        {
            gameObject.GetComponent<MeshRenderer>().enabled = false; //Turn this off so it's not visible
        }
    }
}
