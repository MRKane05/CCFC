using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class flightGroup
{
    //public int eventTriggerCount = 0; //If the number of vehicles in this flight goes below zero it'll trigger an event (such as getting new fighters)

    //So for this we're going to have a flight of aircraft
    public List<GameObject> spawnedVehicles = new List<GameObject>();


    //Built in return functions
    public bool removeVehicle(GameObject thisVehicle)
    {
        if (spawnedVehicles.Contains(thisVehicle))
        {
            spawnedVehicles.Remove(thisVehicle);
            return true;
        } else
        {
            return false;   //We weren't able to remove this vehicle
        }

        return true;
    }
}

//Because we've made many of the elements of the game into mini-games we can really drill down what our missions will be and simplify things further here
public class MissionConstructionBase : MonoBehaviour {


    //We're going to have to start somewhere!
    IEnumerator Start()
    {
        yield return null;
        //Give us a beat
        DoStart();
    }

    public virtual void DoStart()
    {

    }

    public virtual void ConstructMission()
    {
        //So to kick off we're going to need a place where the mission is happening
        //Depending on the mission type we'll have to set our objectives which will be destroy/protect
        //These objectives mightn't happen all at once, so we'll need some sort of global "wave handler" system for controlling how waves come into the scene
        //We'll also need bonus objectives to complete littered around in a constructive way
        //Really that's about it on the surface
    }

    flightGroup MakeFlight_Fighters(int team, int number, )
    {
        flightGroup newFlight = new flightGroup();

        return newFlight;
    }
}
