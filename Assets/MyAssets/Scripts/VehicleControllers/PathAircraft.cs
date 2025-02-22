using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This is an actor (like a bomber) that follows a set path (in this case: forward)
public class PathAircraft : Actor {
    public float crusingSpeed = 4;
    //We need a path of locaitons that we're moving through to get to our target, and we're going to have to setup our behaviour to roll/pitch etc.
    public List<Vector3> pathPositions = new List<Vector3>();
    public List<Transform> pathTransforms = new List<Transform>();
    public int currentPathPosition = 0;
    Quaternion transTargRotation;
    public float maxAngleFactor = 0.3f;
    public float maxAngleRoll = 15f;

    //Ok, for the moment lets assume that path vehicles will only be bombers (although I do see them being used for static gunner sections
    public GameObject bombPrefab;
    public Vector3 targetDropLocation;  //This'll be set as part of the information for arranging the bomber path and needs to be different for the vehicles flying in formation and doing carpet bombing
    public Range startDropRange = new Range(10, 20);    //Add a bit of variance as to when the AI will start dropping bombs
    float startDropRadius = 15f;

    float nextDropTime = 0;
    public float dropFrequency = 1f;
    public int dropNumber = 5;

    public MissionConstructionBase ourMissionConstructor;
    public enum enBombingState { NULL, TOTARGET, BOMBING, FINISHED }
    public enBombingState bombingState = enBombingState.TOTARGET;

    public enum enMissionState { NULL, NOTCOMPLETE, COMPLETE, FAILED }
    public enMissionState MissionState = enMissionState.NOTCOMPLETE;

    public override void DoStart()
    {
        base.DoStart();

        startDropRadius = startDropRange.GetRandom();

        targetRotation = gameObject.transform.rotation;
        transTargRotation = gameObject.transform.rotation;
        //Populate our positions list for testing
        if (pathTransforms.Count > 0)
        {
            foreach(Transform thisTransform in pathTransforms)
            {
                pathPositions.Add(thisTransform.position);
            }
        }
    }

    public void setMissionState(enMissionState newState)
    {
        
        if (newState == enMissionState.NOTCOMPLETE || newState == enMissionState.NULL)
        {
            MissionState = newState;
            //Inform our mission controller of something having happened with us
        }
        ((Mission_Bombers)ourMissionConstructor).BomberReturnState(newState);
        

    }

    void Update()
    {
        speed = crusingSpeed;
        transform.position += transform.forward * Time.deltaTime * speed;   //Simply move this vehicle forward
        if (pathPositions.Count > 0 && currentPathPosition < pathPositions.Count && !bIsDead)
        {
            TurnToFace(pathPositions[currentPathPosition]);
            //Need to check and see if we should move onto our next point
            if (Vector3.SqrMagnitude(gameObject.transform.position - pathPositions[currentPathPosition]) < 1f)
            {
                currentPathPosition++;
                if (currentPathPosition == pathPositions.Count) //We're at our last path position. Remove this actor.
                {
                    SoftRemoveActor();  //TODO: We'll want to fade this out or something good-looking like that. But for the moment lets just blink vanish
                }
            }
        }

        if (bIsDead)
        {
            AircraftDeathSpiral();
            //We'll need to keep a ticker on when to do an explosion effect
            if (Time.time > dieTime)
            {
                doExplode(0);
            }
        } else
        {
            CheckBombingBehavior();
        }
    }

    void CheckBombingBehavior()
    {
        //Step 1: check to see if we're close enough to trigger
        //Step 2: start bombing
        //Step 3: finish bombing
        switch (bombingState)
        {
            case enBombingState.TOTARGET:
                //Quick 2D calculation

                //IDEA: Have the bomber do a sphere check on the ground ahead to see if there's a target there, and if there is drop a volley of 3 bombs
                float flatDistance = Vector2.SqrMagnitude(new Vector2(gameObject.transform.position.x, gameObject.transform.position.z) - new Vector2(targetDropLocation.x, targetDropLocation.z));
                if (flatDistance < startDropRadius * startDropRadius)
                {
                    bombingState = enBombingState.BOMBING;  //Switch into our bombing state
                }
                break;
            case enBombingState.BOMBING:
                if (Time.time > nextDropTime)
                {
                    DropBomb();
                    nextDropTime = Time.time + dropFrequency;
                    dropNumber--;
                }
                if (dropNumber <=0)
                {
                    bombingState = enBombingState.FINISHED;
                    setMissionState(enMissionState.COMPLETE);   //We've been successful with our bombing run
                }
                break;
            case enBombingState.FINISHED:
                break;
        }
    }

