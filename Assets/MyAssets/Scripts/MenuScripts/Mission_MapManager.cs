using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

//manages all of the stuff to do with the map selection, and is a stupidly over-complicated class because of it
[System.Serializable]
public class mapArrayEntry {
	public GameObject mapTile;
	public Mission_MapSection mapScript;
	//public float enemyActivity = 0.1F;
	public int tileTeam = -1;
	public int tileEnt = -1;
}

[System.Serializable]
public class mapTileSaveForm
{
	public int tileTeam = -1;
	public int tileEnt = -1;
}

[System.Serializable]
public class saveForm
{
	public List<mapTileSaveForm> tileStates;
	public List<conflictTile> conflictTiles;
	public int friendlyBaseEnt = -1;
	public int enemyBaseEnt = -1;
}

[System.Serializable]
//Notes on the map for locations of interest
public class keyLocation
{
	public int tileNumber = -1;
	public int team = 0;
	public int level = 1; //Logically the higher the level the more expansive it is, and more difficult
	public enum enKeyLocationType { NULL, STRATEGIC, TOWN, BASE }
	public enKeyLocationType keyLocationType = enKeyLocationType.NULL;
	public keyLocation(int newTileNumber, int newTeam, int newLevel, enKeyLocationType newLocationType)
    {
		tileNumber = newTileNumber;
		team = newTeam;
		level = newLevel;
		keyLocationType = newLocationType;
    }
}

[System.Serializable]
//Do we store conflicts as seperate arrays or on tiles?
public class conflictTile
{
	public conflictTile(int newTileNumber, int newTurnsRemaining, int newTilesGained, int newConflictTeam)
    {
		tileNumber = newTileNumber;
		turnsRemaining = newTurnsRemaining;
		tilesGained = newTilesGained;
		conflictTeam = newConflictTeam;
    }
	public int tileNumber = -1;
	public int turnsRemaining = 3;  //How many turns until this conflict is resolved
	public int tilesGained = 1;	//How many tiles can be gained/lost if this is resolved
	public int conflictTeam = 0; //Which team will win this conflict?
	public bool bTurnValid = true;	//This is disabled if we're removed by an action
}

public class Mission_MapManager : MonoBehaviour {
	public static Mission_MapManager Instance;

	public Mission_SelectPanel ourSelectPanel;

	public MapCameraMovement ourCamera;

	int mapWidth = 8, mapHeight = 18;

	float tileWidth = 120.0f, tileHeight = 69.2F; //these are hard-baked values
	Vector3 alternateStep;

	public GameObject mapTile;	//What will we make our map out of?

	public List<mapArrayEntry> mapArray = new List<mapArrayEntry>();

	int friendlyTeam = 0, enemyTeam = 1;

	public List<conflictTile> conflictTiles = new List<conflictTile>();
	public List<keyLocation> keyLocations = new List<keyLocation>();    //This'll hold things like the enemy base, locations that will net more rewards etc.

	public List<Color> teamColors = new List<Color>() { Color.green, Color.red };

	[HideInInspector] public bool bMapLoaded = false;
	//Called whenever we want to add a new "conflict" point
	int addNewConflictTile(int team, int turnsRemaining, int tilesGained)
	{
		Debug.Log("Team Added: " + team);
		List<int> openTiles = new List<int>();
		//This doesn't have to follow any rhyme or reason.
		//Begin by finding all the tiles that we've got which we can use (must be in conflict) and don't add anything that's already a conflict tile
		for (int i = 0; i < mapArray.Count; i++)
		{
			if (mapArray[i].mapScript.bNoMansLand)
			{
				bool bTileTaken = false;
				foreach (conflictTile t in conflictTiles)
				{
					if (t.tileNumber == i)
					{
						bTileTaken = true;
					}
				}
				if (!bTileTaken)    //We can add our tile to any of these
				{
					openTiles.Add(i);
				}
			}
		}
		int entryNumber = Random.RandomRange(0, openTiles.Count);

		int newTileNumber = openTiles[entryNumber]; //All that to figure out which tile we're going to use...
													//We need a random number for time remaining, and some sort of comparative for number of tiles lost. For the moment lets just dump down some random numbers
		conflictTile newConflictTile = new conflictTile(newTileNumber, turnsRemaining, tilesGained, team);
		conflictTiles.Add(newConflictTile);

		//Seeing as this is called after the map is resolved we need to set our tile tints here
		mapArray[newTileNumber].mapScript.setConflictMarker(teamColors[team], team, turnsRemaining, true);
		return newTileNumber;
	}

