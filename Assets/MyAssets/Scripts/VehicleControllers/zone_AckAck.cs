using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//essentually AckAck is fired in this zone.
public class zone_AckAck : MonoBehaviour {

	float lastTriggerTime =0;
	float reTriggerTime = 0.5f;	//This will target our player
	float volumeTriggerTime = 1f;	//This will go "around" our player for effect

	public GameObject ackAckPrefab;
	public float ZoneWidth = 50f;	//How big is the zone of the Ack Ack (this will be set on setup)
	public float EffectZoneSize = 30f; //How bid of a zone do we have for Ack Ack effects firing in front of the player?
	public float MinimumHeight = 10f;
	public Range LeadRange = new Range(2f, 7f);

	public int Team = 0;

	public List<GameObject> EffectPrefabs = new List<GameObject>(); //The idea here is that we'll recycle these to prevent memory issues

	bool bIsPositionInZone(Vector3 position)
    {
		position = gameObject.transform.InverseTransformPoint(position);
		if (new Vector2(position.x, position.z).sqrMagnitude > ZoneWidth*ZoneWidth)
        {
			return false;
        }
		return true;
    }
	
	List<actorWrapper> ActorsInZone(int team)
    {
		List<actorWrapper> actorList = new List<actorWrapper>();
		if (team == 0)	//This is an enemy zone
        {
			foreach (actorWrapper thisActor in ((LevelController)LevelControllerBase.Instance).friendlyList)
            {
				if (bIsPositionInZone(thisActor.vehicle.transform.position))
                {
					actorList.Add(thisActor);
                }
            }
        }

		return actorList;
    }

	// have the Ack Ack seek the enemy fighters to some degree...which now means that this
	//thing needs a team and class extension etc.
	//It needs to be EXCITING!
	void Update()
	{
		//We need to check if something we want to shoot at is in this zone
		//We need functions to handle the size of the zone
		//We might need something to look at targets and "cluster" according to what we're shooting at
		//Could gameify the flak even further by having a small "brew point" before it explodes, giving the player the opportunity to dodge

		if (Time.time - reTriggerTime > lastTriggerTime)
		{
			//make an Ack Ack!
			lastTriggerTime = Time.time;

			//Vector3 spawnPoint = transform.position + new Vector3(ZoneWidth.GetRandom(), ZoneHeight.GetRandom(), ZoneWidth.GetRandom());
			List<actorWrapper> ZoneActors = ActorsInZone(0);
			if (ZoneActors.Count > 0)
			{
				//Pick an actor and make an explosion ahead of them
				int TargetActor = Random.Range(0, ZoneActors.Count);

				Vector3 targetPosition = ZoneActors[TargetActor].vehicle.gameObject.transform.position + ZoneActors[TargetActor].vehicle.gameObject.transform.forward * LeadRange.GetRandom();
				//We could do with a random on the Ack Ack
				targetPosition += new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
				GameObject newAckAck = GetAckAckEffect(); //Instantiate(ackAckPrefab, targetPosition, Quaternion.identity);
				newAckAck.transform.position = targetPosition;
				newAckAck.SetActive(true); //Turn this effect on
				AckAckExplosion newAckAckScript = newAckAck.GetComponent<AckAckExplosion>();
				newAckAckScript.SetupAckAck(targetPosition, Random.Range(0.5f, 1.2f), Team);

			}
		}

		if (Time.time - reTriggerTime > volumeTriggerTime && PlayerController.Instance != null)
		{
			//Make a "background" Ack Ack
			volumeTriggerTime = Time.time;
			//This is only worthwhile if our player sees it, so lets see about making a pattern that'll put it within a forward zone of the player
			//Of course the clever player could use this as an offensive weapon, but Ack Ack is known for that
			Vector3 targetPosition = PlayerController.Instance.transform.position + PlayerController.Instance.transform.forward * EffectZoneSize;
			targetPosition += new Vector3(Random.Range(-EffectZoneSize, EffectZoneSize), Random.Range(-EffectZoneSize, EffectZoneSize), Random.Range(-EffectZoneSize, EffectZoneSize));
			//We need something to check and see if this point is under the terrain
			if (targetPosition.z > MinimumHeight) //We can do a bang here
            {
				GameObject newAckAck = GetAckAckEffect(); //Instantiate(ackAckPrefab, targetPosition, Quaternion.identity);
				newAckAck.transform.position = targetPosition;
				newAckAck.SetActive(true); //Turn this effect on
				AckAckExplosion newAckAckScript = newAckAck.GetComponent<AckAckExplosion>();
				newAckAckScript.SetupAckAck(targetPosition, Random.Range(0.5f, 1.2f), Team);
			}
		}
	}

	GameObject GetAckAckEffect()
    {
		bool bHasFoundEffect = false;
		GameObject newAckAck = null;
		foreach (GameObject thisEffect in EffectPrefabs)
        {
			if (!bHasFoundEffect)
			{
				if (!thisEffect.GetComponent<AckAckExplosion>().bIsActive)
				{
					bHasFoundEffect = true;
					newAckAck = thisEffect;
				}
			}
        }
		if (!bHasFoundEffect)
		{
			newAckAck = Instantiate(ackAckPrefab) as GameObject;
			EffectPrefabs.Add(newAckAck);
		}

		return newAckAck;
	}
}
