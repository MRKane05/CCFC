using UnityEngine;
using System.Collections;

public class Player_Radar : MonoBehaviour {
	//This could be quite expensive given that we're got to have various entries on our radar.
	//Or we could have the aircraft themselves handle their own entry.
	
	
	//We've got to radar the aircraft here because I'm pretty against having a visual display (although we might end up doing
	//something similar to that, will attempt this first)
	public GameObject playerAircraft; //our base aircraft
	public float radius=128F; //how far to the edges of the fields
	
	float sqrRadius;
	
	public float radarScale=0.1F;
	
	
	LevelController levelLink;
	
	// Use this for initialization
	//Register ourselves with the LevelController
	IEnumerator Start () {
		
		
		//Register this with the level controller
		while (((LevelController)LevelControllerBase.Instance) == null) {
			yield return null;
		}
		
		((LevelController)LevelControllerBase.Instance).setRadar(this);
		
		levelLink = ((LevelController)LevelControllerBase.Instance);
		
		yield return null; //ok, we're done
		
	}
	
	// Update is called once per frame
	void LateUpdate () {
		if (levelLink!=null && playerAircraft!=null)
			updateRadarItem();
	}

	void updateRadarItem() {
		sqrRadius = radius * radius;

		//detail our rotations
		transform.localEulerAngles = new Vector3(0, 0, playerAircraft.transform.eulerAngles[1] + 180F); //so our radar should rotate

		//Need to go through all the stuff on the LevelController and place them on our radar.
		if (levelLink.friendlyList.Count > 0) {
			foreach (actorWrapper thisActor in levelLink.friendlyList) {
				if (thisActor.radarObject) //No radar object...invisible!
					radarOffsetItem(thisActor);
			}
		}

		if (levelLink.enemyList.Count > 0) {
			foreach (actorWrapper thisActor in levelLink.enemyList) {
				if (thisActor.radarObject) //No radar object...invisible!
					radarOffsetItem(thisActor);
			}
		}

		//And now our points of interest/waypoints etc.
		if (levelLink.reconPointList.Count > 0)
        {
			foreach (actorWrapper thisActor in levelLink.reconPointList)
			{
				if (thisActor.radarObject) //No radar object...invisible!
					radarOffsetItem(thisActor);
			}
		}
		
		
	}
	
	
	void radarOffsetItem(actorWrapper thisActor) {
		//sqrRadius = radius*radius;
		
		Vector3 targetOffset = (playerAircraft.transform.position - thisActor.vehicle.transform.position);
		targetOffset *= radarScale;
		
		Vector2 targetFlat = new Vector2(targetOffset.x, targetOffset.z);
		//first off check to see if this actor has been shot down or disabled
		if (thisActor.actor.bIsDead) {
			//we shouldn't be displaying this item at all actually - it's out
			if (thisActor.radarLink)
				thisActor.radarLink.gameObject.SetActive(false);
				//Destroy(thisActor.radarLink.gameObject); //I think that'll do it.
		}
		else {

			thisActor.radarLink.isTarget = (thisActor.vehicle == PlayerController.Instance.target); //set radar targeting on/off

			if (targetFlat.sqrMagnitude > sqrRadius) { //we need to put this on the edge no the main
				targetFlat = targetFlat.normalized*radius; //put this to the edge
				thisActor.radarLink.OnRadar(false);
			}
			else
				thisActor.radarLink.OnRadar(true);

			//y is up. x is left. So for this moment I guess it's x and z.
			thisActor.radarObject.transform.localPosition = new Vector3 (targetFlat.x, targetFlat.y, 0); //annul our height
			thisActor.radarObject.transform.localEulerAngles = new Vector3(0, 0, -thisActor.vehicle.transform.eulerAngles[1]+180);
		}
	}
}
