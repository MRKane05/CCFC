using UnityEngine;
using System.Collections;

//Used for the special behaviour that's on our target sights
public class GUI_TargetSights : MonoBehaviour {

	public Color aimNormal = Color.white, aimHit = Color.red;
	public float hitScale = 0.9f; //how much do we scale when we're aiming for a hit?
	public float closeAngle = 6f; //what's the edge of our aim?
	UnityEngine.UI.Image sightImage;

	// Use this for initialization
	void Start () {
		sightImage = gameObject.GetComponent<UnityEngine.UI.Image>();
	}

	//Not sure how to do this properly...
	public void setTargetAim(float aimDegree) {

	}

	float targetAngle=180f;
	float targetFraction = 1f;

	// Update is called once per frame
	void Update () {
		if (PlayerController.Instance.target!=null) { //do we have a target?
			//look for the angle to our target here so we can indicate to the player what this is.
			if (PlayerController.Instance.target!=null) {
				targetAngle = Vector3.Angle (PlayerController.Instance.transform.position - PlayerController.Instance.gunSightObject.transform.position, PlayerController.Instance.transform.position - NGUI_Base.Instance.targetFiringMarker.targetPosition); // PlayerController.Instance.target.transform.position);
				targetFraction = Mathf.Clamp01(targetAngle/closeAngle);
			} else { 
				targetFraction = 1f;
			}

			sightImage.color = Color.Lerp (aimHit, aimNormal, targetFraction); //change colour as we get close to target.
			transform.localScale = Vector3.one*Mathf.Lerp (hitScale, 1f, targetFraction); //Scale our signts down as we get a better aim also
		}
	}
}
