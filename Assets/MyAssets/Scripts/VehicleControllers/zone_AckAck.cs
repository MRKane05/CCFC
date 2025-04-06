using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//essentually AckAck is fired in this zone.
public class zone_AckAck : MonoBehaviour {

	float lastTriggerTime =0;
	float reTriggerTime = 0.5f;

	public GameObject ackAckPrefab;
	public float ZoneWidth = 20f;
	public Range ZoneHeight = new Range(-5, 5);
	public Range LeadRange = new Range(2f, 7f);

	public int Team = 0;

	bool bIsPositionInZone(Vector3 position)
    {
		position = gameObject.transform.InverseTransformPoint(position);
		if (new Vector2(position.x, position.z).magnitude > ZoneWidth)
        {
			return false;
        }
		if (position.y < ZoneHeight.Min || position.y > ZoneHeight.Max)
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
	void Update () {
		//We need to check if something we want to shoot at is in this zone
		//We need functions to handle the size of the zone
		//We might need something to look at targets and "cluster" according to what we're shooting at
		//Could gameify the flak even further by having a small "brew point" before it explodes, giving the player the opportunity to dodge

		if (Time.time - reTriggerTime > lastTriggerTime) {
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
				Instantiate(ackAckPrefab, targetPosition, Quaternion.identity);
			}

		
			//Instantiate(ackAckPrefab, spawnPoint, Quaternion.identity);
		}
	}
}