	void addKeyLocation(int team, keyLocation.enKeyLocationType locationType)
    {
		bool bValidTileFound = false;
		int cycles = 0;
		while (!bValidTileFound && cycles < 10)
        {
			cycles++;
			//Just grab a random tile and make it important by putting a key location on it
			int randomTile = Mathf.FloorToInt(Random.Range(0, mapArray.Count));
			if (mapArray[randomTile].mapScript.team == enemyTeam) //This one should technically be fine
            {
				bValidTileFound = true;
				keyLocation newKeyLocation = new keyLocation(randomTile, enemyTeam, 5, locationType);
				keyLocations.Add(newKeyLocation);

				//We also need to change the graphic for our tile
				mapArray[randomTile].mapScript.setKeyArea(true);

			}
        }
    }

	public void GenerateFreshMap()
	{
		StartCoroutine(mapGenerate());
	}

	IEnumerator mapGenerate() {

		//We need to scram our map before generating a new one
		while (transform.childCount > 0)
        {
			foreach(Transform thisTrans in transform)
            {
				Destroy(thisTrans.gameObject);
            }
			yield return null;
        }
		Debug.Log("Generating Map");
		mapArray.Clear();
		mapArray = new List<mapArrayEntry>();
		conflictTiles.Clear();
		conflictTiles = new List<conflictTile>();
		populateMap();
		runMapTints(false);  //Necessary to figure out what's no-mans land
							 //Add a few conflict tiles
		addNewConflictTile(enemyTeam, 3, 2);
		//addNewConflictTile(enemyTeam, 4, 4);
		addNewConflictTile(enemyTeam, 2, 1);
		addNewConflictTile(friendlyTeam, 2, 2);

		//We could do with adding a smattering of Key areas for the enemy team, somehow...
		addKeyLocation(enemyTeam, keyLocation.enKeyLocationType.STRATEGIC);
		addKeyLocation(enemyTeam, keyLocation.enKeyLocationType.STRATEGIC);
		addKeyLocation(enemyTeam, keyLocation.enKeyLocationType.STRATEGIC);
		addKeyLocation(enemyTeam, keyLocation.enKeyLocationType.BASE);
		addKeyLocation(enemyTeam, keyLocation.enKeyLocationType.BASE);
		addKeyLocation(friendlyTeam, keyLocation.enKeyLocationType.TOWN);
		addKeyLocation(friendlyTeam, keyLocation.enKeyLocationType.TOWN);
		addKeyLocation(friendlyTeam, keyLocation.enKeyLocationType.TOWN);
		addKeyLocation(friendlyTeam, keyLocation.enKeyLocationType.TOWN);

		bMapLoaded = true;
	}

	// Use this for initialization
	void Start () {
		Instance = this;
		alternateStep = new Vector3(-mapWidth/2F, 0, -mapHeight/2F); //used to offset our hex areas
		if (!SaveUtility.Instance.CheckSaveFile("mapTileState.json"))
		{ //we need to make a new map
			GenerateFreshMap();
		} else
        {
			loadMapState();
			runMapTints(false);
			bMapLoaded = true;
		}
	}

    #region ProxyTileFunctions
    public void winTile() {
		//need to reset this tile with a winning condition
		playTile (friendlyTeam);
		//for the moment...
		ResolveConflictTile(ourSelectPanel.selectedTile, friendlyTeam, true);
		runMapTints(true);
	}
	
	public void looseTile() { //this isn't really a "message" now is it?
		playTile (enemyTeam);
		runMapTints(true);
	}
	#endregion

	float tileViewDistance = 5f;
	float cameraTileMoveSpeed = 0.5f;
	float cameraTileWait = 1f;
	public Vector3 GetCameraLookAtTile(int thisTileNumber)
	{
		return mapArray[thisTileNumber].mapTile.transform.position - ourCamera.transform.forward * tileViewDistance;
	}

	public void endTurn()
	{
		StartCoroutine(ShowEndMapResults(flownTile, true));
	}

