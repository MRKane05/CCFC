using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LevelChatterController;

[System.Serializable]
public class ChatterLines
{
	public enLineSpeaker lineSpeaker;
	public string line = "";
}

[System.Serializable]
public class Chatter
{
	public string name = "";
	public List<ChatterLines> chatterLines = new List<ChatterLines>();
}

//NGUI_Base.Instance.setPortraitMessage("Commander", "Protect our bombers!", Color.black);
//This class controls chatter that's sent through from wingmen. Essentially the system will request a bark and it'll play out accordingly
public class LevelChatterController : MonoBehaviour {
	private static LevelChatterController instance = null;
	public static LevelChatterController Instance { get { return instance; } }

	public enum enLineSpeaker { NULL, COMMANDER, GENERAL, WINGMAN_A, WINGMAN_B, SUBJECT }


	public List<Chatter> chatter = new List<Chatter>();

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

	public void playChatter(string callString)
    {
		foreach (Chatter thisChatter in chatter)
        {
			if (thisChatter.name == callString)
            {
				//Need to pull a random chatter
				ChatterLines newChatter = thisChatter.chatterLines[Random.Range(0, thisChatter.chatterLines.Count)];
				string characterName = "";
				switch (newChatter.lineSpeaker) {
					case enLineSpeaker.COMMANDER:
						characterName = "Commander";
						break;
					case enLineSpeaker.WINGMAN_A:
						characterName = "Wingman";
						break;
					case enLineSpeaker.WINGMAN_B:
						characterName = "Wingman";
						break;
					case enLineSpeaker.SUBJECT:
						characterName = "Special";
						break;
                }
				displayChatter(characterName, newChatter.line, Color.black);
			}
        }
    }

	public void displayChatter(string callerName, string targetChatter, Color teamColor)
    {
		NGUI_Base.Instance.setPortraitMessage(callerName, targetChatter, teamColor);
	}
}
