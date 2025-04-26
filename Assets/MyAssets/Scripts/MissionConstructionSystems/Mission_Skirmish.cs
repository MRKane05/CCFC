using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Mission_Bombers;

public class Mission_Skirmish : MissionConstructionBase {
    public int targetEnemy = 3; //How many enemy will we have to drop for this mission?
    public int enemyRemaining = 3; //How many enemy are left to spawn?
    int nextMinSpawnCount = 2;	//At what number of planes remaining will we spawn more?
    float levelStartTime = 0;
    public Range fighterSpawnHeight = new Range(30, 50);

    float bomberSpawnChance = 1f; //0.125f;

    public bool bMissionConcluding = false;
    public override void DoStart()
    {
        base.DoStart();
        //GenerateMission(1f);
    }

    public void GenerateMission(float difficulty)
    {
        //In this we'd set the target number of enemy we'd be fighting against, and also the amount of support we're expected to have
        levelStartTime = Time.time;
        targetEnemy = Random.Range(3, 15);
        ((LevelController)LevelControllerBase.Instance).AddWingmen();
        //We can also think about dropping in balloons, photographic points, and also enabling and positioning the Ack ack zone
        //Figure out our balloons
        if (Random.value > 0.75f)
        {
            if (LevelChatterController.Instance)
            {
                LevelChatterController.Instance.playChatter("hasballoons");
            }
            addBalloonField(Random.Range(1, 3));    //For the moment
        }
    }