	IEnumerator ShowEndMapResults(int flownTileNumber, bool bTileWon) {

		//PROBLEM: We might need a delay until after this map has finished loading
		//Start with what our player view should be doing
		yield return new WaitForSeconds(1f);
		ourCamera.DoMoveToPosition(GetCameraLookAtTile(flownTileNumber));	//Move our camera to this position
		yield return new WaitForSeconds(1f);
		//if we win this tile it becomes friendly territory
		if (!mapArray[flownTileNumber].mapScript.bIsConflicted)
		{
			SetTileTeam(flownTileNumber, bTileWon ? friendlyTeam : enemyTeam);
			Debug.Log("Flown Tile colour set");
		}
		else
		{
			Debug.Log("Flown Tile was a conflict tile");
			ResolveConflictTile(flownTileNumber, friendlyTeam, true);   //If we've flown a conflict we want it to either be removed or completed
		}
		runMapTints(true);

		yield return new WaitForSeconds(1f);	//Wait to show the results of our battle. This might need an icon of sorts

		//Go through the conflict tiles and see if anything has changed
		//Update the map to reflect this showing things changing (somehow) - could this be set on the tiles themselves so they flash?
		runMapTints(true); //Go through and fix up all the map tints
						   //We need to look at what our conflicts are doing
		//So...
		if (conflictTiles.Count < 4)
		{
			//Perhaps we add some more tiles if there aren't enough
			if (true || Random.value > 0.75f)
			{
				int newConflict = addNewConflictTile(Random.value < 0.25 ? friendlyTeam : enemyTeam, Mathf.RoundToInt(Random.RandomRange(3, 6)), 1);
				ourCamera.DoMoveToPosition(GetCameraLookAtTile(newConflict)); //Move our camera to this position
				yield return new WaitForSeconds(2f);
			}
		}
		//Dependig on the number of conflictTiles we might want to add some more
		foreach (conflictTile thisTile in conflictTiles)
		{
			//We want to move the camera to this tile, and show it counting down. 
			//For the moment shall we just handle them basically
			//Decrease our day count on the conflict tiles
			//If at zero resolve
			if (thisTile.bTurnValid)	//if we've been deactivated don't bother running this tile as it lost out in chess-space
			{
				ourCamera.DoMoveToPosition(GetCameraLookAtTile(thisTile.tileNumber)); //Move our camera to this position
				yield return new WaitForSeconds(0.75f);
				thisTile.turnsRemaining--;
				if (thisTile.turnsRemaining <= 0)
				{
					//need to resolve this tile, which involves switching the teams for the six touching it's sides
					ResolveConflictTile(thisTile.tileNumber, thisTile.conflictTeam, false);
					thisTile.bTurnValid = false;	//Flat this tile for removal
					mapArray[thisTile.tileNumber].mapScript.setConflictMarker(Color.white, 0, 0, false);
				}
				else
				{
					//Update our tile
					mapArray[thisTile.tileNumber].mapScript.setConflictMarkerText(thisTile.turnsRemaining.ToString());
					mapArray[thisTile.tileNumber].mapTile.transform.DOPunchScale(Vector3.one * 0.25f, 0.5f, 2);
				}
				runMapTints(true);
				yield return new WaitForSeconds(1f);
			}
		}


		//Finally we need to go through our conflicts and see if any have been invalidated
		foreach (conflictTile thisTile in conflictTiles)
		{
			if (!thisTile.bTurnValid)
            {
				conflictsToRemove.Add(thisTile);
            }
		}
		
		//See if we need to remove some of the conflict markers and tiles
		if (conflictsToRemove.Count > 0)
		{
			foreach (conflictTile removeTile in conflictsToRemove)
			{
				//Clear our marker...
				Debug.Log("Doing Tile Remove");
				RemoveConflictTile(removeTile, true);
				//I think the problem  is that this value has changed by the time we come to do this
			}
		}
		runMapTints(true); //So that we can see things resolve
		ourCamera.returnToStart();
	}

	public List<conflictTile> conflictsToRemove = new List<conflictTile>();
	public void ResolveConflictTile(int tileNumber, int conflictTeam, bool PlayerIntervened)
	{
		foreach (conflictTile thisTile in conflictTiles)
		{
			if (thisTile.tileNumber == tileNumber) //we have a hit!
			{
				//Debug.Log("Have Conflict Tile");
				if (thisTile.conflictTeam == enemyTeam)
				{
					if (PlayerIntervened)
					{
						//for the moment we'd just remove this conflict						
						//mapArray[thisTile.tileNumber].mapScript.setConflictMarker(teamColors[thisTile.conflictTeam], thisTile.conflictTeam, -1, false);
						//conflictTiles.Remove(thisTile);   //And remove our entry
						//conflictsToRemove.Add(thisTile);
						thisTile.bTurnValid = false;	//This will mark us for removal
						mapArray[thisTile.tileNumber].mapScript.setConflictMarker(Color.white, -1, -1, false);  //Disable this conflict marker
					}
					else
					{
						ConflictWinTiles(tileNumber, conflictTeam); //In theory...
						mapArray[thisTile.tileNumber].mapScript.setConflictMarker(Color.white, -1, -1, false);  //Disable this conflict marker
					}
				} else {  //handle our friendly team
					//Debug.Log("Doing Friendly Resolve");
					ConflictWinTiles(tileNumber, conflictTeam); //In theory...
					//conflictsToRemove.Add(thisTile);
					thisTile.bTurnValid = false;
					mapArray[thisTile.tileNumber].mapScript.setConflictMarker(Color.white, -1, -1, false);  //Disable this conflict marker
				}
			}
		}
	}

