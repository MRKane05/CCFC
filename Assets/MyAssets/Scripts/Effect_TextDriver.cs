using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//Anything that explodes pretty much needs a text driver for doing this. This will handle that stuff. Not sure what else this could be used for...
public class Effect_TextDriver : MonoBehaviour {

	Text ourLabel;
	float lifeTime = 0.7f; //that'll do.
	float startTime;
	Vector3 startScale;

	public AnimationCurve scaleControl;

	public Color[] colors = new Color[] {Color.yellow, Color.red};

	float textTilt;

	// Use this for initialization
	void Start () {
		//when we're created...almost. We might be doing this prompted to save on instantiation in the future.
		ourLabel = GetComponentInChildren<Text>();
		startTime = Time.time;
		startScale = transform.localScale;

		PromptedStart();
	}


	float distanceGraduate = 20f; //guesswork.
	float proximity = 0f;

	public virtual void PromptedStart() {

		textTilt = Random.value*60f - 30f;

		//seeing as this is an explosion we should figure out how far it is to our player and graduate things accordingly...
		proximity = 1-Mathf.Clamp01((transform.position - PlayerController.Instance.ourAircraft.gameObject.transform.position).magnitude/distanceGraduate);
	
		ourLabel.text = LanguageManager.Instance.returnExplosion(proximity); //just make this random for the moment

	}
	
	// Update is called once per frame
	float lifeGrad = 0f;
	void Update () {
		lifeGrad = Mathf.Clamp((Time.time - startTime)/lifeTime, 0f, 0.9999f);

		float colorInterp = lifeGrad * (colors.Length-1);
		int colorFloor = Mathf.FloorToInt(colorInterp);
		colorInterp -= colorFloor;

		ourLabel.color = Color.Lerp (colors[colorFloor], colors[colorFloor+1], colorInterp);

		transform.LookAt(PlayerController.Instance.ourAircraft.gameObject.transform.position); //expensive?

		//finally we could do with some animation of scale I suppose
		transform.localScale = startScale * scaleControl.Evaluate((Time.time - startTime)/lifeTime);
		transform.localEulerAngles += Vector3.forward * textTilt;

		//this needs a time ticker on it.
		//ourLabel.gameObject.transform.localPosition = new Vector3((Random.value-0.5f)*proximity, (Random.value-0.5f)*proximity,(Random.value-0.5f)*proximity);
	}
}
