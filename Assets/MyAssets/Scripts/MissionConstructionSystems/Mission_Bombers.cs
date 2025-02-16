using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This is for constructing a mission that involves bombers (as opposed to a skirmish, scouting, or some other sort of mission. Start straightforward...)
public class Mission_Bombers : MissionConstructionBase
{

    public GameObject targetObject;
    public Range bomberHeight = new Range(20, 50);
    public Range bomberHeightVariation = new Range(-5, 10);
    public Range rotationVariance = new Range(-7, 7);

    //Ok, so now I've got to figure out how I'd go about adding in bombers flying in formation
    //Really I'd like to have aircraft in diamond formation (even though this wasn't a thing until WW11
    float formationDistance = 30;   //This should become public for tweaking
    int numBombers = 4;

    public override void DoStart()
    {
        base.DoStart();
        AddTargetBombers(targetObject.transform.position, 100f);
    }

    //This'll need shifted to somewhere better
    public virtual void AddTargetBombers(Vector3 thisTargetLocation, float spawnInRange)
    {
        int addedBombers = 1;   //Start at 1 for our primary bomber
        float CruiseHeight = bomberHeight.GetRandom();
        //So basically we need to spawn some bombers in, and assemble a path for them to fly to the target
        float incomingAngle = Random.Range(0, 360);
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
        actorWrapper newActor = ((LevelController)LevelControllerBase.Instance).addFighterActor(prefabManager.Instance.friendlyBombers[0], 0, spawnLocation, startQuat, groupTag, null);
        //Have to get the controller for this vehicle and set the flight points in it
        PathAircraft bomberController = newActor.vehicle.GetComponent<PathAircraft>();
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
    }

    void addFormationBomber(Vector3 formationPosition, Vector3 baseSpawnLocation, Quaternion startRotation, List<Vector3> pathPoints, Vector3 baseTargetPosition)
    {

        string groupTag = "bomber_Group_" + Time.time.ToString("f0");
        actorWrapper newActor = ((LevelController)LevelControllerBase.Instance).addFighterActor(prefabManager.Instance.friendlyBombers[0], 0, baseSpawnLocation + startRotation * Quaternion.AngleAxis(180, Vector3.up) * formationPosition, startRotation, groupTag, null);
        //Have to get the controller for this vehicle and set the flight points in it
        PathAircraft bomberController = newActor.vehicle.GetComponent<PathAircraft>();
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
            bomberController.targetDropLocation = baseTargetPosition + Quaternion.AngleAxis(angleToNext, Vector3.up) * formationPosition; ;   //This'll need some logic applied to it
            GameObject targetPathPoint = new GameObject("targetPositionPoint");
            targetPathPoint.transform.position = bomberController.targetDropLocation;

            bomberController.pathPositions = offsetPathPoints;
        }
    }
}
