using UnityEngine;
using System.Collections;

//Used by anything we can shoot
public class TargetSphereOffset : MonoBehaviour {
	//We need our speed, and the distance to our player
	GameObject playerAircraft;
	public float ourSpeed=4, bulletSpeed=50;
	public GameObject targetSphere;
	
	// Use this for initialization
	IEnumerator Start () {
		//playerAircraft = GameObject.Find("PlayerAircraft");
		while (PlayerController.Instance == null)
			yield return null;
		
		playerAircraft = PlayerController.Instance.ourAircraft.gameObject;
	
	}
	
	// Update is called once per frame
	void Update () {
		//collider.center...
		if (playerAircraft)
			targetSphere.transform.position = transform.position + transform.forward*(playerAircraft.transform.position-transform.position).magnitude*ourSpeed/bulletSpeed;
	}
}
