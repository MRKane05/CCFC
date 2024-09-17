using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//We need some listed way of making a mission up
[System.Serializable]
public class missionEvent
{
    //The core event itself
    public enum enEventType { NONE, PLANES, BASE, STATIC, VEHICLES, BALLOON, PHOTO } //This will be an event driver that'll make something of it
    public enEventType eventType = enEventType.NONE;
    //What does the player have to do with said thing?
    public enum enPlayerTask { NONE, DESTROY, PROTECT }
    public enPlayerTask playerTask = enPlayerTask.NONE;

    //For handling visible and invisible objectives, or just plonking things into the world
    public enum enObjectiveType { NONE, VISIBLE, HIDDEN }
    public enObjectiveType objectiveType = enObjectiveType.NONE;

    //Somehow this'll work in with things?
    public enum enTriggerType { NONE, DISTANCE, TIME, MARKER, DESTROYED, DAMAGED, START }
    public enTriggerType triggerType = enTriggerType.NONE;

    public enum enGameplayState { NONE, ALIVE, AllDESTROYED }
    public enGameplayState gameplayState = enGameplayState.ALIVE;

    //I'm not sure if this should go here, or on the object itself.
    public List<GameObject> spawnObject = new List<GameObject>();
    public List<Vector3> spawnLocations = new List<Vector3>();

    public int repeatCount = 0; //Suppose we want something to repeat after it's triggered. I'm not totally sure how this'd work
    public float triggerValue = 0; //This might be used for trigger time, or any other float related read value (such as damage amount or distance)
    public float postTriggerDelay = 10f;    //How long after our trigger does the event fire off?
    public int eventTeam = -1;
    public int requisitionScore = 0; //if the task is completed we get these points. If it's failed we'd lose these points if it's an enObjectiveType.VISIBLE

    public GameObject eventMarker;  //What are we linked to?
    public GameObject eventItem;	//What are we going to spawn for this event? I'd assume for the likes of planes that this could be a squad object

    public int watchedMisionEvent; //So if we're linked to another event for some reason (say it gets destroyed) this is what we're to look at
    public MissionEventObject watchedEvent;
}

//An object wrapper (might get moved somewhere else) for something we can spawn into our world as a mission event
//Might include set dressing, I dunno
public class MissionEventObject : MonoBehaviour {
    public missionEvent ourMissionEvent;
    public MissionEventsManager ourMissionEventManager;
    //public List<GameObject> spawnObject = new List<GameObject>();	//What we'll be spawning in, it could be a single object, a flight of planes, or similar
    public List<actorWrapper> groupActors = new List<actorWrapper>();   //Can't remember why this is here...

    public bool bCanTrigger = true; //Set to false if we've already been triggered. This might need to become more complex.
    public bool bAllDestroyed = false;
    //Called when we're spawning all of these in, this is seperate from onTrigger as we might need to setup first
    public virtual void doSpawn()
    {

    }

    //Trigger our event (if it needs triggered)
    public virtual void doTrigger()
    {
        bCanTrigger = false;
    }

    //Called when all of the spawnedObjects have been destroyed
    public virtual void onSetDestroyed()
    {

    }

    void Update()
    {
        //PromptedUpdate();
    }

    public void doTick(float gameTime)
    {
        checkTrigger(gameTime);

    }

    public virtual void actorTakingDamage(MissionEventObject thisEvent, Actor thisActor, float healthRatio)
    {
        //I'm not sure what we can do with this now? Send a trigger through the system? I can see it being useful, but likewise I'm not sure how it'll
        //handle in a group
        //specialTrigger(thisEvent, healthRatio);
        //Pass this event up to our master controller
        ourMissionEventManager.actorTakingDamage(thisEvent, thisActor, healthRatio);    //This is problematic as it'll cause an iterate on every bullet hit, everywhere...
    }

    public virtual void actorCallbackDie(Actor thisActor)
    {
        //Called when the actor dies, not sure what the best implementation here is
        foreach(actorWrapper thisWrapper in groupActors)
        {
            if (thisWrapper.actor == thisActor)
            {
                Debug.LogError("Actor marked as dead");
                thisWrapper.bActorDead = true;
            }
        }

        bool someAlive = false;
        //So at this stage we should check and see if we've got any actors left alive
        foreach (actorWrapper thisWrapper in groupActors)
        {
            if (!thisWrapper.bActorDead)
            {
                someAlive = true;
            }
        }

        if (!someAlive) { //we need to report back to our missions event manager that this group is all dead
            bAllDestroyed = true;
            Debug.LogError("AllDestroyed");
            //We should look to see if this is our "complete" state
            if (ourMissionEvent.playerTask == missionEvent.enPlayerTask.DESTROY) {
                ourMissionEventManager.EventComplete(this);
            }


            ourMissionEventManager.EventAllDestroyed(this);
        }
    }

    public virtual void specialTrigger(MissionEventObject triggerMEV, float triggerValue)
    {
        if (!bCanTrigger) { return; }

        switch (ourMissionEvent.triggerType)
        {
            case missionEvent.enTriggerType.DAMAGED:
                if (ourMissionEvent.watchedEvent == triggerMEV) {  //this is us
                    if (triggerValue < ourMissionEvent.triggerValue)    //Our target has been damaged too much. Trigger a response
                    {
                        doTrigger();
                    }
                }
                break;
            default:
                break;
        }
    }

    public virtual void checkTrigger(float gameTime)
    {
        if (!bCanTrigger) { return;  }
        
            //So do we look to see if an event should be triggered here, or is it handled in the levelController?
            //Ok, in theory six of one and a half-dozen of another I suppose
            //Timed events can happen here
            //Triggered events will be called from another MissionEventObject
            //Distance events will have to be checked here
            //Destroyed must be a triggered event on something else, or do we keep a check on the state of things?
        switch (ourMissionEvent.triggerType)
        {
            case missionEvent.enTriggerType.START:
                doTrigger();
                break;
            case missionEvent.enTriggerType.DISTANCE:
                //dosomething
                break;
            case missionEvent.enTriggerType.TIME:
                if (gameTime > ourMissionEvent.triggerValue + ourMissionEvent.postTriggerDelay)
                {
                    doTrigger();
                }
                break;
            case missionEvent.enTriggerType.MARKER:
                //doSomething
                break;
            case missionEvent.enTriggerType.DESTROYED:
                //If our target object/group is destroyed we spawn
                if (ourMissionEvent.watchedEvent.bAllDestroyed)
                {
                    doTrigger();
                }
                //doSomething
                break;
            default:
                //doSomething
                break;
        }
    }
}
