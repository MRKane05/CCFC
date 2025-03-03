using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mission_Skirmish : MissionConstructionBase {
    public int targetEnemy = 3; //How many enemy will we have to drop for this mission?
    public int enemyRemaining = 3; //How many enemy are left to spawn?
    int nextMinSpawnCount = 2;	//At what number of planes remaining will we spawn more?
    float levelStartTime = 0;
    public Range fighterSpawnHeight = new Range(30, 50);

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
        ((LevelController)LevelControllerBase.Instance).AddWingmen();
    }

    public override void DoUpdate()
    {
        base.DoUpdate();

        //So if we're over an enemy base we're going to need to keep spawning in fighters to harass the player and the bombers
        if (((LevelController)LevelControllerBase.Instance).enemyList.Count < nextMinSpawnCount && Time.time - levelStartTime > 5f) //Make sure that we give everything a breath before we're into it. This might be modified for storytelling reasons
        {
            int fightersToSpawn = Mathf.Min(Random.Range(2, 4), enemyRemaining);
            enemyRemaining -= fightersToSpawn;  //Count this down as we work through the fighers
            nextMinSpawnCount = Random.Range(2, 6); //This should reflect the number of bombers in our flight in order to balance difficulty
            ((LevelController)LevelControllerBase.Instance).AddFighterFlight(LevelController.Instance.getTerrainHeightAtPoint(PlayerController.Instance.gameObject.transform.position), fighterSpawnHeight.GetRandom(), 20f, fightersToSpawn, 1);
        }
        
        if (enemyRemaining <= 0 && ((LevelController)LevelControllerBase.Instance).enemyList.Count == 0 && !bMissionConcluding)
        {
            bMissionConcluding = true;
            NGUI_Base.Instance.setPortraitMessage("Commander", "The skies are clear. Well done!", Color.black);
            DelayFinishMission(true, 5f);
        }
    }
}