	void RemoveConflictTile(conflictTile thisTile, bool bAlsoRemoveEntry)
    {
		mapArray[thisTile.tileNumber].mapScript.setConflictMarker(teamColors[thisTile.conflictTeam], thisTile.conflictTeam, -1, false);
		if (bAlsoRemoveEntry)
        {
			conflictTiles.Remove(thisTile);
		}
	}

	public void ConflictClaimTile(int tileNum, int conflictTeam)
	{
		mapArray[tileNum].mapScript.setTeam(conflictTeam);
		if (mapArray[tileNum].mapScript.bIsConflicted)
        {
			if (mapArray[tileNum].mapScript.conflictTeam != conflictTeam)
            {
				mapArray[tileNum].mapScript.setConflictMarker(Color.white, -1, -1, false);	//Disable this conflict marker
				//of course we also have to find our entry and remove it
				foreach (conflictTile thisConflict in conflictTiles)
                {
					if (thisConflict.tileNumber == tileNum)
                    {
						thisConflict.bTurnValid = false;
					}
                }
			}
        }
	}

	//We need to check if these tiles overlap with other conflicts on the other team, and if they do to remove said conflict, or do we?
	public void ConflictWinTiles(int tileNum, int conflictTeam)
    {
		//Essentially this is a copy of the tile touching code with a few tweaks...
		float rowRepeat = Mathf.Repeat(tileNum, 16);
		bool bOddRow = rowRepeat > 7;
		//Lets do a quick naming set...
		//mapArray[tileNum].mapScript.setTileLabel(tileNum.ToString() + (bOddRow ? "o" : "e"));
		//Logically there are  six tiles around this one, and we want to see if one of them is a team equal to "thisTeam"
		//			n+width*2
		//	n+width			n+width-1
		//		        n                   
		//  n-width			n-width-1
		//			n-width*2

		//There's a werid outside case where we can not set the tile we're on after things have happened with the different randoms
		mapArray[tileNum].mapScript.setTeam(conflictTeam);

		int tileTop = tileNum + mapWidth * 2;
		if (tileTop < mapArray.Count - 1)
		{
			//mapArray[tileTop].mapScript.setTeam(conflictTeam);
			ConflictClaimTile(tileTop, conflictTeam);
			//Check and see if this tile has an enemy conflict on it, and if it does then remove it
		}
		//Left edge: n is a multiple of 2 to width i.e. 0, 16, 32, 48
		//Check to see if our tile is on the right edge before allowing checks on the right
		float leftEdgeMod = (float)(tileNum + 1) / (float)(mapWidth * 2f);
		/*
		if (leftEdgeMod == Mathf.Floor(leftEdgeMod))
		{
			mapArray[tileNum].mapScript.setTileLabel(tileNum.ToString() + "L");
		}*/

		//Debug.Log("edgMod: " + leftEdgeMod);
		//Somehow this is all inverted from the numberings?
		if (leftEdgeMod != Mathf.Floor(leftEdgeMod))
		{ //are we an edge tile
		  //Debug.Log("Not left edge tile");
			int tileTopLeft = tileNum + mapWidth + (bOddRow ? 1 : 0);
			if (tileTopLeft < mapArray.Count - 1)
			{
				//mapArray[tileTopLeft].mapScript.setTeam(conflictTeam);
				ConflictClaimTile(tileTopLeft, conflictTeam);
				//if (mapArray[tileTopLeft].mapScript.team == thisTeam) { return true; }
			}
			int tileBottomLeft = tileNum - mapWidth + (bOddRow ? 1 : 0);
			if (tileBottomLeft > 0)
			{
				//mapArray[tileBottomLeft].mapScript.setTeam(conflictTeam);
				ConflictClaimTile(tileBottomLeft, conflictTeam);
				
				//if (mapArray[tileBottomLeft].mapScript.team == thisTeam) { return true; }
			}
		}
		else
		{
			//Debug.Log("Left Edge Tile");
		}

		float rightEdgeMod = (float)(tileNum) / (float)(mapWidth * 2.0);
		/*
		if (rightEdgeMod == Mathf.Floor(rightEdgeMod))
		{
			mapArray[tileNum].mapScript.setTileLabel(tileNum.ToString() + "R");
		}*/

		if (rightEdgeMod != Mathf.Floor(rightEdgeMod))
		{ //are we an edge tile
			int tileTopRight = tileNum + mapWidth + (bOddRow ? 0 : -1);
			if (tileTopRight < mapArray.Count - 1)
			{
				//mapArray[tileTopRight].mapScript.setTeam(conflictTeam);
				ConflictClaimTile(tileTopRight, conflictTeam);
				//if (mapArray[tileTopRight].mapScript.team == thisTeam) { return true; }
			}
			int tileBottomRight = tileNum - mapWidth + (bOddRow ? 0 : -1);
			if (tileBottomRight > 0)
			{
				//mapArray[tileBottomRight].mapScript.setTeam(conflictTeam);
				ConflictClaimTile(tileBottomRight, conflictTeam);
				//if (mapArray[tileBottomRight].mapScript.team == thisTeam) { return true; }
			}
		}
		else
		{
			//Debug.Log("this is a right edge tile");
		}

		//Bottom tile:
		int tileBottom = tileNum - mapWidth * 2;
		if (tileBottom >= 0)
		{
			//mapArray[tileBottom].mapScript.setTeam(conflictTeam);
			ConflictClaimTile(tileBottom, conflictTeam);
			Debug.Log("tileBottom: " + tileBottom);
			//if (mapArray[tileBottom].mapScript.team == thisTeam) { return true; }
		}
		//return false;
	}
	
