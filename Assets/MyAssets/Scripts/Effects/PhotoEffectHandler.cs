using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotoEffectHandler : MonoBehaviour {
	public AudioClip cameraNoise;
	AudioSource ourAudio;
	LensFlare ourLensFlare;

	public float LensFlareDuration = 0.75f;
	public AnimationCurve lensFlareAnimation = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.25f, 1f), new Keyframe(1f, 0f));

	// Use this for initialization
	void Start () {
		ourAudio = gameObject.GetComponent<AudioSource>();
		ourLensFlare = gameObject.GetComponent<LensFlare>();
	}
	
	public void doCameraEffect()
    {
		StartCoroutine(CameraEffect());
    }

	IEnumerator CameraEffect()
	{
		ourAudio.pitch = Random.Range(0.8f, 1.2f);
		ourAudio.PlayOneShot(cameraNoise);
		ourLensFlare.enabled = true;
		float timeStart = Time.time;
		while (timeStart + LensFlareDuration > Time.time)
        {
            float LensFlareAlpha = lensFlareAnimation.Evaluate((Time.time - timeStart) / LensFlareDuration);
			ourLensFlare.brightness = LensFlareAlpha;
			//Debug.Log(LensFlareAlpha);
            yield return null;
		}
		ourLensFlare.enabled = false;
		ourLensFlare.brightness = 0f;
	}
}
