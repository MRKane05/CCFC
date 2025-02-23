using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PathAircraft;

//This is for constructing a mission that involves bombers (as opposed to a skirmish, scouting, or some other sort of mission. Start straightforward...)
public class Mission_Bombers : MissionConstructionBase
{
    public enum enBombingTeam { NULL, PLAYER, ENEMY }
    public enBombingTeam BombingTeam = enBombingTeam.PLAYER;

    public GameObject targetObject;
    public Range bomberHeight = new Range(20, 50);
    public Range bomberHeightVariation = new Range(-5, 10);
    public Range rotationVariance = new Range(-7, 7);

    public List<actorWrapper> activeBombers = new List<actorWrapper>();

    //Ok, so now I've got to figure out how I'd go about adding in bombers flying in formation
    //Really I'd like to have aircraft in diamond formation (even though this wasn't a thing until WW11
    float formationDistance = 20;   //This should become public for tweaking

    float generalIncomingAngle = 0;

    BaseGenerator ourBaseGenerator;

    public int totalBombers = 0;    //Will be set at start of mission

    public int missionClearedBombers = 0;

    public override void DoStart()
    {
        base.DoStart();
        GenerateMission(enBombingTeam.PLAYER);  //Our kick to setup

        //We also need to figure out where we're placing our player
        ((LevelController)LevelControllerBase.Instance).playerAircraft.transform.position = ((LevelController)LevelControllerBase.Instance).getTerrainHeightAtPoint(((LevelController)LevelControllerBase.Instance).playerAircraft.transform.position) + Vector3.up * Random.Range(30, 70);
    }

    public void GenerateMission(enBombingTeam targetBombingTeam)
    {
        BombingTeam = targetBombingTeam;

        generalIncomingAngle = Random.Range(0, 360);

        StartCoroutine(DoMissionSetup());
        
    }

    public IEnumerator DoMissionSetup()
    {
        //We should kick by having the LevelController load a terrain to have this happen in
        //Setup terrain with any extras we want
        //Pick a starting location
        if (!ourBaseGenerator)
        {
            ourBaseGenerator = gameObject.GetComponent<BaseGenerator>();
        }

        //This will call our base building (and then get a callback from that to say we're done)
        yield return StartCoroutine(ourBaseGenerator.CreateBase());
        //Now we need to sort out our starting bombers after pulling some information from the base builder


        //Then we position our player
        //Then we give everything the all-clear to proceed


        //IDEA Add in an optional requirement to clear the airspace before bombers will approach
        //So we need to know if we should be protecting the bombers or stopping them. Both will have diffferent implications
        totalBombers = 1;// Random.Range(3, 11); //Of course this'll require a setting for difficulty to be passed through
        missionClearedBombers = 0;
        Debug.Log("Total Bombers: " + totalBombers);
        int numBombers = 0;
        int numGroups = 1;
        float currentSpawnIn = Random.Range(30f, 45f);
        //I really don't like while loops...
        for (int i=0; i<numGroups; i++)
        {
            currentSpawnIn += 3 * i * Random.Range(15f, 35f);   //Really the spawn in range is speed by time, and it'll also vary depending on this being an escort or defense
            int bombersInGroup = 1; // Mathf.Clamp(Random.Range(1, 4), 1, totalBombers-1);
            numBombers += bombersInGroup;
            AddTargetBombers(ourBaseGenerator.getBomberTarget(), currentSpawnIn, bombersInGroup);
        }

        //Ok new idea. We need to add bombers in a way that they'll be flying over in waves. The mission is complete if all bombers are downed, or when all payloads are cleared

        //And the base itself could do with a flight of aircraft to cover it (we need to get guns down for this operation)
        
        ((LevelController)LevelControllerBase.Instance).AddFighterFlight(LevelController.Instance.getTerrainHeightAtPoint(ourBaseGenerator.baseParent.transform.position) + Vector3.up * Random.Range(20f, 50f), 20f, Random.Range(2, 4), BombingTeam == enBombingTeam.PLAYER ? 1 : 0); //These are for base defense

        //Ok, we could do with sending the player a message to direct them here
        if (BombingTeam == enBombingTeam.PLAYER)
        {
            //NGUI_Base.Instance.setGameMessage("Protect our bombers!");
            NGUI_Base.Instance.setPortraitMessage("Commander", "Protect our bombers!", Color.black);
        } else
        {
            //NGUI_Base.Instance.setGameMessage("Enemy bombers incoming! Stop them!");
            NGUI_Base.Instance.setPortraitMessage("Commander", "Incoming enemy bombers! Stop them!", Color.black);
        }
    }

