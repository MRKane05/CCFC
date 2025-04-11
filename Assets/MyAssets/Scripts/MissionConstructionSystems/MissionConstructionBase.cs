using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class flightGroup
{

    //public int eventTriggerCount = 0; //If the number of vehicles in this flight goes below zero it'll trigger an event (such as getting new fighters)

    //So for this we're going to have a flight of aircraft
    public List<actorWrapper> spawnedVehicles = new List<actorWrapper>();


    //Built in return functions
    public bool removeActor(GameObject thisActor)
    {

        return true;
    }

	public bool addActor(actorWrapper thisActor)
    {
		spawnedVehicles.Add(thisActor);
		return true;
    }
}

//Because we've made many of the elements of the game into mini-games we can really drill down what our missions will be and simplify things further here
public class MissionConstructionBase : MonoBehaviour {
	bool bWaitingToFinishMision = false;

    private static MissionConstructionBase instance = null;
    public static MissionConstructionBase Instance { get { return instance; } }

    public enum enMissionPlayState { NULL, START, PLAYING, FINISHED }
    public enMissionPlayState MissionPlayState = enMissionPlayState.START;

    float AckAckZoneRange = 40f;    //How much extra range to we add to the ack ack protection radius?

    void Awake()
    {
        if (instance)
        {
            Debug.Log("Duplicate attempt to create MissionConstructor");
            Destroy(this);
            return;
        }

        instance = this;
    }

    //We're going to have to start somewhere!
    IEnumerator Start() {

		yield return null;

		if (!prefabManager.Instance)
		{
			yield return null;
		}
		//Now we should be fine for sending through the start function
		DoStart();
    }

    public virtual void DoStart()
    {
        MissionPlayState = enMissionPlayState.PLAYING;
    }

	public void DelayFinishMission(bool bSuccess, float delayTime)
    {
		if (!bWaitingToFinishMision)
		{
			bWaitingToFinishMision = true;
			StartCoroutine(DoDelayFinishMission(delayTime));
		}
    }

	IEnumerator DoDelayFinishMission(float delayTime)
    {
		yield return new WaitForSeconds(delayTime);
		((LevelController)LevelControllerBase.Instance).finishMatch(false);
	}

    public virtual void ConstructMission()
    {
        //So to kick off we're going to need a place where the mission is happening
        //Depending on the mission type we'll have to set our objectives which will be destroy/protect
        //These objectives mightn't happen all at once, so we'll need some sort of global "wave handler" system for controlling how waves come into the scene
        //We'll also need bonus objectives to complete littered around in a constructive way
        //Really that's about it on the surface
    }

	public void Update()
    {
		DoUpdate();
    }

	public virtual void DoUpdate()
    {

    }

	public virtual void RemoveActor(GameObject thisActor)
    {

	}

    #region Fighter Generation Methods

    protected float totalBombers = 0;
    protected Vector3 averagePosition = Vector3.zero;

