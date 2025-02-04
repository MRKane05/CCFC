using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This should maybe be in a different place as it could apply to all parts of the game, but the bomber minigame is where it was made first
//I think that having this as an actor is over-complicating things, but we could make Actor into a class that extends off of this
public class DestructableObject : MonoBehaviour {

    public float Health = 500f; //I've no idea what this'll be
    public bool bDestroyed = false;
    public GameObject liveObject, destroyedObject;


    void Start()
    {
        if (!liveObject || !destroyedObject)
        {
            Debug.LogError("GameObject: " + gameObject.name + " does not have live/destroyed meshes set");
        }
        //Set our necessary states
        liveObject.SetActive(true);
        destroyedObject.SetActive(false);
    }

    public virtual void TakeDamage(float damageAmount)
    {
        Health -= damageAmount;
        if (Health <=0 && !bDestroyed)
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
            //Destroy(gameObject); //remove this from the scene
            //PROBLEM: Need to add our scoring for this being a target or not to the levelController (or whatever is in charge of everything)
            ((LevelControllerBomberMinigame)LevelControllerBase.Instance).ObjectDestroyed(gameObject, this);
        }
    }
}
