using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using static Mission_MapSection;
using System.IO;

[System.Serializable]
public class AirframeDescription
{
	//I really don't know about any of these values
	public float weight_standard = 50f;
	public float weight_max = 75f;
	public float turnspeed_standard = 1.5f;
	public float turnspeed_max = 2.0f;
}


[System.Serializable]
public class AircraftDescription	//Basically this is all the stats that we need to make a player aircraft and will be used with the Hangar and also to shuffle data back and forth
{
	[Header("Upgrade Details")]
	//A limiting factor for what we can equip
	public float weight_max = 100f;
	public float weight_standard = 80f;
	public float weight_current = 100f;

	//Player health in another name
	public float armor_max = 150f;
	public float armor_standard = 100f;
	public float armor_current = 100f;

	//Agility. Don't know if this is correct
	public float agility_max = 2f;
	public float agility_standard = 1.5f;
	public float agility_current = 1.5f;


	//Speed. Don't know if this is correct
	public float speed_max = 8f;
	public float speed_standard = 6f;
	public float speed_current = 6f;

	//Accel. Don't know if this is correct
	public float accel_max = 65f;
	public float accel_standard = 50f;
	public float accel_current = 50f;


	//Start with the guns I guess
	[Space]
	[Header("Cannon Details")]
	public float cannons_damage = 1f;   //Damage per shot
	public float cannons_refire_time = 0.25f;	//This is per cannon
	public float cannons_bullet_speed = 50f;
	public float cannons_bullet_range = 100f;
	public float cannons_spread = 0.01f;
	public float cannons_autoaim_angle = 7f;
	[Space]
	[Header("Aircraft Details")]
	public string airframe_modelname = "";
	public AirframeDescription airframe = new AirframeDescription();	//This is our base definitions that'll be exposed in the settings for the menus and sets limits on weight etc.
	public float airframe_health = 100f;
	public float airframe_turnspeed = 1.5f; //A multiplier for the Time.deltaTime to make aircraft turn faster
	public float airframe_mass = 7; //This is a number that worked well with the systems. It has no real world counterpart
	public Color airframe_color_primary = Color.green;
	public Color airframe_color_secondary = Color.yellow;
	[Space]
	[Header("Engine Details")]
	public float engine_max_overdrive_speed = 10f;
	public float engine_max_airspeed = 6;
	public float engine_min_airspeed = 2;
	public float engine_stall_speed = 1.5f;
	public float engine_thrust_power = 30f; //How quickly we can accellerate
	[Space]
	[Header("Secondary Weapons")]
	public string secondary_weapon_name = "";


}

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
	
	public enum enGameState { NULL, MENU, LEVELSETUP, LEVELPLAYING, LEVELENDED, PAUSE }	//for VitaHOT I had states on both the LevelController and gameManager, so we're going to try and put everything here
	public enGameState GameState = enGameState.NULL;

	public int turnNumber = -1;	//Default unpopulated
	public void setGameState(enGameState newState)
    {
		GameState = newState;
    }

	float enemyActivity=0;
	int currentlySelectedTile = -1;
	//need some way of knowing which level we'd want to load

	#region Selected Aircraft Details
	public AircraftDescription SelectedAircraft;
    #endregion

    #region Level Results to Pass Through
    public LevelResults levelResults;
	#endregion

	#region Level Load Screen
	public TextMeshProUGUI loadingScreenMessage;
    public GameObject LoadingScreenBase;
	public Slider LoadingSlider;
	public TextMeshProUGUI loadingScreenTitle;
	public TextMeshProUGUI loadingScreenDescription;
	public TextMeshProUGUI loadingScreenHint;
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

	#region Communication Text Sections
	//This is about the worst way to do this
	public string panelTitle = "";
	public string panelContent = "";
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
		loadingScreenTitle.text = panelTitle;
		loadingScreenDescription.text = panelContent;

		LoadingScreenBase.SetActive(bIsVisible);
		loadingScreenMessage.text = loadingMessage;
		LoadingSlider.value = amount;
    }

	public IEnumerator loadScene(string SceneName)
    {
		loadingScreenTitle.text = panelTitle;
		loadingScreenDescription.text = panelContent;
		loadingScreenHint.text = getHintString();

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

	string getHintString()
	{
		string[] splitFile = new string[] { "\r\n", "\r", "\n" };
		char[] splitLine = new char[] { ',' };
		// name
		string full_path = string.Format("{0}/{1}", Application.streamingAssetsPath, "\\LoadingScreenHints.txt");
		StreamReader reader = new StreamReader(full_path);
		string LoadingHintsText = reader.ReadToEnd().Trim();
		reader.Close();

		//TextAsset LoadingHintsFile = Resources.Load("LoadingScreenHints") as TextAsset;
		//Debug.Log("LoadingScreenFileText: " + LoadingHintsText);
		string[] NameLines = LoadingHintsText.Split(splitFile, System.StringSplitOptions.None);
		//Debug.Log("Name Lines Length: " + NameLines.Length);
		return NameLines[(int)Random.Range(0, NameLines.Length)];
	}

	GameObject pauseMenu;

	void LateUpdate()
    {
        #region Pause Menu Functionality
		//This needs to be on late update to prevent interference with the player speed control stuff
		if ((Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.M)|| Input.GetButtonDown("Start")) && GameState != enGameState.MENU)
        {
			if (UIMenuHandler.Instance)
			{
				Debug.Log("Loading Pause Menu");
				GameState = enGameState.PAUSE;
				Time.timeScale = 0.00001f;	//Set our pause timescale. I'm not sure if this is effective elsewhere
				UIMenuHandler.Instance.LoadMenuSceneAdditively("Menu_Pause", null, null);
			}
		}
        #endregion
    }
}