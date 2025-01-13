using UnityEngine;
using System.Collections;

//This is the core of all the game mechanics, it handles the information transfer sections basically.
//Can't be too cluttered
public class gameManager : MonoBehaviour {
	
	private static gameManager instance = null;
	public static gameManager Instance {get {return instance;}}
	
	float enemyActivity=0;
	int currentlySelectedTile = -1;
	//need some way of knowing which level we'd want to load
	
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
	
	public void setEnemyActivity(float newActivity) {
		enemyActivity = newActivity;	
	}
	
	public void MakeMission(int selectedTile) { //load a level we're going to play on
		currentlySelectedTile = selectedTile;
		enemies = Mathf.RoundToInt(enemyActivity*12F);
		wingmen = 3; //standard flight group.
		
		//Before doing this we need to "save the state" of our map

		///something something something map.
		StartCoroutine(loadMission()); //now boot up our mission function stuff
	}
	
	IEnumerator loadMission() {
		//load our level
		AsyncOperation async = Application.LoadLevelAsync("Level"); //PROBLEM: Need better level loading logic here
		yield return async;
		Debug.Log ("Loading complete");
		
		yield return null;
		
		//seems to be getting trapped here for some reason
		while (LevelController.Instance == null)
			yield return null; //we can't setup our game just yet
		
		LevelController.Instance.createMatch(enemies, wingmen); //will do for the moment
	}

	public void ConcludeMission()
    {
		StartCoroutine(doConcludeMission(currentlySelectedTile));
    }

	IEnumerator doConcludeMission(int targetTile)
	{
		Time.timeScale = 1f;
		//load our level
		AsyncOperation async = Application.LoadLevelAsync("MissionSelection"); //PROBLEM: Need better level loading logic here
		yield return async;
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
		Mission_MapManager.Instance.LevelCompleted(targetTile, true); //For the moment lets hard-code this
	}
}