	//called if we're doing something with a tile such as changing teams
	public void playTile(int team) {
		//so the basic rules are that we'll get a unitsChange from the damage we do to this tile.
		SetTileTeam(ourSelectPanel.selectedTile, team);
	}
	void SetTileTeam(int thisTile, int team)
	{
		if (team != mapArray[thisTile].mapScript.team)
		{
			//reduce the tile strength by the units change
			//but for now just set it...
			mapArray[thisTile].tileTeam = team;
			mapArray[thisTile].mapScript.setTeam(team);
			if (team == 0)
			{
				mapArray[thisTile].mapScript.setTint(Color.Lerp(Color.white, Color.green, 0.33f), false, false);
			}
			else
			{
				mapArray[ourSelectPanel.selectedTile].mapScript.setTint(Color.Lerp(Color.white, Color.red, 0.33f), false, false);
			}
		}
	}

	public void selectMissionElements(int tileNumber, conflictTile thisConflict)
    {
		//So I want something that says "we need to do X as a primary goal" and then add in a secondary, and a few other events for giving it spice...
		//This'll then need to reflect things like conflictTiles (which might have two or more primary goals) and also elevate with difficulty

		//So we need to know what we have to do...
		//Down bombers/protect bombers
		//Destroy convoy/protect convoy
		//=> May include trucks, tanks, mobile artillery, trains
		//Destroy ground targets/protect ground targets
		//=> Defined ground targets: tanks, artillery, fuel silos, ackack cannons
		//Strafing run (is this a different game mode?)
		//Special case rescue missions where the player has to shoot the towers on a prison camp, and then cover the vans as they escape
		//Reconnance (could actually be a pre-requsite to targets and getting requisition points before the game points things out to the player)

		//First we want to see if this is a conflict tile as that'll dictate the sort of thing we're doing (more complex, multi-stage)
		if (thisConflict!=null)
        {
			//Because in this state we're going to always have to include attacking key ground targets while being harassed by fighters
        }

		/*
		Really this needs to be a list with things like:
		["Destroy 5 enemy fuel silos",
		base world generation step,
		setup mission: base with vehicles, silos, ack ack guns, player start position,
		add additional mission extra goals (ground targets, photo points, things that can be put to one side)
		target sight highlited,
		enemy fighters within X distance of silo,
		enemy fighters spawned after first two silos destroyed,
		enemy fighters spawned after forth silo destroyed,
		spawn more fighters after fifth silo destroyed,
		mop up fighters to complete mission
		]
		*/

    }

	//the player has pressed this button, check it over
	public void selectMapSegment(int thisTile) {
		//Check this tile to see if it's "flyable"
		//...linked tiles are:
		//   2
		//1      3
		//   b
		//4      5
		//   6

		//1: number + width - 1
		//2: number + height
		//3: number + width
		//4: number - width - 1
		//5: number - width
		//6: number - height
		if (tileIsTouching(thisTile, friendlyTeam) || mapArray[thisTile].mapScript.team==friendlyTeam) { //acceptable tile selection
			//We need to check if this is a conflict tile
			ourSelectPanel.selectThis(thisTile); //call up our information and select panel
			//Debug.LogError("Tile Valid");
		} else
        {
			//Debug.LogError("Tile not Valid");
        }
	}

