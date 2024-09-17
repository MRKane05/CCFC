using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//There'll be lots going on here, mainly selecting the aircraft stuff (our actual aircraft engineering will be in a different panel)
public class Mission_SetupPanel : MonoBehaviour {
	
	public Slider wingmenSlider;
	public Mission_SelectPanel ourSelectPanel;
	
	
	
	//So this'll be different depending on what type of mission we're flying.
	//For the moment we'll setup for a basic patrol
	//Also plan on having a bombing mission (bridge busting, ship attack and buildings/structures), prision break/personalle rescue, base defense, 
	//straffing run (convoy, or similar), supply drops, intel collection, etc.
	
	//we're going to fly this mission
	public void selectMission() {

		//this needs to collect information required to make the mission stuff and make it happen
		//tell our gameController that we're all go with this mission and it's stats.
		//Move onto the loadout screen for the fighter, and also for selecting flight positions and wingmen etc.

		//Details will be sent to the mission maker, but for the moment we'll send through our enemy propensity stuff
		//gameManager.Instance.MakeMission(); //for the moment, but this isn't the best way of doing things
		//Now we need to junction onto the mission setup scene, which allows us to setup the fighter we pick, as well as wingmen etc.
		Debug.LogError("Function Disabled");
		gameObject.SetActive(false); //turn this off.
	}
	
	public void cancel() {
		ourSelectPanel.gameObject.SetActive(true);
		gameObject.SetActive(false);		
	}
}
