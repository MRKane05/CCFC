using UnityEngine;
using System.Collections;

//This class handles the prefabs for this scene, the idea being that we can request one when we want to make a
//gameobject
public class prefabManager : MonoBehaviour {
	private static prefabManager instance = null;
	public static prefabManager Instance {get {return instance;}}
	
	
	public GameObject playerPrefab;
	
	public GameObject[] enemyPrefabList, friendlyPrefabList;

	//now we need to break down our balloons and likewise...
	public GameObject[] enemyBalloons, friendlyBalloons;

	public GameObject[] enemyBombers, friendlyBombers;
	
	
	// Use this for initialization
	IEnumerator Start () {
		if (instance)
		{
			Debug.Log("Duplicate attempt to create prefabManager");
			Destroy(this);
			yield break;
		}
		
		instance = this;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	//Because the player prefabs will be custom
	public GameObject returnPlayerPrefab() { 
		return playerPrefab;
	}
	
	//Return prefabs of fighters, optional random range
	public GameObject getEnemyFighter(float minEntry, float maxEntry) {
		return enemyPrefabList[0];	
	}
	
	public GameObject getFriendlyFighter(float minEntry, float maxEntry) {
		return friendlyPrefabList[0];	
		
	}

	public GameObject getEnemyBalloon(float minEntry, float maxEntry) {
		return enemyBalloons[0];
	}
}