	//need a function that we can query easily when it comes to checking if a tile is associated with another team...
	public bool tileIsTouching(int tileNum, int thisTeam) {
		float rowRepeat = Mathf.Repeat(tileNum, 16);
		bool bOddRow = rowRepeat > 7;
		//Lets do a quick naming set...
		//mapArray[tileNum].mapScript.setTileLabel(tileNum.ToString() + (bOddRow ? "o" : "e"));
		//Logically there are  six tiles around this one, and we want to see if one of them is a team equal to "thisTeam"
		//			n+width*2
		//	n+width			n+width-1
		//		        n                   
		//  n-width			n-width-1
		//			n-width*2

		int tileTop = tileNum + mapWidth * 2;
		if (tileTop < mapArray.Count - 1)
        {
			if (mapArray[tileTop].mapScript.team == thisTeam) { return true; }
        }
		//Left edge: n is a multiple of 2 to width i.e. 0, 16, 32, 48
		//Check to see if our tile is on the right edge before allowing checks on the right
		float leftEdgeMod = (float)(tileNum+1) / (float)(mapWidth*2f);
		if (leftEdgeMod == Mathf.Floor(leftEdgeMod))
        {
			mapArray[tileNum].mapScript.setTileLabel(tileNum.ToString() + "L");
		}
		//Debug.Log("edgMod: " + leftEdgeMod);
		//Somehow this is all inverted from the numberings?
		if (leftEdgeMod != Mathf.Floor(leftEdgeMod))
		{ //are we an edge tile
			//Debug.Log("Not left edge tile");
			int tileTopLeft = tileNum + mapWidth + (bOddRow ? 1 : 0);
			if (tileTopLeft < mapArray.Count - 1)
			{
				if (mapArray[tileTopLeft].mapScript.team == thisTeam) { return true; }
			}
			int tileBottomLeft = tileNum - mapWidth + (bOddRow ? 1 : 0);
			if (tileBottomLeft > 0)
			{
				if (mapArray[tileBottomLeft].mapScript.team == thisTeam) { return true; }
			}
		} else
        {
			//Debug.Log("Left Edge Tile");
        }
		
		float rightEdgeMod = (float)(tileNum) / (float)(mapWidth * 2.0);
		if (rightEdgeMod == Mathf.Floor(rightEdgeMod))
        {
			mapArray[tileNum].mapScript.setTileLabel(tileNum.ToString() + "R");
		}

		if (rightEdgeMod != Mathf.Floor(rightEdgeMod))
		{ //are we an edge tile
			int tileTopRight = tileNum + mapWidth + (bOddRow ? 0 : -1);
			if (tileTopRight < mapArray.Count - 1)
			{
				if (mapArray[tileTopRight].mapScript.team == thisTeam) { return true; }
			}
			int tileBottomRight = tileNum - mapWidth + (bOddRow? 0 :-1);
			if (tileBottomRight > 0)
			{
				if (mapArray[tileBottomRight].mapScript.team == thisTeam) { return true; }
			}
		} else
        {
			//Debug.Log("this is a right edge tile");
        }

		//Bottom tile:
		int tileBottom = tileNum - mapWidth * 2;
		if (tileBottom > 0)
        {
			if (mapArray[tileBottom].mapScript.team == thisTeam) { return true; }
		}
		return false;
	}

	public void runMapTints(bool bAnimateTint)
    {
		//We have to go through all the tiles and see if an enemy tile is touching a green tile, and if it is color it white
		for (int i=0; i<mapArray.Count; i++)
        {
			if (mapArray[i].mapScript.team == enemyTeam && tileIsTouching(i, friendlyTeam))
            {
				//This is a contestable tile
				mapArray[i].mapScript.setTint(Color.white, true, bAnimateTint);
				mapArray[i].mapScript.bNoMansLand = true;
            } else
            {
				mapArray[i].mapScript.bNoMansLand = false;
				//Handle our other colour tints
				if (mapArray[i].mapScript.team == friendlyTeam) {
					mapArray[i].mapScript.setTint(Color.Lerp(Color.white, Color.green, 0.33f), false, bAnimateTint);
				}
				else if (mapArray[i].mapScript.team == enemyTeam) {
					mapArray[i].mapScript.setTint(Color.Lerp(Color.white, Color.yellow, 0.33f), false, bAnimateTint);
				}
			}
        }
    }