    //This'll need shifted to somewhere better
    public virtual void AddTargetBombers(Vector3 thisTargetLocation, float spawnInRange, int numBombers)
    {
        //int addedBombers = 4;   //Start at 1 for our primary bomber
        float CruiseHeight = bomberHeight.GetRandom();
        //So basically we need to spawn some bombers in, and assemble a path for them to fly to the target
        float incomingAngle = generalIncomingAngle + Random.Range(-25, 25); //Add some variance for our incoming angle but don't do so in a way that'll have aircraft cross over each other
        Vector3 spawnLocation = thisTargetLocation + Quaternion.AngleAxis(incomingAngle, Vector3.up) * Vector3.forward * spawnInRange;
        
        spawnLocation = LevelController.Instance.getTerrainHeightAtPoint(spawnLocation) + Vector3.up * (CruiseHeight + bomberHeightVariation.GetRandom());
        //So...our points
        int numIncomingPoints = Random.Range(1, 3); //This will control how much our flightpath will vary up to the target point
        List<Vector3> flightPoints = new List<Vector3>();
        Debug.Log("incomingPoints: " + numIncomingPoints);
        GameObject newGameObject = new GameObject("Start");
        newGameObject.transform.position = spawnLocation;
        for (int i=0; i<numIncomingPoints; i++)
        {
            //So basically we've got a range that we've got to divide into parts here
            float distFraction = (float)(numIncomingPoints - i) / (float)(numIncomingPoints + 1);
            Debug.Log("Dist Fraction: " + distFraction);
            float newDistPoint = spawnInRange * distFraction;
            incomingAngle += rotationVariance.GetRandom();
            Vector3 nextPoint = thisTargetLocation + Quaternion.AngleAxis(incomingAngle, Vector3.up) * Vector3.forward * spawnInRange * distFraction;
            nextPoint = LevelController.Instance.getTerrainHeightAtPoint(nextPoint) + Vector3.up * (CruiseHeight + bomberHeightVariation.GetRandom());
            newGameObject = new GameObject("i:" + i.ToString());
            newGameObject.transform.position = nextPoint;
            flightPoints.Add(nextPoint);
            //Pick rotations for our incoming paths
        }
        //And one for over the target
        Vector3 overtargetPoint = LevelController.Instance.getTerrainHeightAtPoint(thisTargetLocation) + Vector3.up * (CruiseHeight + bomberHeightVariation.GetRandom());
        flightPoints.Add(overtargetPoint);
        newGameObject = new GameObject("OverTarget");
        newGameObject.transform.position = overtargetPoint;

        //And one for flying past the target point
        Vector3 pastTargetPoint = LevelController.Instance.getTerrainHeightAtPoint(thisTargetLocation + Quaternion.AngleAxis(incomingAngle + 180f, Vector3.up) * Vector3.forward * spawnInRange) + Vector3.up * (CruiseHeight + bomberHeightVariation.GetRandom());
        flightPoints.Add(pastTargetPoint);
        newGameObject = new GameObject("PastTarget");
        newGameObject.transform.position = pastTargetPoint;

        string groupTag = "bomber_Group_" + Time.time.ToString("f0");
        Quaternion startQuat = Quaternion.LookRotation(flightPoints[0] - spawnLocation, Vector3.up);
        //Ok! We need to spawn in our bomber!
        actorWrapper newActor = ((LevelController)LevelControllerBase.Instance).addFighterActor(BombingTeam == enBombingTeam.PLAYER ? prefabManager.Instance.friendlyBombers[0] : prefabManager.Instance.enemyBombers[0], 0, spawnLocation, startQuat, groupTag, null);
        activeBombers.Add(newActor);
        //Have to get the controller for this vehicle and set the flight points in it
        PathAircraft bomberController = newActor.vehicle.GetComponent<PathAircraft>();
        bomberController.ourMissionConstructor = this;

        if (bomberController)
        {
            bomberController.pathPositions = flightPoints;
            bomberController.targetDropLocation = thisTargetLocation;   //This'll need to be changed for the formation behaviour too
        } else
        {
            Debug.LogError("No path Aircraft on bomber");
        }


        //Ok, cool, our primary bomber is placed as expected. We've got to add our formation bombers which will go
        //      P
        //   1     2
        //      3

        //Really not sure what the best way to add bombers in formation is here
        if (numBombers > 1) //We have 1st position
        {
            addFormationBomber(new Vector3(formationDistance, 0, formationDistance), spawnLocation, startQuat, flightPoints, thisTargetLocation);
        } 
        if (numBombers > 2)
        {
            addFormationBomber(new Vector3(-formationDistance, 0, formationDistance), spawnLocation, startQuat, flightPoints, thisTargetLocation);
        }
        if (numBombers > 3)
        {
            addFormationBomber(new Vector3(0, 0, formationDistance*2f), spawnLocation, startQuat, flightPoints, thisTargetLocation);
        }
    }

