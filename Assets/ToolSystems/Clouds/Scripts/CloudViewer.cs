using UnityEngine;
using System.Collections;

//The object this class is attached to is the "global orientation object" for all of the cloud objects
//Basically this has the axis details which are used to rebuild the clouds as the object moves around
public class CloudViewer : MonoBehaviour {

	private static CloudViewer instance = null;
	public static CloudViewer Instance {get {return instance;}}

	public Material cloudMat; //used for the cloud "scrolling" that we can add for free
	public Vector2 cloudScroll = new Vector2(0.005F, 0);
	Vector2 cloudOffset = Vector2.zero;
	bool bRefreshClouds=false;

	public Vector3 cloudUp, cloudRight; //these are the only values we actually need here :)
	Vector3 previousCloudUp, previousCloudRight;
	private GameObject jointTransform;

	public bool bShouldRefresh() {
		return bRefreshClouds;
	}

	void Start() 
	{
		if(instance)
		{
			Debug.LogError("Duplicate attempt to create CloudViewer");
			Destroy(this);
		}
		
		//Set this as the singleton instance
		instance = this;
	}

	// Update is called once per frame
	void Update () {

		//quickly set our joint transform stuff
		if (!jointTransform) {
			jointTransform = new GameObject("jointTransform");
			jointTransform.transform.parent = gameObject.transform; //child this
			jointTransform.transform.localPosition = Vector3.zero;
		}

		jointTransform.transform.LookAt(gameObject.transform.forward*3F+gameObject.transform.position); //just to be sure

		cloudUp = jointTransform.transform.up;
		cloudRight = jointTransform.transform.right;

		//could do with some value checks to see if it "should" transform.
		//Debug.Log ((cloudUp-previousCloudUp).sqrMagnitude);
		if ((cloudUp - previousCloudUp).sqrMagnitude > 0.001F || (cloudRight - previousCloudRight).sqrMagnitude > 0.001F) {
			previousCloudUp = cloudUp;
			previousCloudRight = cloudRight;
			bRefreshClouds = true;
		}
		else {
			bRefreshClouds = false;
		}


		//and finally set our panning of the cloud material.
		if (cloudMat) {
			cloudOffset.x = Mathf.Repeat(cloudOffset.x + cloudScroll.x*Time.deltaTime, 1F);
			cloudOffset.y = Mathf.Repeat(cloudOffset.y + cloudScroll.y*Time.deltaTime, 1F);
			cloudMat.SetTextureOffset("_MainTex", cloudOffset);
		}
	}
}
