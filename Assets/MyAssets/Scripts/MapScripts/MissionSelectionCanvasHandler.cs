using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerAircraftCollection
{
	public int currentAircraft = 0;
	public List<AircraftDescription> Aircraft = new List<AircraftDescription>();
}

//This is the handler for our aircraft information, and everything that our player could potentially be flying
public class MissionSelectionCanvasHandler : MonoBehaviour {
	public enum enCurrentCanvas { NULL, MAP, HANGAR }
	public enCurrentCanvas CurrentCanvas = enCurrentCanvas.NULL;

	public PlayerAircraftCollection playerVehicles = new PlayerAircraftCollection();
	public AircraftDescription DefaultAircraft = new AircraftDescription();

	public GameObject mapObject;
	public GameObject hangarObject;
	public GameObject hangarStartButton;

	//and of course a list of aircraft that the player could fly will need to be built and cached somewhere

	// Use this for initialization
	void Start () {
		LoadPlayerVehicles();
	}

	public void SelectMainCanvas(enCurrentCanvas NewCanvas)
    {
		switch (NewCanvas)
        {
			case enCurrentCanvas.MAP:
				mapObject.SetActive(true);
				break;
			case enCurrentCanvas.HANGAR:
				hangarObject.SetActive(true);
				UIHelpers.SetSelectedButton(hangarStartButton);
				break;
        }
    }

	public void OpenHangar()
    {
		mapObject.SetActive(false);
		SelectMainCanvas(enCurrentCanvas.HANGAR);
    }

	public void HangarClose()
    {
		//Kind of important that we save our settings here, and that'll be done through the game controller
		if (gameManager.Instance)
        {
			gameManager.Instance.DoHangarClose();
		}
		SavePlayerVehicles();

		hangarObject.SetActive(false);
		//At the moment we're only going back to our map
		SelectMainCanvas(enCurrentCanvas.MAP);
	}

	void SavePlayerVehicles()
    {
		//Really we need to get the one from the active game controller also...
		playerVehicles.Aircraft[playerVehicles.currentAircraft] = gameManager.Instance.SelectedAircraft;
		string playerAircraftData = JsonUtility.ToJson(playerVehicles);
		SaveUtility.Instance.SaveTXTFile(playerAircraftData, "playerAircraftData.json");
	}

	void LoadPlayerVehicles()
    {
		if (SaveUtility.Instance.CheckSaveFile("playerAircraftData.json"))
		{
			string saveText = SaveUtility.Instance.LoadTXTFile("playerAircraftData.json");
			PlayerAircraftCollection ourSaveForm = new PlayerAircraftCollection();
			playerVehicles = JsonUtility.FromJson<PlayerAircraftCollection>(saveText);

			//Now we need to update our game controller
			gameManager.Instance.SelectedAircraft = playerVehicles.Aircraft[playerVehicles.currentAircraft];
		} else
        {
			//We need to make a new starting state save form
			playerVehicles = new PlayerAircraftCollection();
			playerVehicles.Aircraft.Add(DefaultAircraft);
        }
	}
}
