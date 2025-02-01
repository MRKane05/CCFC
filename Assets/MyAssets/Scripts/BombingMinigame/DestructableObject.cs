using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This should maybe be in a different place as it could apply to all parts of the game, but the bomber minigame is where it was made first
//Should this be an actor?
public class DestructableObject : MonoBehaviour {

    public float Health = 500f; //I've no idea what this'll be

    public void TakeDamage(float damageAmount)
    {
        Health -= damageAmount;
        if (Health <=0)
        {
            Destroy(gameObject); //remove this from the scene
            //PROBLEM: Need to add our scoring for this being a target or not to the levelController (or whatever is in charge of everything)
            ((LevelControllerBomberMinigame)LevelControllerBase.Instance).ObjectDestroyed(gameObject, this);
        }
    }
}
