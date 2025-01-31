using UnityEngine;
using System.Collections;

//This is only a bones class at the moment but will hopefully be expanded into something that handles all of our explosions and higher SFX
public class sfx_Explosion : MonoBehaviour {

	public GameObject target; //that which we're attached to
	public float lifeTime = 4f; //how long until we delete this?
	public float soundDelay = 3f;

	void Start () {
		StartCoroutine (delaySound(soundDelay));
		Destroy (gameObject, lifeTime); //put the watcher on it
	}

	IEnumerator delaySound(float soundDelay) {
		yield return new WaitForSeconds(soundDelay);
		GetComponent<AudioSource>().Play(); //will be our bang noise
		yield return null;
	}

	void LateUpdate () {
		//track our target
		if (target!=null) {
			transform.position = target.transform.position;
			transform.rotation = target.transform.rotation;
		}
	}
}
