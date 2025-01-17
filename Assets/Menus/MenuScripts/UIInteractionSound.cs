using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A singleton for playing any audio of buttons being pressed/interacted with
public class UIInteractionSound : MonoBehaviour {
	private static UIInteractionSound instance = null;
	public static UIInteractionSound Instance { get { return instance; } }

	public AudioSource ourAudioSource;
	public AudioClip SelectionChangeSound;
	public AudioClip ClickButtonSound;
	public AudioClip DeniedButtonSound;
	void Awake()
	{
		if (instance)
		{
			Debug.Log("Somehow there's a duplicate UIInteractionSound in the scene");
			Debug.Log(gameObject.name);
			Destroy(gameObject);    //Remove ourselves from the scene
		}
		else
		{
			instance = this;
			if (!ourAudioSource)
			{
				ourAudioSource = gameObject.GetComponent<AudioSource>();
			}
		}
	}

	public void PlayClick()
    {
		if (ourAudioSource)
		{
			ourAudioSource.PlayOneShot(ClickButtonSound);
		}
	}

	public void PlayDenied()
    {
		if (ourAudioSource)
		{
			ourAudioSource.PlayOneShot(DeniedButtonSound);
		}
	}

	public void PlaySelect()
    {
		if (ourAudioSource)
		{
			ourAudioSource.PlayOneShot(SelectionChangeSound);
		}
	}
}
