using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using static Mission_MapSection;

[System.Serializable]
public class LevelResults
{
	public bool bWonLevel = true;
}

//This is the core of all the game mechanics, it handles the information transfer sections basically.
//Can't be too cluttered
public class gameManager : MonoBehaviour {
	
	private static gameManager instance = null;
	public static gameManager Instance {get {return instance;}}
	
	public enum enGameState { NULL, MENU, LEVELSETUP, LEVELPLAYING, LEVELENDED, PAUSE}	//for VitaHOT I had states on both the LevelController and gameManager, so we're going to try and put everything here
	public enGameState GameState = enGameState.NULL;

	public int turnNumber = -1;	//Default unpopulated
	public void setGameState(enGameState newState)
    {
		GameState = newState;
    }

	float enemyActivity=0;
	int currentlySelectedTile = -1;
	//need some way of knowing which level we'd want to load

	#region Level Results to Pass Through
	public LevelResults levelResults;
	#endregion

	#region Level Load Screen
	public TextMeshProUGUI loadingScreenMessage;
    public GameObject LoadingScreenBase;
	public Slider LoadingSlider;
	#endregion

	#region Universal Debug Flags
	public bool bDebugEvents = false;
	#endregion

	#region Level Selection Details
	public bool bCanSelectMission = true;
	public int selectedTile = -1;
	public enMissionType missionType = enMissionType.FLIGHT;
	public float missionDifficulty = 5; //Out of 10? I dunno
	#endregion
	// Use this for initialization
	void Awake () {
		if (instance)
		{
			Debug.Log("Duplicate attempt to create gameManager");
			Debug.Log (gameObject.name);
			Destroy(this);
			return; //cancel this
		}
		
		instance = this;
		
		DontDestroyOnLoad(this); //this is about the only thing that's not cycled around.
	}
	
	int enemies, wingmen;
	
	public void reEnableMapInteraction()
    {
		bCanSelectMission = true;
    }

	public void setEnemyActivity(float newActivity) {
		enemyActivity = newActivity;	
	}

	//This is called from our panel accepting the mission so we've got to store our informatoin and boot off of that. It's minimal for now.

	public void StartMissionFromUI()
    {
		MakeMission(selectedTile);
    }

	public void MakeMission(int selectedTile) { //load a level we're going to play on
		currentlySelectedTile = selectedTile;
		enemies = Mathf.RoundToInt(enemyActivity*12F);
		wingmen = 2; //standard flight group.
		
		//Before doing this we need to "save the state" of our map

		///something something something map.
		StartCoroutine(loadMission()); //now boot up our mission function stuff
	}

	public void SetLoadingScreen(string loadingMessage, float amount, bool bIsVisible)
    {
		LoadingScreenBase.SetActive(bIsVisible);
		loadingScreenMessage.text = loadingMessage;
		LoadingSlider.value = amount;
    }

	public IEnumerator loadScene(string SceneName)
    {
		//This extra block of code handles the loading bar and loading screen
		AsyncOperation async = Application.LoadLevelAsync(SceneName); //PROBLEM: Need better level loading logic here
		loadingScreenMessage.text = "Loading...";
		LoadingScreenBase.SetActive(true);
		while (!async.isDone)
		{
			LoadingSlider.value = async.progress;
			//Debug.Log(LoadingSlider.value);
			if (async.progress >= 0.9f)
			{
				LoadingSlider.value = 1f;
				async.allowSceneActivation = true;
			}
			yield return null;
		}
		yield return async;
		LoadingScreenBase.SetActive(false);
		Debug.Log("Loading complete");
	}
	
	IEnumerator loadMission() {

		//load our level
		UIMusicHandler.Instance.SetMusicTrack(false);   //Set our combat track playing
		if (missionType == enMissionType.FLIGHT || missionType == enMissionType.BASEDEFENCE || missionType == enMissionType.BASEATTACK || missionType == enMissionType.SKIRMISH)
		{
			yield return StartCoroutine(loadScene("Level"));
		} else if (missionType == enMissionType.TURRET)
        {
			yield return StartCoroutine(loadScene("TailgunnerSetupScene"));
		} else if (missionType == enMissionType.BOMBING)
        {
			yield return StartCoroutine(loadScene("BombingRunMinigame"));
		} else if (missionType == enMissionType.STRAFING)
        {
			yield return StartCoroutine(loadScene("StrafingRun"));
        }
		
		yield return null;
		
		//seems to be getting trapped here for some reason
		while (LevelControllerBase.Instance == null)
			yield return null; //we can't setup our game just yet
		
		if (missionType == enMissionType.FLIGHT)
		{
			((LevelController)LevelControllerBase.Instance).createMatch(enemies, wingmen); //will do for the moment
		}
	}

	public void ConcludeMission()
    {
		StartCoroutine(doConcludeMission(currentlySelectedTile));
    }

	IEnumerator doConcludeMission(int targetTile)
	{
		Time.timeScale = 1f;
		/*
		//load our level
		AsyncOperation async = Application.LoadLevelAsync("MissionSelection"); //PROBLEM: Need better level loading logic here
		yield return async;
		*/
		UIMusicHandler.Instance.SetMusicTrack(true); //Set our menu music playing
		yield return StartCoroutine(loadScene("MissionSelection"));
		Debug.Log("Loading complete");

		yield return null;

		//seems to be getting trapped here for some reason
		while (Mission_MapManager.Instance == null)
		{
			yield return null; //we can't setup our game just yet
		}
		while (!Mission_MapManager.Instance.bMapLoaded)
        {
			yield return null;
        }
		//We need to report back to our system/map manager somehow. I don't know where this part of the game is centered...
		//PROBLEM: This is a hacky way to figure things out (and knowing my luck it'll stay, forever)
		Mission_MapManager.Instance.LevelCompleted(targetTile, levelResults.bWonLevel); //For the moment lets hard-code this
	}

	[HideInInspector]
	public bool bNeedsNewSave = false;
	public void StartNewGame()
    {
		bNeedsNewSave = true;
    }
}