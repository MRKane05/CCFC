using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Used in the fighter game, as opposed to the bomber game
public class WorldDestructableObject : DestructableObject {

    public override void TakeDamage(float damageAmount)
    {
        Health -= damageAmount;
        //Debug.Log("Health: " + Health + " Damage: " + damageAmount);
        if (Health <= 0 && !bDestroyed)
        {
            DoDestroy();
            //Destroy(gameObject); //remove this from the scene
            //PROBLEM: Need to add our scoring for this being a target or not to the levelController (or whatever is in charge of everything)
            //((LevelController)LevelControllerBase.Instance).ObjectDestroyed(gameObject, this);

        }
    }
}
