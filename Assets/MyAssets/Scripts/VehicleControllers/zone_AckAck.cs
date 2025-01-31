using UnityEngine;
using System.Collections;

//essentually AckAck is fired in this zone.
public class zone_AckAck : MonoBehaviour {

	float lastTriggerTime =0;
	float reTriggerTime = 0.5f;

	public GameObject ackAckPrefab;
	public Range ZoneWidth = new Range(-10, 10);
	public Range ZoneHeight = new Range(-5, 5);

	// have the Ack Ack seek the enemy fighters to some degree...which now means that this
	//thing needs a team and class extension etc.
	//It needs to be EXCITING!
	void Update () {
		if (Time.time - reTriggerTime > lastTriggerTime) {
			//make an Ack Ack!
			lastTriggerTime = Time.time;

			Vector3 spawnPoint = transform.position + new Vector3(ZoneWidth.GetRandom(), ZoneHeight.GetRandom(), ZoneWidth.GetRandom());
		
			Instantiate(ackAckPrefab, spawnPoint, Quaternion.identity);
		}
	}
}
