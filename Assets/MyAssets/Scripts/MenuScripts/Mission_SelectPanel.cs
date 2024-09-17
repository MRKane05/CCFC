using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//This is for confirming which mission we're going to fly, and then we hand over control to the setup panel
public class Mission_SelectPanel : MonoBehaviour {
	
	public Mission_MapManager ourMapManager;
	public Mission_SetupPanel ourSetupPanel; //this is what we hand off to
	
	public int selectedTile = -1;
	
	public Text missionDetails; //information about the missions
	
	
	float enemyActivity;
	
	// Use this for initialization
	void Start () {
	
	}

	public void selectThis(int thisMapEntry) {
		//I need to figure out where the mission is being constructed, and this should reflect that...
		gameObject.SetActive(true);
		selectedTile=thisMapEntry; //so we can pull the details, or they'll be provided here

		//almost need some sort of "post up" title thing here so we can choose what the end results for this tile are (that's the test bit)

		//bring up the details for the mission we've selected
		writeMissionBrief();	
	}
	
	void writeMissionBrief() {
		
		mapArrayEntry mapDetail  = ourMapManager.mapArray[selectedTile];
		/*
		enemyActivity = mapDetail.enemyActivity;
		Debug.Log("Enemy Activity: " + Mathf.RoundToInt(enemyActivity * 100F) + "%");
		missionDetails.text = "Enemy Activity: " + Mathf.RoundToInt(enemyActivity*100F) + "%";
		*/
	}
	
	//called from the buttons, we're not flying this mission
	public void cancel() { 
		gameObject.SetActive(false); //just turn it off for the moment
	}
	
	public void acceptMission() {
		gameManager.Instance.setEnemyActivity(enemyActivity);
		//We don't have this bit done yet, lets just get to it
		//ourSetupPanel.gameObject.SetActive(true);
		gameObject.SetActive(false);

		//Just hard-fire our mission off to go
		//We need some concept of which tile we're operating on to pass the information back and forth
		gameManager.Instance.MakeMission(selectedTile);
	}

	// Update is called once per frame
	void Update () {
	
	}
}
