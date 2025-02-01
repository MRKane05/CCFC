using UnityEngine;
using System.Collections;

//This points at the target we're targeting. Might be expanded to be an indicator of sorts
public class TargetIndicator : MonoBehaviour {
	public ActorController ourAnchor;
	public GameObject indicator;
	public Color targetColor, currentColor;

	public Material markerMat;

	void Start() {
		//set this stuff up
		markerMat = indicator.GetComponent<Renderer>().sharedMaterial;
	}

	public bool IsVisibleFrom(Renderer renderer, Camera camera) {
		if (renderer == null) //we've no renderer on this one...
			return true; //assume that it's ok.

		Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
		return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
	}
	
	// Will want to animate this at some stage to fit the toony feel
	void Update () {
		if (ourAnchor.target!=null) {
			if (!IsVisibleFrom(ourAnchor.target.GetComponent<Renderer>(), Camera.main)) { //can we see this? Then make it dominant
				if (ourAnchor.target!=null) {//(!ourAnchor.target.renderer.IsVisibleFrom(Camera.main))) { //could do with fading this out when our target is visible in the camera
					//indicator.SetActive(true);
					transform.LookAt(ourAnchor.target.transform.position);
					targetColor = Color.red;
				}
			}
			else { //should check to see if we've a waypoint
				//indicator.SetActive(false); //turn this off
				targetColor = new Color(targetColor[0], targetColor[1], targetColor[2], 0);
			}
		}
		else { //see if we've a valid waypoint that we should be aiming toward.
			if (((LevelController)LevelControllerBase.Instance).getMissionEndGoal!=null) {//(((LevelController)LevelControllerBase.Instance).currentWaypoint < ((LevelController)LevelControllerBase.Instance).waypoints.Length) {
				//we can look at this because we've a valid waypoint to point at.
				if (!IsVisibleFrom(((LevelController)LevelControllerBase.Instance).getMissionEndGoal.GetComponent<Renderer>(), Camera.main)) { //can we see this? Then make it dominant

					transform.LookAt(((LevelController)LevelControllerBase.Instance).getMissionEndGoal.transform.position);
					targetColor = Color.yellow;

				}
				else { //should check to see if we've a waypoint
					//indicator.SetActive(false); //turn this off
					targetColor = new Color(targetColor[0], targetColor[1], targetColor[2], 0);
				}
			}
		}

		currentColor = Color.Lerp (currentColor, targetColor, Time.deltaTime*4); //this is working well
		markerMat.SetColor("_TintColor", currentColor); //this doesn't seem to be taking effect at all
	}
}
