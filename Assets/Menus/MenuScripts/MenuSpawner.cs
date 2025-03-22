using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script simply spawns in a menu, and is used for the likes of the first menu the player hits. I'm sure it'll be expanded.

public class MenuSpawner : MonoBehaviour {
	public enum enMenuSpawnBehavior {  NULL, ONSTART }
	public enMenuSpawnBehavior MenuSpawnBehavior = enMenuSpawnBehavior.ONSTART;
	public string MenuToSpawn = "";
	// Use this for initialization
	void Start () {
		if (MenuSpawnBehavior == enMenuSpawnBehavior.ONSTART)
        {
			UIMenuHandler.Instance.LoadMenuSceneAdditively(MenuToSpawn, null, null);
		}
	}
}
