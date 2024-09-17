using UnityEngine;
using System.Collections;

//essentually AckAck is fired in this zone.
public class zone_AckAck : MonoBehaviour {

	float lastTriggerTime =0;
	float reTriggerTime = 0.5f;

	public GameObject ackAckPrefab;

	// have the Ack Ack seek the enemy fighters to some degree...which now means that this
	//thing needs a team and class extension etc.
	//It needs to be EXCITING!
	void Update () {
		if (Time.time - reTriggerTime > lastTriggerTime) {
			//make an Ack Ack!
			lastTriggerTime = Time.time;

			Vector3 spawnPoint = transform.position + new Vector3(Random.Range(-10, 10), Random.Range (-5, 5), Random.Range(-10, 10));
		
			Instantiate(ackAckPrefab, spawnPoint, Quaternion.identity);
		}
	}
}
