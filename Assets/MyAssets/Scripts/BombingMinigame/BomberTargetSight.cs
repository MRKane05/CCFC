using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Bit of a backwards setup with this to try and make things properly unnatural
public class BomberTargetSight : MonoBehaviour {
	public GameObject bomber;
	public float forwardDistance;
	public float lerpSpeed = 5f;
	public LayerMask bombHitMask;
	public GameObject bombObject;
	public GameObject groundObject; //To use as a "world object" for dropping bombs

	public float targetSightHeight = 40f;
	public int bombsAvaliable = 6;
	
	// Update is called once per frame
	void Update () {
		Vector3 targetPosition = new Vector3(bomber.transform.position.x, targetSightHeight, bomber.transform.position.z + forwardDistance);
		gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, targetPosition, Time.deltaTime * lerpSpeed);

		//Ok, I'm happy with how that feels. Lets see about doing our bomb dropping stuff :)
		if (Input.GetButtonDown("Right Shoulder") || Input.GetKeyDown(KeyCode.Space))
        {
			doDropBomb();
        }
	}

	public void doDropBomb()
    {
		if (bombsAvaliable <=0 ) { return; }
		bombsAvaliable -= 1;
		//We need to do a raycast through the center of our sight object (this) and see where it hits the ground (or another target)
		//Actually this is simply screen position to targetsignt position
		Vector3 rayDir = gameObject.transform.position- Camera.main.transform.position;
		rayDir = rayDir.normalized;

		RaycastHit hit;
		// Does the ray intersect any objects excluding the player layer
		if (Physics.Raycast(Camera.main.transform.position, rayDir.normalized, out hit, Mathf.Infinity, bombHitMask))

		{
			//Debug.DrawRay(Camera.main.transform.position, rayDir * hit.distance, Color.yellow);
			//Debug.Log("Did Hit");
			//We we basically need a bomb object that'll do a lerp from the player position down to the hit position
			doBombObject(hit.point);
		}
		else
		{
			//Debug.DrawRay(Camera.main.transform.position, rayDir * 1000, Color.white);
			//Debug.Log("Did not Hit");
		}
	}

	public void doBombObject(Vector3 targetPosition)
    {
		GameObject newBomb = Instantiate(bombObject) as GameObject;
		newBomb.transform.position = bomber.transform.position - Vector3.up * 2f;   //Put the bomb in below our fighter
		PointLerpBomb bombScript = newBomb.GetComponent<PointLerpBomb>();
		newBomb.transform.SetParent(groundObject.transform);

		targetPosition = groundObject.transform.InverseTransformPoint(targetPosition);
		bombScript.SetupBomb(targetPosition, groundObject); //PROBLEM: In theory we should have something to denote bomb travel time, but lets just get with it!
    }
}
