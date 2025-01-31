using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AckAckZoneSpawner : MonoBehaviour {
	public Range LimitX = new Range(-46, 46);
	public Range LimitY = new Range(-15, 30);
	public GameObject AckAckZonePrefab;
	public Range SpawnFrenquency = new Range(0.5f, 3f); //How frequently do we drop in an Ack Ack?
	float nextAckAckTime = 0;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.time > nextAckAckTime)
        {
			SpawnAckAck();
        }
	}

	void SpawnAckAck() {
		GameObject newAckAck = Instantiate(AckAckZonePrefab) as GameObject;
		newAckAck.transform.position = new Vector3(LimitX.GetRandom(), 50, LimitY.GetRandom());
		Destroy(newAckAck, Random.Range(2, 5));	//And our kill function for the moment
		nextAckAckTime = Time.time + SpawnFrenquency.GetRandom();

	}
}
