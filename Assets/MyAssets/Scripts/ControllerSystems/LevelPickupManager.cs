using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//So our team AI should be able to get a pickup (yay...another thing to programme in...) which means we need something universal, and it feels wrong for it
//to be on the Level Controller as these pickups will vary depending on the game mode with the skirmish gametypes being something that's collected
//tailgunner being something that's shot, and bombing etc.

[System.Serializable]
public class PickupObject
{
	public float spawnProbability = 0.5f;
	public GameObject pickupPrefab;
}


public class LevelPickupManager : MonoBehaviour {

	private static LevelPickupManager instance = null;
	public static LevelPickupManager Instance { get { return instance; } }

	public List<PickupObject> PickupPrefabs = new List<PickupObject>();

	public GameObject PhotoPositionPrefab;


	void Awake()
	{
		if (instance)
		{
			Debug.Log("Duplicate attempt to create LevelPickupManager");
			Destroy(this);
			return;
		}

		instance = this;
	}

	GameObject getRandomPickup()
    {
		return PickupPrefabs[Random.Range(0, PickupPrefabs.Count)].pickupPrefab;

	}

	public void SpawnPickup(GameObject instigator)
    {
		GameObject newPickup = Instantiate(getRandomPickup(), instigator.transform.position, Quaternion.identity) as GameObject;
		PickupBase newPickupScript = newPickup.GetComponent<PickupBase>();
		newPickupScript.DoPickupStart(instigator);
    }
}
