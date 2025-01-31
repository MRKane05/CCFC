using UnityEngine;
using System.Collections;

public class RadarItem : MonoBehaviour {
	
	//Basically there are two things that this could be, a blip or an item
	public GameObject blipIcon, blipBrackets, spriteIcon, targetBrackets;

	private bool bIsTarget=false;

	void Start() {
		targetBrackets.SetActive(false); //for the moment
	}

	public void OnRadar(bool isOn) {
		blipBrackets.SetActive (!isOn && bIsTarget);
		blipIcon.SetActive(!isOn);

		spriteIcon.SetActive(isOn);
		targetBrackets.SetActive(isOn && bIsTarget);
	}

	//need some way of checking if we're being targeted and if so set the target brackets on.
	public bool isTarget {
		set { bIsTarget = value; }
	}
}