	void populateMap()
	{ //go through and put down all our markers
	  //blank map population function

		int ent = 0;
		//We could use a gradient of some description to try and add some variance to the map as to what's friendly and what isn't, and having a couple of different ones could really make for interesting starting situations

		//So we've got layers stepped across, then stagger, then step
		for (int y = 0; y < mapHeight; y++)
		{
			for (int x = 0; x < mapWidth; x++)
			{
				GameObject newMapTile = Instantiate(mapTile) as GameObject;

				newMapTile.transform.parent = transform; //set it to this, which will also be linked to the handler

				if (Mathf.Approximately(y / 2F, y / 2)) //this is a normal section
					newMapTile.transform.localPosition = new Vector3((x - mapWidth / 2F) * tileWidth, (mapHeight / 4F - y / 2F) * tileHeight, 0);
				else
					newMapTile.transform.localPosition = new Vector3((x - mapWidth / 2F) * tileWidth + tileWidth / 2, (mapHeight / 4F - y / 2F) * tileHeight, 0);

				newMapTile.transform.localScale = new Vector3(1, 1, 1);
				newMapTile.transform.localEulerAngles = new Vector3(0, 0, 180);
				//populate our tile with details
				Mission_MapSection mapSelection = newMapTile.GetComponent<Mission_MapSection>();

				mapSelection.ourManager = this;
				mapSelection.segmentNumber = ent; //so we can look back on this


				//put our entry into the array for future reference
				mapArrayEntry newMapEntry = new mapArrayEntry();
				newMapEntry.mapTile = newMapTile;
				newMapEntry.mapScript = mapSelection;
				//newMapEntry.enemyActivity = (float)y/(float)mapHeight;

				//we'll either be loading the map detail or setting it up automatically
				mapSelection.setTileLabel("" + ent);

				newMapEntry.tileEnt = ent;
				newMapEntry.tileTeam = enemyTeam;
				mapArray.Add(newMapEntry);

				ent++; //cycle this up one
			}
		}

		//we need to populate our map for starting colours (friendly/otherwise) and it's best if it's not in the above loop
		//Lets just assume a seeded starting point
		int playerStartTile = Mathf.FloorToInt(Random.Range(0, 15));
		Vector3 startTilePosition = mapArray[playerStartTile].mapTile.transform.position;
		float playerStartRadius = 3f;	//I've no idea what this'll be yet

		for (int i = 0; i< mapArray.Count; i++)
		{
			bool bPlayerOwned = Vector3.Distance(startTilePosition, mapArray[i].mapTile.transform.position) < playerStartRadius;
			mapArray[i].mapScript.setTeam(bPlayerOwned ? friendlyTeam : enemyTeam);
			//mapSelection.setTeam(friendlyTeam); //Unsure if we still use this?
			//newMapEntry.mapScript.setTeam(friendlyTeam);
			mapArray[i].mapScript.setTint(Color.Lerp(Color.white, bPlayerOwned ? Color.green : Color.red, 0.33f), false, false);
			mapArray[i].tileTeam = bPlayerOwned ? friendlyTeam : enemyTeam;
		}
	}

	void populateMapFromSave(saveForm thisSaveForm)
	{ //go through and put down all our markers
	  //blank map population function

		//As opposed to making a map, loading a map involves "unwrapping" it
		for (int ent=0; ent<thisSaveForm.tileStates.Count; ent++)
        {
			//from the ent we need to figure out what our tile is
			int y = Mathf.FloorToInt(ent / mapWidth);
			int x = ent - y * mapWidth;

			GameObject newMapTile = Instantiate(mapTile) as GameObject;

			newMapTile.transform.SetParent(transform); //set it to this, which will also be linked to the handler

			if (Mathf.Approximately(y / 2F, y / 2)) //this is a normal section
				newMapTile.transform.localPosition = new Vector3((x - mapWidth / 2F) * tileWidth, (mapHeight / 4F - y / 2F) * tileHeight, 0);
			else
				newMapTile.transform.localPosition = new Vector3((x - mapWidth / 2F) * tileWidth + tileWidth / 2, (mapHeight / 4F - y / 2F) * tileHeight, 0);

			newMapTile.transform.localScale = new Vector3(1, 1, 1);
			newMapTile.transform.localEulerAngles = new Vector3(0, 0, 180);
			//populate our tile with details
			Mission_MapSection mapSelection = newMapTile.GetComponent<Mission_MapSection>();

			mapSelection.ourManager = this;
			mapSelection.segmentNumber = ent; //so we can look back on this


			//put our entry into the array for future reference
			mapArrayEntry newMapEntry = new mapArrayEntry();
			newMapEntry.mapTile = newMapTile;
			newMapEntry.mapScript = mapSelection;
			//newMapEntry.enemyActivity = (float)y/(float)mapHeight;

			//we'll either be loading the map detail or setting it up automatically
			mapSelection.setTileLabel("" + ent);

			//sort out our sides (basic assignment for the moment)
			if (thisSaveForm.tileStates[ent].tileTeam == friendlyTeam)
			{ //this can be our "base ours"
				mapSelection.setTeam(friendlyTeam); //Unsure if we still use this?
				newMapEntry.mapScript.setTeam(friendlyTeam);
				mapSelection.setTint(Color.Lerp(Color.white, Color.green, 0.33f), false, false);
				newMapEntry.tileTeam = friendlyTeam;
			}
			else
			{    //The enemy tiles aren't getting a number assigned correctly (the tiles are remaining at -1)
				mapSelection.setTeam(enemyTeam);    //Unsure if we still use this
				newMapEntry.mapScript.setTeam(enemyTeam);
				mapSelection.setTint(Color.Lerp(Color.white, Color.red, 0.33f), false, false);
				newMapEntry.tileTeam = enemyTeam;
			}
			newMapEntry.tileEnt = ent;
			mapArray.Add(newMapEntry);
		}

		foreach (conflictTile thisConflict in thisSaveForm.conflictTiles)
        {
			conflictTile newConflictTile = new conflictTile(thisConflict.tileNumber, thisConflict.turnsRemaining, thisConflict.tilesGained, thisConflict.conflictTeam);
			conflictTiles.Add(newConflictTile);

			//Seeing as this is called after the map is resolved we need to set our tile tints here
			mapArray[thisConflict.tileNumber].mapScript.setConflictMarker(teamColors[thisConflict.conflictTeam], thisConflict.conflictTeam, thisConflict.turnsRemaining, true);
		}
	}

