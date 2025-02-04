using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The idea is that this is a destructable object that moves and stops when the object in front of it becomes stopped
public class ChainMovingDestructable : DestructableObject
{
    public float ForwardSpeed = 20f;    //How fast will we move forward?
    public float SlowDownSpeed = 1f;    //How quickly will our vehicle stop
    public bool bIsStopped = false;

    public ChainMovingDestructable VehicleInfront;  //We could raytrace this but I don't think it's necessary
    //I want these vehicles to move to the side (look derailed for the trains perhaps) but for the moment lets just roll with this


    void Update()
    {
        if (!bDestroyed)
        {
            if (!VehicleInfront)
            {
                DoMoveForward();
            }
            else
            {
                if (!VehicleInfront.bDestroyed && !VehicleInfront.bIsStopped)
                {
                    DoMoveForward();
                } else
                {
                    ForwardSpeed = Mathf.MoveTowards(ForwardSpeed, 0, Time.deltaTime * SlowDownSpeed);
                    if (ForwardSpeed <= 1)
                    {
                        bIsStopped = true;
                    }
                    DoMoveForward();
                }
            }
        }
    }

    void DoMoveForward()
    {
        gameObject.transform.position += gameObject.transform.forward * ForwardSpeed * Time.deltaTime;
    }
}
