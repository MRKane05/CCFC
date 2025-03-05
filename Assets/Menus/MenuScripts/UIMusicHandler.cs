using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMusicHandler : MonoBehaviour {
	private static UIMusicHandler instance = null;
	public static UIMusicHandler Instance { get { return instance; } }
	public AudioSource ourAudioSource;

	void Awake()
	{
		if (instance)
		{
			//Debug.Log("Removing duplicate UIMusicHandler");
			//Debug.Log(gameObject.name);
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

	public AudioClip menuTheme;
	public AudioClip[] ActionThemes;

	public void SetMusicTrack(bool bMenuMusic)
    {
		if (!bMenuMusic)
        {
			ourAudioSource.clip = ActionThemes[Random.Range(0, ActionThemes.Length)];
			ourAudioSource.Play();
        } else
        {
			ourAudioSource.clip = menuTheme;
			ourAudioSource.Play();
        }
	}
}


