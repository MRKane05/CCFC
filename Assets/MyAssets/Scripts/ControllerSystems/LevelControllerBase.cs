﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//As I expand what this game is ther eare some functions that I really need to move to a base class and have other things reference/call as necessary
public class LevelControllerBase : MonoBehaviour {

	private static LevelControllerBase instance = null;
	public static LevelControllerBase Instance { get { return instance; } }

	void Awake()
	{
		if (instance)
		{
			Debug.Log("Duplicate attempt to create LevelControllerBase");
			Destroy(this);
			return;
		}

		instance = this;
	}

	public void finishMatch(bool bPlayerDestroyed)
	{
		//Of course we need some concept of how well this has gone
		//PROBLEM: Mission success evaluation isn't complete yet
		gameManager.Instance.levelResults.bWonLevel = !bPlayerDestroyed;    //Flip this if we're destroyed for the moment
		StartCoroutine(FinishLevel(bPlayerDestroyed));
		//gameManager.Instance.ConcludeMission();
	}

	IEnumerator FinishLevel(bool bPlayerDestroyed)
	{
		yield return new WaitForSeconds(3f);    //Give a little pause after it's complete
												//We want to load our panel scene here and pause so that the player can bask in the warm glow of seeing a mission complete scene
		if (!bPlayerDestroyed)
		{
			AsyncOperation async = Application.LoadLevelAsync("LevelComplete_Win"); //We're assuming that the player won...
			yield return async;
		}
		else
		{
			AsyncOperation async = Application.LoadLevelAsync("LevelComplete_ShotDown"); //We're assuming that the player won...
			yield return async;
		}
		Time.timeScale = 0f;    //Going to need to turn this back on somewhere...
								//What we could actually start doing now is loading our other scene in the background...I'm not sure just how that'll work however, so for the moment fuck it, lets get it working!
	}
}
