using UnityEngine;
using System.Collections;

public class GUI_Speedo : MonoBehaviour {

	public float mSpeed; //7.2 is the current models maximum
	public float speed {
		get { return speed; }
		set { mSpeed = value; }
	}

	public float speedHighBase = 5f, speedHighMax = 8f; //when does our needle start to misbehave?

	public GameObject arrowPivot;
	public float rotateFactor = 60f; //how much to rotate this arrow by in the dial.
	public float maximumPosition=280f; //just a number I suppose
	public float[] sinFrequencies;
	public float[] sinAmplitudes;

	//need something for adding in the factors that cause the speedo to twitch as our speed gets higher.
	
	float needleTwitch = 0f;
	void Update () {
		needleTwitch = 0f;
		for (int i=0; i<sinFrequencies.Length; i++) {
			needleTwitch += Mathf.Sin (Time.time*sinFrequencies[i])*sinAmplitudes[i];
		}

		needleTwitch *= Mathf.Clamp01((mSpeed-speedHighBase)/(speedHighMax-speedHighBase)); //so our needle will only twitch as we start going fast.

		arrowPivot.transform.eulerAngles = new Vector3(0, 0, Mathf.Clamp(mSpeed*rotateFactor+needleTwitch,-maximumPosition, 0));
		//arrowPivot.transform.eulerAngles = new Vector3(0, 0, mSpeed*rotateFactor+needleTwitch);

	}
}