	#region SaveMapFunctions
	public void saveMapState()
    {
		saveForm ourSaveForm = new saveForm();
		ourSaveForm.tileStates = new List<mapTileSaveForm>();

		for (int i=0; i<mapArray.Count; i++)
        {
			mapTileSaveForm newMapSave = new mapTileSaveForm();
			newMapSave.tileTeam = mapArray[i].tileTeam;
			newMapSave.tileEnt = mapArray[i].tileEnt;
			ourSaveForm.tileStates.Add(newMapSave);
        }
	
		ourSaveForm.conflictTiles = new List<conflictTile>();
		for (int i=0; i<conflictTiles.Count; i++)
        {
			ourSaveForm.conflictTiles.Add(conflictTiles[i]);
        }
		
		string mapTileState = JsonUtility.ToJson(ourSaveForm);
		SaveUtility.Instance.SaveTXTFile(mapTileState, "mapTileState.json");
		/*
		System.IO.File.WriteAllText(Application.persistentDataPath + "/mapTileState.json", mapTileState);
		Debug.Log(mapTileState);
		Debug.Log("MapSaved: " + Application.persistentDataPath + "/mapTileState.json");
		*/
	}

	/*
	public bool checkSavedMap()
    {
		//Check and see if we've got a saved map state
		if (!System.IO.File.Exists(Application.persistentDataPath + "/mapTileState.json"))
		{
			return false;
		}
		return true;
	}*/

	public void loadMapState()
    {   /*
		saveForm ourSaveForm = new saveForm();
		Debug.Log("Loading Map State");
		if (System.IO.File.Exists(Application.persistentDataPath + "/mapTileState.json"))
		{
			string fileData = System.IO.File.ReadAllText(Application.persistentDataPath + "/mapTileState.json");
			ourSaveForm = JsonUtility.FromJson<saveForm>(fileData);
		}*/
		if (SaveUtility.Instance.CheckSaveFile("mapTileState.json"))
		{
			string saveText = SaveUtility.Instance.LoadTXTFile("mapTileState.json");
			saveForm ourSaveForm = new saveForm();
			ourSaveForm = JsonUtility.FromJson<saveForm>(saveText);
			populateMapFromSave(ourSaveForm);
		}
	}
	#endregion

	#region GameMapReturnFunctions
	int flownTile = 56;

	//This is where our gameManager will send a callback following a mission concluding. We'll evaluate our tile and see what's changed
	public void LevelCompleted(int tileNumber, bool bWon)
    {
		//We need a delay until acknowledgement or some sort of camera focus thing here so that we know what our outcome is
		//if we win this tile it becomes friendly territory
		/*
		SetTileTeam(tileNumber, bWon ? friendlyTeam : enemyTeam);
		runMapTints(true);
		//After this turn we need to go around our events and see if any of them have changed or need updating
		*/
		flownTile = tileNumber;
		//endTurn();	//Make sure we count down our turn here too
		StartCoroutine(ShowEndMapResults(tileNumber, bWon));
		//saveMapState(); //For the moment this can go here
	}
    #endregion
}
