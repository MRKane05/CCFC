using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Because our colliders won't always be on the thing that has our vehicle...seemed like a good idea at the time
public class ActorDamageRedirect : MonoBehaviour
{
    public Actor redirectActor;

    public virtual void takeDamage(float thisDamage, string damageType, GameObject instigator, int damagingTeam, float delay)
    {
        if (redirectActor)
        {
            Debug.Log("Redirect taking damage");
            redirectActor.takeDamage(thisDamage, damageType, instigator, damagingTeam, delay);
        }
    }
}
