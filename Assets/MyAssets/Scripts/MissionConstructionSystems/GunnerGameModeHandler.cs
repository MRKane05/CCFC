using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Because a gunner mission is different from a standard objective-based mission we need a different handler for enemies which will be spawned in waves
public class GunnerGameModeHandler : MissionConstructionBase {

	protected float dropRadius = 20; //might need to modulate this...
									 //I expect we can handle almost everything from here actually
	float levelStartTime = 0;
	public float levelDuration = 180; //How long until we enter the bombing run (in seconds)

	bool bNextLevelLoading = false;

	int nextMinSpawnCount = 2;	//At what number of planes remaining will we spawn more?

	public override void DoStart()
    {
		nextMinSpawnCount = Random.Range(1, 3);
		levelStartTime = Time.time;
		//We could do with giving the player some escort wingmen

		//This isn't working because of speeds. It's going to need a little extra AI logic I think
		//Wingmen really don't work in this case, but what would is friendly bombers in formation
	}
	
	public override void DoUpdate()
    {

		//Check to see if we should keep adding enemies into the level to harass the player
		if (((LevelController)LevelControllerBase.Instance).enemyList.Count < nextMinSpawnCount && Time.time-levelStartTime > 5f)	//Make sure that we give everything a breath before we're into it. This might be modified for storytelling reasons
        {
			((LevelController)LevelControllerBase.Instance).AddFlightGroup(PlayerController.Instance.ourAircraft.transform.position, 30f, 3f, Random.Range(0f, 1f), Random.Range(2, 4), 1); //Add a fighter group, and have it on the player immediately
			//AddFlightGroup(PlayerController.Instance.ourAircraft.transform.position, 30f, 3f, Random.Range(0f, 1f), Random.Range(2, 4), 1); //Add a fighter group, and have it on the player immediately
			//((LevelController)LevelControllerBase.Instance).AddFighterFlight(LevelController.Instance.getTerrainHeightAtPoint(ourBaseGenerator.baseParent.transform.position) + Vector3.up * Random.Range(20f, 50f), 20f, Random.Range(2, 4), BombingTeam == enBombingTeam.PLAYER ? 1 : 0); //These are for base defense

		}

		if (Time.time-levelStartTime > levelDuration && !bNextLevelLoading)
        {
			bNextLevelLoading = true;	//PROBLEM: I really need a better way to handle this because this is very hacky
			((LevelController)LevelControllerBase.Instance).finishMatch(false);
        }
    }
}