    public actorWrapper addFormationBomber(Vector3 formationPosition, int newTeam, Vector3 baseSpawnLocation, Quaternion startRotation, List<Vector3> pathPoints, Vector3 baseTargetPosition)
    {
        //IDEA: We should make these bombers opportunistic
        string groupTag = "bomber_Group_" + Time.time.ToString("f0");
        GameObject targetBomber = newTeam == 0 ? prefabManager.Instance.friendlyBombers[0] : prefabManager.Instance.enemyBombers[0];
        actorWrapper newActor = ((LevelController)LevelControllerBase.Instance).addFighterActor(targetBomber, newTeam, baseSpawnLocation + startRotation * Quaternion.AngleAxis(180, Vector3.up) * formationPosition, startRotation, groupTag, null);
        totalBombers++;
        //activeBombers.Add(newActor);
        //Have to get the controller for this vehicle and set the flight points in it
        PathAircraft bomberController = newActor.vehicle.GetComponent<PathAircraft>();
        bomberController.ourMissionConstructor = this;  //So that we've got a callback link for sending important information through

        //GameObject newPoint = new GameObject("Formation Start");
        float angleToNext = 0;
        //newPoint.transform.position = baseSpawnLocation + startRotation * formationPosition;
        averagePosition += baseSpawnLocation + startRotation * formationPosition;
        if (bomberController)
        {
            List<Vector3> offsetPathPoints = new List<Vector3>();

            //We need to go through our pathPoints and offset them according to heading
            for (int i = 0; i < pathPoints.Count; i++)
            {
                if (i < pathPoints.Count - 2)
                {
                    angleToNext = Mathf.Atan2(pathPoints[i].x - pathPoints[i + 1].x, pathPoints[i].z - pathPoints[i + 1].z) * 180f / Mathf.PI;
                    Vector3 pointWithOffset = pathPoints[i] + Quaternion.AngleAxis(angleToNext, Vector3.up) * formationPosition;
                    //GameObject newPathPoint = new GameObject("formationPath: " + i.ToString());
                    //newPathPoint.transform.position = pointWithOffset;
                    offsetPathPoints.Add(pointWithOffset);
                }
                else
                {   //We want the point at the end looking back
                    angleToNext = Mathf.Atan2(pathPoints[i].x - pathPoints[i - 1].x, pathPoints[i].z - pathPoints[i - 1].z) * 180f / Mathf.PI;
                    Vector3 pointWithOffset = pathPoints[i] + Quaternion.AngleAxis(angleToNext + 180, Vector3.up) * formationPosition;
                    //GameObject newPathPoint = new GameObject("formationPath: " + i.ToString());
                    //newPathPoint.transform.position = pointWithOffset;
                    offsetPathPoints.Add(pointWithOffset);
                }
            }

            bomberController.pathPositions = offsetPathPoints;

            //So our base target position will be Count-2
            angleToNext = Mathf.Atan2(pathPoints[pathPoints.Count - 2].x - pathPoints[pathPoints.Count - 1].x, pathPoints[pathPoints.Count - 2].z - pathPoints[pathPoints.Count - 1].z) * 180f / Mathf.PI;
            bomberController.targetDropLocation = baseTargetPosition + Quaternion.AngleAxis(angleToNext, Vector3.up) * new Vector3(formationPosition.x, 0, 0);   //This'll need some logic applied to it
            //GameObject targetPathPoint = new GameObject("targetPositionPoint");
            //targetPathPoint.transform.position = bomberController.targetDropLocation;

            bomberController.pathPositions = offsetPathPoints;
        }
        return newActor;
    }

    #endregion

    #region Balloon Generation Methods
    public void addBarrageBalloons(Vector3 clusterCenter, float clusterSpacing, int newTeam)
    {
        //I think balloons will be put down as their own objects so we've a bit of control over their location
        List<Vector3> balloonPositions = new List<Vector3>();


        int flightCount = Mathf.RoundToInt(Random.Range(1, 3));
        //int flightCount = 1;	//PROBLEM this is a testing hack
        float lineAngle = Random.Range(0f, 360f);
        Quaternion AngleAxis = Quaternion.AngleAxis(lineAngle, Vector3.up);
        Vector3 lineForward = new Vector3(clusterSpacing, 0, 0);
        for (int i = 0; i < flightCount; i++)
        {
            Vector3 spawnLocation = clusterCenter;
            spawnLocation += AngleAxis * lineForward * i;

            //Make our line wonky
            float clusterNoise = clusterSpacing / 3f;
            spawnLocation += new Vector3(Random.Range(-clusterNoise, clusterNoise), Random.Range(-clusterNoise, clusterNoise), Random.Range(-clusterNoise, clusterNoise));
            balloonPositions.Add(spawnLocation);
            string groupTag = "bomber_Group_" + Time.time.ToString("f0");
            GameObject targetBalloon = newTeam == 0 ? prefabManager.Instance.friendlyBalloons[0] : prefabManager.Instance.enemyBalloons[0];
            actorWrapper newActor = ((LevelController)LevelControllerBase.Instance).addFighterActor(targetBalloon, newTeam, spawnLocation, Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up), groupTag, null);
        }

        //We need to maybe add a zone to the ackack in the scene
        Vector3 centroid = Vector3.zero;
        foreach (Vector3 balloonPos in balloonPositions)
        {
            centroid += balloonPos;
        }
        centroid /= balloonPositions.Count;

        //We need a measure of the size of the balloon field
        float fieldSize = 0f;
        foreach (Vector3 balloonPos in balloonPositions)
        {
            fieldSize = Mathf.Max(fieldSize,
                Vector2.Distance(new Vector2(centroid.x, centroid.z), new Vector2(balloonPos.x, balloonPos.z)));
        }

        //PROBLEM: Need to have something to control Ack Ack zones going down, but for now lets just make it into a random
        if (Random.value > 0.33f)
        {
            ((LevelController)LevelControllerBase.Instance).AckAckZone.Zones.Add(new Vector4(centroid.x, centroid.z, fieldSize, 0f));
        }
    }
    #endregion
}
