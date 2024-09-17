using UnityEngine;
using System.Collections;

//this class will download our language file and populate lists
public class LanguageManager : MonoBehaviour {
	private static LanguageManager instance = null;
	public static LanguageManager Instance {get {return instance;}}
	
	public string[] exp_Small = new string[] {"POP"} , exp_Medium = new string[] {"BOOM"} , exp_Large = new string[] {"KA-BOOM!"} ;

	void Awake() {
		if (instance)
		{
			Debug.Log("Duplicate attempt to create LanguageManager");
			Destroy(this);
			return;
		}
		
		instance = this;
	}

	//return a string based off the power that we call for here (0-1)
	public string returnExplosion(float power) {
		//pick our array
		if (power <= 1f/3f) {
			return exp_Small[Mathf.FloorToInt(Random.Range(0f, 1f)*exp_Small.Length)];
		} else if (power >= 2f/3f) {
			return exp_Large[Mathf.FloorToInt(Random.Range(0f, 1f)*exp_Large.Length)];
		} else {
			return exp_Medium[Mathf.FloorToInt(Random.Range(0f, 1f)*exp_Medium.Length)];
		}

	}

}
