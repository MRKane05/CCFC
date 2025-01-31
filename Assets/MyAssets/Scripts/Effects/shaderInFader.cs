using UnityEngine;
using System.Collections;

//Used on every material we want to fade in for 
public class shaderInFader : MonoBehaviour {
	Material mat;
	Shader matShader;
	public Shader fadeShader;
	Actor actorClass;

	Material inFadeMat;
	Color matCol, lineCol;

	//Hopefully this will get it before the start tick
	void Awake () {
		if (fadeShader==null) { //if we don't have one assigned then find our default
			fadeShader=Shader.Find ("CCFC/Lighted Outline Transparent");
		}

		actorClass = CCFCTools.FindInParents<Actor>(gameObject.transform);

		if (actorClass!=null) {
			mat = gameObject.GetComponent<Renderer>().sharedMaterial; //hopefully we won't ever have more than one...
			matShader = mat.shader;
			//For the duration that we're fading in we'll need a new material.
			gameObject.GetComponent<Renderer>().material = Instantiate(mat) as Material;
			inFadeMat = gameObject.GetComponent<Renderer>().sharedMaterial;
			inFadeMat.shader = fadeShader;
			matCol = mat.GetColor("_Color");
			lineCol = mat.GetColor ("_OutlineColor");
		}
	}

	void Update () {
		if (actorClass.inFade<1f) {
			matCol.a = Mathf.Clamp01(actorClass.inFade);
			lineCol.a = Mathf.Clamp01(actorClass.inFade);
			inFadeMat.SetColor("_Color", matCol);
			inFadeMat.SetColor ("_OutlineColor", lineCol);
			//Debug.Log ("Fade: " + actorClass.inFade);
		} else { //and we only want to do this once...or do we?
			if (gameObject.GetComponent<Renderer>().sharedMaterial!=mat) {
				gameObject.GetComponent<Renderer>().sharedMaterial = mat; //set that back.
			}
		}
	}
}