    void addFormationBomber(Vector3 formationPosition, Vector3 baseSpawnLocation, Quaternion startRotation, List<Vector3> pathPoints, Vector3 baseTargetPosition)
    {

        string groupTag = "bomber_Group_" + Time.time.ToString("f0");
        actorWrapper newActor = ((LevelController)LevelControllerBase.Instance).addFighterActor(BombingTeam == enBombingTeam.PLAYER ? prefabManager.Instance.friendlyBombers[0] : prefabManager.Instance.enemyBombers[0], 0, baseSpawnLocation + startRotation * Quaternion.AngleAxis(180, Vector3.up) * formationPosition, startRotation, groupTag, null);
        activeBombers.Add(newActor);
        //Have to get the controller for this vehicle and set the flight points in it
        PathAircraft bomberController = newActor.vehicle.GetComponent<PathAircraft>();
        bomberController.ourMissionConstructor = this;  //So that we've got a callback link for sending important information through

        GameObject newPoint = new GameObject("Formation Start");
        float angleToNext = 0;
        newPoint.transform.position = baseSpawnLocation + startRotation * formationPosition;
        if (bomberController)
        {
            List<Vector3> offsetPathPoints = new List<Vector3>();
            
            //We need to go through our pathPoints and offset them according to heading
            for (int i=0; i< pathPoints.Count; i++)
            {
                if (i < pathPoints.Count - 2) {
                    angleToNext = Mathf.Atan2(pathPoints[i].x - pathPoints[i + 1].x, pathPoints[i].z - pathPoints[i + 1].z) * 180f / Mathf.PI;
                    Vector3 pointWithOffset = pathPoints[i] + Quaternion.AngleAxis(angleToNext, Vector3.up) * formationPosition;
                    GameObject newPathPoint = new GameObject("formationPath: " + i.ToString());
                    newPathPoint.transform.position = pointWithOffset;
                    offsetPathPoints.Add(pointWithOffset);
                } else
                {   //We want the point at the end looking back
                    angleToNext = Mathf.Atan2(pathPoints[i].x - pathPoints[i - 1].x, pathPoints[i].z - pathPoints[i - 1].z) * 180f / Mathf.PI;
                    Vector3 pointWithOffset = pathPoints[i] + Quaternion.AngleAxis(angleToNext + 180, Vector3.up) * formationPosition;
                    GameObject newPathPoint = new GameObject("formationPath: " + i.ToString());
                    newPathPoint.transform.position = pointWithOffset;
                    offsetPathPoints.Add(pointWithOffset);
                }
            }

            bomberController.pathPositions = offsetPathPoints;

            //So our base target position will be Count-2
            angleToNext = Mathf.Atan2(pathPoints[pathPoints.Count-2].x - pathPoints[pathPoints.Count-1].x, pathPoints[pathPoints.Count-2].z - pathPoints[pathPoints.Count - 1].z) * 180f / Mathf.PI;
            bomberController.targetDropLocation = baseTargetPosition + Quaternion.AngleAxis(angleToNext, Vector3.up) * new Vector3(formationPosition.x, 0, 0);   //This'll need some logic applied to it
            GameObject targetPathPoint = new GameObject("targetPositionPoint");
            targetPathPoint.transform.position = bomberController.targetDropLocation;

            bomberController.pathPositions = offsetPathPoints;
        }
    }


    public override void RemoveActor(GameObject thisActor)
    {
        for (int i = 0; i < activeBombers.Count; i++)
        { //Need another method here
            if (activeBombers[i].vehicle == thisActor)
            { 
                //remove this entry
                activeBombers.RemoveAt(i); //that way we'll get the correct one
                //We need to know if this vehicle is being removed for cleanup, or because it's been shot down...
                PathAircraft thisBomber = thisActor.GetComponent<PathAircraft>();
                if (thisBomber)
                {
                    //Check to see if it's alive or dead as for evaluating our mission

                }
            }
        }
    }

    public override void DoUpdate()
    {
        base.DoUpdate();
        
        //So something we need is to be able to add groups of bombers so that there are ongoing waves. Of course this'll also mean clearing out the other bombers we've got if they're not destroyed

    }

    public void BomberReturnState(enMissionState newState)
    {
        switch (newState)
        {
            case enMissionState.COMPLETE:
                //Increase the complete counter
                break;
            case enMissionState.FAILED:
                //Increase the failed counter
                break;
        }
        missionClearedBombers++;
        CheckMissionComplete();

    }

    public void CheckMissionComplete()
    {
        if (missionClearedBombers >= totalBombers)
        {
            //This would be mission complete if we're attacking an emey base, so the trick here would be to add a countdown to say that we're done
            if (BombingTeam == enBombingTeam.PLAYER)
            {
                NGUI_Base.Instance.setPortraitMessage("Commander", "Bombing run complete!", Color.black);
                DelayFinishMission(true, 5f);
            }
        }
    }
}