    void addBalloonField(int numFields)
    {
        //Balloons might come with:
        //-Just balloons
        //An added ack ack zone
        //Escort fighters
        //Fighters that spawn in after a balloon is destroyed as a "surprise attack"
        for (int i = 0; i < numFields; i++)
        {
            //Put down a patch of balloons positioned at random around the player
            addBarrageBalloons(PlayerController.Instance.gameObject.transform.position + Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up) * Vector3.forward * Random.Range(75f, 150f), 25f, 1);
        }
    }


    //Vector3 averagePosition = Vector3.zero;
    //For the moment lets make it so that the bombers are always enemy bombers
    public virtual void AddPathBombers(Vector3 thisTargetLocation, float spawnInRange, int numBombers)
    {
        enBombingTeam BombingTeam = enBombingTeam.ENEMY;

        averagePosition = Vector3.zero;
        int newTeam = BombingTeam == enBombingTeam.PLAYER ? 0 : 1;
        float CruiseHeight = fighterSpawnHeight.GetRandom();

        float generalIncomingAngle = Random.Range(0, 360);
        //So basically we need to spawn some bombers in, and assemble a path for them to fly to the target
        float incomingAngle = generalIncomingAngle + Random.Range(-25, 25); //Add some variance for our incoming angle but don't do so in a way that'll have aircraft cross over each other
        Vector3 spawnLocation = thisTargetLocation + Quaternion.AngleAxis(incomingAngle, Vector3.up) * Vector3.forward * spawnInRange;

        Range bomberHeightVariation = new Range(-5, 10);
        spawnLocation = LevelController.Instance.getTerrainHeightAtPoint(spawnLocation) + Vector3.up * (CruiseHeight + bomberHeightVariation.GetRandom());
        averagePosition += spawnLocation;
        //So...our points
        int numIncomingPoints = Random.Range(1, 3); //This will control how much our flightpath will vary up to the target point
        List<Vector3> flightPoints = new List<Vector3>();
        //Debug.Log("incomingPoints: " + numIncomingPoints);
        GameObject newGameObject = new GameObject("Start");
        newGameObject.transform.position = spawnLocation;
        /*
        for (int i = 0; i < numIncomingPoints; i++)
        {
            //So basically we've got a range that we've got to divide into parts here
            float distFraction = (float)(numIncomingPoints - i) / (float)(numIncomingPoints + 1);
            //Debug.Log("Dist Fraction: " + distFraction);
            float newDistPoint = spawnInRange * distFraction;
            Range rotationVariance = new Range(-7, 7);
            incomingAngle += rotationVariance.GetRandom();
            Vector3 nextPoint = thisTargetLocation + Quaternion.AngleAxis(incomingAngle, Vector3.up) * Vector3.forward * spawnInRange * distFraction;
            nextPoint = LevelController.Instance.getTerrainHeightAtPoint(nextPoint) + Vector3.up * (CruiseHeight + bomberHeightVariation.GetRandom());
            //newGameObject = new GameObject("i:" + i.ToString());
            //newGameObject.transform.position = nextPoint;
            flightPoints.Add(nextPoint);
            //Pick rotations for our incoming paths
        }
        */

        //Just make the next point on the other side of everything and have our bombers fly a straight line
        Vector3 nextPoint = thisTargetLocation + Quaternion.AngleAxis(incomingAngle, Vector3.up) * Vector3.forward * -spawnInRange; ;
        nextPoint = LevelController.Instance.getTerrainHeightAtPoint(nextPoint) + Vector3.up * (CruiseHeight + bomberHeightVariation.GetRandom());
        //newGameObject = new GameObject("i:" + i.ToString());
        //newGameObject.transform.position = nextPoint;
        flightPoints.Add(nextPoint);

        //And one for over the target
        Vector3 overtargetPoint = LevelController.Instance.getTerrainHeightAtPoint(thisTargetLocation) + Vector3.up * (CruiseHeight + bomberHeightVariation.GetRandom());
        flightPoints.Add(overtargetPoint);

        //And one for flying past the target point
        Vector3 pastTargetPoint = LevelController.Instance.getTerrainHeightAtPoint(thisTargetLocation + Quaternion.AngleAxis(incomingAngle + 180f, Vector3.up) * Vector3.forward * spawnInRange) + Vector3.up * (CruiseHeight + bomberHeightVariation.GetRandom());
        flightPoints.Add(pastTargetPoint);

        string groupTag = "bomber_Group_" + Time.time.ToString("f0");
        Quaternion startQuat = Quaternion.LookRotation(flightPoints[0] - spawnLocation, Vector3.up);
        //Ok! We need to spawn in our bomber!
        GameObject targetBomber = newTeam == 0 ? prefabManager.Instance.friendlyBombers[0] : prefabManager.Instance.enemyBombers[0];
        actorWrapper newActor = ((LevelController)LevelControllerBase.Instance).addFighterActor(targetBomber, newTeam, spawnLocation, startQuat, groupTag, null);
        //totalBombers++;
        //activeBombers.Add(newActor);
        //Have to get the controller for this vehicle and set the flight points in it
        PathAircraft bomberController = newActor.vehicle.GetComponent<PathAircraft>();
        bomberController.ourMissionConstructor = this;

        if (bomberController)
        {
            bomberController.pathPositions = flightPoints;
            bomberController.targetDropLocation = thisTargetLocation;   //This'll need to be changed for the formation behaviour too
        }
        else
        {
            Debug.LogError("No path Aircraft on bomber");
        }


        //Because we're flying a skirmish we don't want our bomber actually bombing
        bomberController.MissionState = PathAircraft.enMissionState.JUSTFLYING;
        //Ok, cool, our primary bomber is placed as expected. We've got to add our formation bombers which will go
        //      P
        //   1     2
        //      3
        float formationDistance = 20;

        //Really not sure what the best way to add bombers in formation is here
        if (numBombers > 1) //We have 1st position
        {
            actorWrapper newBomber =  addFormationBomber(new Vector3(formationDistance, 0, formationDistance), newTeam, spawnLocation, startQuat, flightPoints, thisTargetLocation);
            PathAircraft newBomberController = newBomber.vehicle.GetComponent<PathAircraft>();
            newBomberController.MissionState = PathAircraft.enMissionState.JUSTFLYING;
        }
        if (numBombers > 2)
        {
            actorWrapper newBomber = addFormationBomber(new Vector3(-formationDistance, 0, formationDistance), newTeam, spawnLocation, startQuat, flightPoints, thisTargetLocation);
            PathAircraft newBomberController = newBomber.vehicle.GetComponent<PathAircraft>();
            newBomberController.MissionState = PathAircraft.enMissionState.JUSTFLYING;
        }
        if (numBombers > 3)
        {
            actorWrapper newBomber = addFormationBomber(new Vector3(0, 0, formationDistance * 2f), newTeam, spawnLocation, startQuat, flightPoints, thisTargetLocation);
            PathAircraft newBomberController = newBomber.vehicle.GetComponent<PathAircraft>();
            newBomberController.MissionState = PathAircraft.enMissionState.JUSTFLYING;
        }

        averagePosition /= numBombers;

        //Need to figure out if we want to add a flight of escort fighters
        
        if (BombingTeam == enBombingTeam.ENEMY)
        {
            //Add bomber escort
            ((LevelController)LevelControllerBase.Instance).AddFighterFlight(LevelController.Instance.getTerrainHeightAtPoint(averagePosition), fighterSpawnHeight.GetRandom(), 20f, Random.Range(2, 4), 1); //These are bomber escorts
        }
    }

    //This should maybe be on the base class? Will have to see
    public virtual void SpawnWave()
    {
        int fightersToSpawn = Mathf.Min(Random.Range(2, 4), enemyRemaining);
        enemyRemaining -= fightersToSpawn;  //Count this down as we work through the fighers
        //Spawn in fighters
        if (Random.value < bomberSpawnChance)
        {
            if (LevelChatterController.Instance)
            {
                LevelChatterController.Instance.playChatter("hasbombers");
            }
            AddPathBombers(PlayerController.Instance.gameObject.transform.position, Random.Range(100f, 150f), Random.Range(1, 4));
        }
        else
        { //just standard fighters 
            if (LevelChatterController.Instance)
            {
                LevelChatterController.Instance.playChatter("hasfighters");
            }
            ((LevelController)LevelControllerBase.Instance).AddFighterFlight(LevelController.Instance.getTerrainHeightAtPoint(PlayerController.Instance.gameObject.transform.position), fighterSpawnHeight.GetRandom(), 20f, fightersToSpawn, 1);
        }
    }

    public override void DoUpdate()
    {
        base.DoUpdate();

        //So if we're over an enemy base we're going to need to keep spawning in fighters to harass the player and the bombers
        if (((LevelController)LevelControllerBase.Instance).enemyList.Count < nextMinSpawnCount && Time.time - levelStartTime > 5f && enemyRemaining > 0) //Make sure that we give everything a breath before we're into it. This might be modified for storytelling reasons
        { 
            nextMinSpawnCount = Random.Range(2, 6); //This should reflect the number of bombers in our flight in order to balance difficulty
            SpawnWave();
        }
        
        if (enemyRemaining <= 0 && ((LevelController)LevelControllerBase.Instance).enemyList.Count == 0 && !bMissionConcluding)
        {
            bMissionConcluding = true;
            NGUI_Base.Instance.setPortraitMessage("Commander", "The skies are clear. Well done!", Color.black);
            DelayFinishMission(true, 5f);
        }
    }
}
