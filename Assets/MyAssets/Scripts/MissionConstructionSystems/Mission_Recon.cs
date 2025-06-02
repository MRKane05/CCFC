using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Basically the player has to go through points to engage with fighters and collect intel
//We'll finally have use for the waypoints!!
public class Mission_Recon : MissionConstructionBase {

    float levelStartTime = 0;

    public GameObject ReconPointPrefab;
    public Range ReconHeightRange = new Range(20, 70);

    public List<GameObject> ReconPoints;

    public override void DoStart()
    {
        base.DoStart();
        //GenerateMission(1f);
    }

    public void GenerateMission(float difficulty)
    {
        //In this we'd set the target number of enemy we'd be fighting against, and also the amount of support we're expected to have
        levelStartTime = Time.time;
        int targetRconPoints = Random.Range(3, 7);

        //List<Vector3> ReconOffsets = AddReconPoints(targetRconPoints);
        /*
        List<Vector3> spawnPoints = PointDistributor.PointDistributor.DistributePointsAroundPlayer(
            playerPosition,
            numPoints: 10,
            maxRadius: 20f,
            minRadius: 3f,
            minDistanceBetweenPoints: 2f);
        */
        //For the moment lets just generate in a crooked line...
        Vector3 newPosition = PlayerController.Instance.transform.position;
        float flatAngle = Random.Range(0, 360);
        Range pointDistance = new Range(20, 50);
        List<Vector3> linearReconPoints = new List<Vector3>();
        for (int i = 0; i < targetRconPoints; i++)
        {
            newPosition += Quaternion.AngleAxis(flatAngle, Vector3.up) * Vector3.forward * pointDistance.GetRandom();
            linearReconPoints.Add(newPosition);
            flatAngle += Random.Range(-45, 45);
        }

        GenerateReconPoints(linearReconPoints);
       
    }

    void GenerateReconPoints(List<Vector3> Offsets)
    {
        for (int i=0; i<Offsets.Count; i++)
        {
            Vector3 FinalPosition = LevelControllerBase.Instance.getTerrainHeightAtPoint(PlayerController.Instance.transform.position + Offsets[i]);
            FinalPosition += Vector3.up * ReconHeightRange.GetRandom();

            GameObject newReconPoint = Instantiate(ReconPointPrefab, FinalPosition, Quaternion.identity) as GameObject;
            reconPointActor newReconActor = newReconPoint.GetComponent<reconPointActor>();

            //A few details to control the setup
            int numPhotos = Random.Range(2, 5);
            int numFighters = 0;
            if (Random.value > 0.125f)
            {
                numFighters = Random.Range(1, 5);
            }
            int numBalloons = 0;
            if (Random.value > 0.125f)
            {
                numBalloons = Random.Range(1, 3);
            }

            newReconActor.DoSetup(FinalPosition, this, numBalloons, numFighters, numPhotos, true); //For the moment we'll roll with this and lets the points set themselves up
            ReconPoints.Add(newReconPoint);
            ((LevelController)LevelControllerBase.Instance).AddReconPoint(0, newReconPoint);
        }
    }


    float clusterRange = 200f;  //I've no idea what this should be.
    float minRange = 25;
    
    public List<Vector3> AddReconPoints(int numRecon)
    {
        //So there's a couple of ways we could put a point in:
        //Cluster: where the player will have to go around all of the points and there won't be all that reason behind them
        //Line: something that can be flown through from the beginning to the end with minimal issues, and maybe the odd outlier to make things more interesting

        //Lets start with a cluster :)
        int NumPoints = 0;
        List<Vector3> newPoints = new List<Vector3>();
        int numCycles = 0;

        //Start by creating a collection of points and then apply them around the player
        while (newPoints.Count < numRecon && numCycles < 100)
        {
            numCycles++;
            Vector3 newPosition = new Vector3(Random.Range(minRange, clusterRange) * Random.value > 0.5f? 1f:-1f, 0, Random.Range(minRange, clusterRange) * Random.value > 0.5f ? 1f : -1f);
            //Make sure our points aren't too close together
            bool bCanAdd = true;
            foreach (Vector3 thisPosition in newPoints)
            {
                if (Vector3.Distance(thisPosition, newPosition) < minRange)
                {
                    bCanAdd = false;
                }
            }
            if (bCanAdd)
            {
                newPoints.Add(newPosition);
            }
        }
        return newPoints;
    }
}
