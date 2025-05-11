using UnityEngine;
using System.Collections;

//Our GUI base class which is sent useful information by the player controller
//This is getting a little messy, but I still prefer the universal approach
public class NGUI_Base : MonoBehaviour {
	//This needs to be instanced to make things easier
	private static NGUI_Base instance = null;
	public static NGUI_Base Instance {get {return instance;}}
	
	public NGUI_HealthBar ourHealthBar;
	public UI_SecondaryAmmoCounter secondaryAmmoCounter;
	public Player_Radar ourPlayerRadar;
	public NGUI_ObjectTracker ourTargetSight; //will we want these to be customizable?
	public NGUI_ObjectTracker waypointGUI; //can also be used for the waypoint stuff

	public NGUI_ObjectTracker targetDistanceMarker;

	public NGUI_GameMessage ourGameMessage;
	public NGUI_AIMessage ourPortraitMessage;

	//this will need modified
	public NGUI_firingTracker targetFiringMarker; //what we should shoot at with our target
	//public GUI_Speedo speedo;
	public TargetCamera_View ourTargetCamera;



	AudioSource ourAudio;

	// Use this for initialization
	IEnumerator Start () {
		if (instance)
		{
			Debug.Log("Duplicate attempt to create NGUI_Base");
			Destroy(this);
		}
		ourAudio = gameObject.GetComponent<AudioSource>();
		
		instance = this;
		
		while (PlayerController.Instance==null)
			yield return null;
		
		//Wait to setup the next bits.
		
		ourPlayerRadar.playerAircraft = PlayerController.Instance.ourAircraft.gameObject; //assign the vehicle
		
		ourTargetSight.trackObject = PlayerController.Instance.ourAircraft.GetComponent<Actor>().gunSight; //another messy!
	}

	public void setGameMessage(string newMessage) {
		ourGameMessage.setMessage(newMessage);
	}

	public void setPortraitMessage(string newName, string newText, Color teamColor)
    {
		ourPortraitMessage.setMessage(newName, newText, teamColor);
	}

	public void assignWaypoint(GameObject thisWaypoint) {
		Debug.Log ("Adding Waypoint");
		if (waypointGUI) {
			waypointGUI.trackObject = thisWaypoint;
		}
	}
	
	public void assignWaypoint(waypointWrapper thisWaypoint) {
		Debug.Log ("Adding Waypoint");
		if (waypointGUI) {
			waypointGUI.trackObject = thisWaypoint.waypoint; //assign this to the thing so that it'll display	
		}
	}

	public void assignHealth(float newProp) {
		if (ourHealthBar) {
			ourHealthBar.setHealthProp(newProp); //feed this through to the system
		}
	}

	public void setSecondaryAmmoCount(float newCount, bool bIsVisible)
	{
		if (secondaryAmmoCounter) {
			if (bIsVisible)
			{
				secondaryAmmoCounter.setAmmoCount(newCount);
			} else
            {
				secondaryAmmoCounter.gameObject.SetActive(false);	//Disable this counter
            }
		}
	}

	//commands need to go through here to the player controller

	//we want to target our "best" target.
	#region targeting
	public void targetBest(bool bDirectionMatters) {
		//This needs to be sent through to the players systems having elected a best target from the levelController
		((LevelController)LevelControllerBase.Instance).requestTarget(PlayerController.Instance, bDirectionMatters); //should callback the controller with the target

	}

    public void nextTarget()
    {
		if (PlayerController.Instance.target!=null)
			((LevelController)LevelControllerBase.Instance).requestListTarget(1, PlayerController.Instance);
		else
			((LevelController)LevelControllerBase.Instance).requestTarget(PlayerController.Instance, false);
	}

	public void previousTarget() { //Cycle this stuff backwards
		if (PlayerController.Instance.target!=null)
			((LevelController)LevelControllerBase.Instance).requestListTarget(-1, PlayerController.Instance);
		else
			((LevelController)LevelControllerBase.Instance).requestTarget(PlayerController.Instance, false);
	}

	public void objectiveTarget() {

	}

	public void attackingTarget() {

	}



	//this is being pinged twice?
	public void setTarget(GameObject newTarget) {
		if (ourTargetCamera)
		{
			ourTargetCamera.setTarget(newTarget); //pass this on also

			//we need to get our target's targetSphere for this
			//GameObject targetSphere = newTarget.GetComponent<TargetSphereOffset>().targetSphere;
			//Debug.Log (newTarget);
			targetFiringMarker.trackObject = newTarget;
			targetDistanceMarker.trackObject = newTarget; //Ok, so this won't be so simple and will have to be forward calculated...
			ourAudio.PlayOneShot(ourAudio.clip);
		}
	}

	#endregion
	
	// Update is called once per frame
	void Update () {
		//handle ongoing things like the speedo, and altimeter
		//handleSpeedo();
	}
	/*
	void handleSpeedo() {
		if (speedo && PlayerController.Instance) {
			//speedo.transform.eulerAngles = new Vector3(0, 0, 160-PlayerController.Instance.ourAircraft.getSpeed()*70); //Not the best way to do things
			speedo.speed = PlayerController.Instance.ourAircraft.getSpeed();
		}
	}*/


}
