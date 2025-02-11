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
    public override void DoStart()
    {
        base.DoStart();

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
            }
        }
        if (bIsDead)
        {
            AircraftDeathSpiral();
            //We'll need to keep a ticker on when to do an explosion effect
        }
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

	public override void takeDamage(float thisDamage, string damageType, GameObject instigator, int damagingTeam, float delay)
	{
        base.takeDamage(thisDamage, damageType, instigator, damagingTeam, delay);
        //PROBLEM: We need to update the players UI just for this particular type of aircraft (this is going to get screwy when we add more than one, but for the moment it's getting things on the ground)
        float damageProp =health / maxHealth;
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
}