    void DropBomb()
    {
        GameObject newBomb = Instantiate(bombPrefab, gameObject.transform.position, gameObject.transform.rotation);
        Bomber_Bomb newBombScript = newBomb.GetComponent<Bomber_Bomb>();
        newBombScript.SetupProjectile(transform.forward * crusingSpeed, 1, -1);
    }

    public static float AngleDir(Vector3 fwd, Vector3 targetDir)
    {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        float dir = Vector3.Dot(perp, Vector3.up);

        if (dir > 0.0)
        {
            return Mathf.Clamp(dir, 0f, 1f);
        }
        else if (dir < 0.0)
        {
            return Mathf.Clamp(dir, -1f, 0);
        }
        else
        {
            return 0.0f;
        }
    }

    float turnDot = 0;
    public virtual void TurnToFace(Vector3 thisPosition)
    {
        //This feels pretty nice to have as a turning approach. We still need to get roll in somehow
        Vector3 toVector = (thisPosition - gameObject.transform.position).normalized;
        //float remainingAngle = Vector3.Angle(transform.forward, toVector);
        turnDot = Mathf.Lerp(turnDot, (AngleDir(transform.forward, toVector)), Time.deltaTime * 1f);    //Smooth our turnDot out so it's not so abrupt when rounding a point
        Quaternion lookRotation = Quaternion.LookRotation(toVector, Vector3.up); //Figure out where we should be going
        transTargRotation = Quaternion.RotateTowards(transTargRotation, lookRotation, yawspeed);
        targetRotation = Quaternion.Slerp(targetRotation, transTargRotation, Time.deltaTime * 1f);

        //Apply our aircraft roll to our targetRotation and set our actual transform rotation
        transform.rotation = targetRotation * Quaternion.AngleAxis(maxAngleRoll * Mathf.Clamp(turnDot, -maxAngleFactor, maxAngleFactor) / maxAngleFactor, transform.forward);
    }

    float dieTime = 0;

	public override void takeDamage(float thisDamage, string damageType, GameObject instigator, int damagingTeam, float delay)
	{
        base.takeDamage(thisDamage, damageType, instigator, damagingTeam, delay);
        //PROBLEM: We need to update the players UI just for this particular type of aircraft (this is going to get screwy when we add more than one, but for the moment it's getting things on the ground)
        float damageProp =health / maxHealth;
        if (health <= 0 && !bIsDead)
        {
            bIsDead = true;
            dieTime = Time.time + 5f;   //How long until we expode and get removed?
            setMissionState(enMissionState.FAILED); //We've been unsuccessful with our bombing run as we've been shot down before it was complete
        }

        if (bIsPlayerVehicle)
        {
            //update our health bar
            NGUI_Base.Instance.assignHealth(damageProp);
            if (health <= 0)
            {
                bIsDead = true;
                ((LevelController)LevelControllerBase.Instance).finishMatch(true);  //Handle our die state
            }
        }
    }

    //So the idea behind this is that it'll move between a path of points that are assigned to it as an array
    protected virtual void AircraftDeathSpiral()
    {
        if (transform.localRotation.eulerAngles.z < 90F || transform.localRotation.eulerAngles.z > 270)
        {
            transform.RotateAround(transform.right, rollspeed * Time.deltaTime * 2f);
            transform.RotateAround(transform.forward, rollspeed * Time.deltaTime);
        }
        else
        {
            transform.RotateAround(transform.right, -rollspeed * Time.deltaTime * 2f);
            transform.RotateAround(transform.forward, -rollspeed * Time.deltaTime);
        }
    }
}
