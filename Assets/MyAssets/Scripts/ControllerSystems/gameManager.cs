using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using static Mission_MapSection;
using System.IO;
using System.Collections.Generic;

[System.Serializable]
public class UpgradePathEffects
{
	//This needs to control how our upgrade affects the different stats of our aircraft
	//float levels = 5; //lets make this standard
	public float upgradeLevel = 0f;	//What's our current level?
	public float costPerLevel = 10f;	//I don't know, this'll be something open to balance
	public float finalWeight = 0f;		//How much weight will we add with the complete upgrade?
	public float finalArmor = 0f;		//How much armor will this upgrade add in final
	public float finalAgility = 0f;	//How much agility will this add in final
	public float finalSpeed = 0f;		//How much speed will this add in final
	public float finalAccel = 0f;		//How much acceleration will this add in final?

	public UpgradePathEffects(float cost, float weight, float armor, float agility, float speed, float accel)
    {
		costPerLevel = cost;
		finalWeight = weight;
		finalArmor = armor;
		finalAgility = agility;
		finalSpeed = speed;
		finalAccel = accel;
    } 
}


[System.Serializable]
public class CannonsItem
{
	public string cannons_name = "standard_cannons";

	public float unlock_cost = 200f;
	public float apply_cost = 25f;
	public float downgrade_cost = 5f;
	public float cannons_weight = 25f;

	public float cannons_damage = 1f;   //Damage per shot
	public float cannons_refire_time = 0.25f;   //This is per cannon
	public float cannons_bullet_speed = 50f;
	public float cannons_bullet_range = 100f;
	public float cannons_spread = 0.01f;
	public float cannons_autoaim_angle = 7f;

	public CannonsItem(string newName, float newUnlockCost, float newApplyCost, float newCannonsWeight, float newDamage, float newRefireTime, float newBulletSpeed, float newRange, float newSpread, float newAutoaim)
    {
		cannons_name = newName;
		unlock_cost = newUnlockCost;
		apply_cost = newApplyCost;
		cannons_weight = newCannonsWeight;

		cannons_damage = newDamage;
		cannons_refire_time = newRefireTime;
		cannons_bullet_speed = newBulletSpeed;
		cannons_bullet_range = newRange;
		cannons_spread = newSpread;
		cannons_autoaim_angle = newAutoaim;
    }
}


[System.Serializable]
public class AircraftDescription	//Basically this is all the stats that we need to make a player aircraft and will be used with the Hangar and also to shuffle data back and forth
{
	public string aircraft_model = "camel";

	[Header("Upgrade Details")]
	//A limiting factor for what we can equip
	public float weight_max = 100f;
	public float weight_standard = 80f;
	public float weight_current = 100f;
	[Space]
	//Player health in another name
	public float armor_max = 150f;
	public float armor_standard = 100f;
	public float armor_current = 100f;
	[Space]
	//Agility. Don't know if this is correct
	public float agility_max = 2f;
	public float agility_standard = 1.5f;
	public float agility_current = 1.5f;
	[Space]
	//Speed. Don't know if this is correct
	public float speed_max = 8f;
	public float speed_standard = 6f;
	public float speed_current = 6f;
	[Space]
	//Accel. Don't know if this is correct
	public float accel_max = 65f;
	public float accel_standard = 50f;
	public float accel_current = 50f;

	[Header("Upgrade Linear Sections")]
	public UpgradePathEffects upgrade_airframe = new UpgradePathEffects(10f, 5f, 0f, 0.5f, 0f, 0f);
	public UpgradePathEffects upgrade_engine = new UpgradePathEffects(15f, 10f, 0f, -0.2f, 2f, 0f);
	public UpgradePathEffects upgrade_armor = new UpgradePathEffects(10f, 15f, 50f, -0.2f, -1.5f, 0f);

	//Start with the guns I guess
	[Space]
	[Header("Cannon Details")]
	//I don't know if this should be stored here, or in another part of the system and referred to
	//I don't know if this should be stored here, or in another part of the system and referred to
	public CannonsItem AttachedCannons = new CannonsItem("standard_cannons", 200f, 25f, 25f, 1f, 0.25f, 50f, 100f, 0.01f, 7f);

	/*
	//We kind of don't need this
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
	*/
	[Space]
	[Header("Secondary Weapons")]
	public string secondary_weapon_name = "";
	[Space]
	[Header("Special Ability")]
	public string special_ability_name = "";

}

[System.Serializable]
public class LevelResults
{
	public bool bWonLevel = true;
}

[System.Serializable]
public class PlayerGameStats
{
	public float money = 30;	//Because calling it "requsition points" might only be a surface level thing
}


[System.Serializable]
public class LevelScoreItem
{
	//public string itemName = "";
	public float itemPoints = 1;
	public int count = 0;

	public LevelScoreItem(float newPoints, int newCount)
	{
		//itemName = newName;	//We probably don't need to be storing this twice
		itemPoints = newPoints;
		count = newCount;
	}
}

//Some way of keeping track of the players score. I guess
[System.Serializable]
public class PlayerLevelScore
{
	public Dictionary<string, LevelScoreItem> playerLevelScore = new Dictionary<string, LevelScoreItem>();

	public void AddScoreItem(string itemName, float itemPoints) //Called when we get a kill
	{
		if (playerLevelScore.ContainsKey(itemName))
		{
			playerLevelScore[itemName].count++;
		}
		else
		{
			LevelScoreItem newScoreItem = new LevelScoreItem(itemPoints, 1);
			playerLevelScore.Add(itemName, newScoreItem);
		}
	}
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
	//We need something that contains a description of all the aircraft we're working with. This shouldn't take up too much memory, and I'm not sure
	//where it should live...
	#endregion

	#region player statistics and other data
	public PlayerLevelScore PlayerScore = new PlayerLevelScore(); //our stats for this level
	public PlayerGameStats playerStats;
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
		loadPlayerData();	//Load the data for our player
	}

	public void loadPlayerData()
	{
		if (SaveUtility.Instance.CheckSaveFile("playerData.json"))
		{
			string saveText = SaveUtility.Instance.LoadTXTFile("playerData.json");
			PlayerGameStats ourSaveForm = new PlayerGameStats();
			playerStats = JsonUtility.FromJson<PlayerGameStats>(saveText);
		}
	}

	public void savePlayerData()
    {
		string playerSaveState = JsonUtility.ToJson(playerStats);
		SaveUtility.Instance.SaveTXTFile(playerSaveState, "playerData.json");
	}

	public void DoHangarClose()
    {
		savePlayerData();
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


	#region scoreKeeping
	public void addKill(string itemName, float itemPoints)
	{
		//levelPlayerStats.kills++;
		Debug.LogError("Adding Kill: " + itemName + ", " + itemPoints);
		PlayerScore.AddScoreItem(itemName, itemPoints);
	}

	#endregion
}