using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

[System.Serializable]
public class CharacterPortrait
{
	public string characterPortrait = ""; //What portrait will we load for this line of dialogue?
	public bool bCharacterActive = false;
	public bool bNeedsAnimateOn = false;
	public bool bNeedsAnimateOff = false;
}

[System.Serializable]
public class DialogueLine
{
	public List<CharacterPortrait> characterPortraits_Left = new List<CharacterPortrait>(); //If this is null then the portraits won't be changed
	public List<CharacterPortrait> characterPortraits_Right = new List<CharacterPortrait>(); //If this is null then the portraits won't be changed

	public string spokenLine = "";	//What line of dialogue will be up to read?
	public string audioClipName = ""; //What's the name of the audio clip we should load from resources/streaming assets?
}

[System.Serializable]
public class DialogueGroup
{
	public string language = "ENG";
	public List<DialogueLine> currentDialogue;
}

public class UI_DialogueManager : MonoBehaviour {
	public List<DialogueGroup> currentFullDialogue;	//This will be grouped by language
	public List<DialogueLine> currentDialogue;	//This is the per-language curated set
	[Space]
	[Header("Dialogue Read Area")]
	public TextMeshProUGUI dialogueLine;
	public int currentLine = 0;
	public GameObject nextButton;
	[Space]
	[Header("Character Portrait Details")]
	public GameObject portraitPrefab;
	public GameObject portraitAnchor_Left;
	public GameObject portraitAnchor_Right;
	//This might be an approach, but so would scaling
	/*
	public Color speakerActive = Color.white;
	public Color speakerInactive = Color.grey;
	*/
	float activeSpeakerScale = 1.1f;	//This works a bit more intuitively

	public List<GameObject> currentPortraits_Left = new List<GameObject>();
	public List<GameObject> currentPortraits_Right = new List<GameObject>();

	float stackingOffset = 180f;

	IEnumerator Start()
    {
		yield return null;
		UIHelpers.SetSelectedButton(nextButton);
		//nextLine();
		loadAndParseDialogueFile("TestDialogue");
    }

	void loadAndParseDialogueFile(string fileName)
    {
		string assetPath = "Dialogue/" + fileName;
		Object newDialogueObject = Resources.Load(assetPath);
		if (newDialogueObject)
		{
			TextAsset newDialogue = (TextAsset)newDialogueObject;
			string newTxt = newDialogue.text;
			string[] lines = newTxt.Split(new string[] { "\r\n", "\n" }, System.StringSplitOptions.None); //Regex.Split(newTxt, "\n|\r|\r\n");
			//Debug.Log("Lines: " + lines.Length);
			parseDialogueFile(lines);
		}
	}

	string leftCharCodon = "charLeft:";
	string rightCharCodon = "charRight:";
	string lineCodon = "charLine:";
	string audioCodon = "charAudio:";

	CharacterPortrait parseCharacterData(string thisCharacter)
    {
		CharacterPortrait newCharacter = new CharacterPortrait();
		string newEntry = thisCharacter.Trim();
		if (newEntry.Length > 3)    //Quick sanity check
		{
			bool bCharacterStar = false;
			if (newEntry.Contains("*"))
			{
				newCharacter.bCharacterActive = true;
				newEntry.Replace("*", "");  //Remove our star so that we don't carry it through to our filename
			}
			if (newEntry.Contains(">"))
            {
				newCharacter.bNeedsAnimateOn = true;
				newEntry.Replace(">", "");
            }
			if (newEntry.Contains("<"))
			{
				newCharacter.bNeedsAnimateOff = true;
				newEntry.Replace("<", "");
			}
			//CharacterPortrait newPortrait = new CharacterPortrait();
			newCharacter.characterPortrait = "Portraits/" + newEntry;
		}
		return newCharacter;
    }

	void parseDialogueFile(string[] lines)
    {
		//The idea here is that we'll have a file that's human readable and easy to write for our dialogue interactions i.e.
		//* denotes scaled character portrait
		//> denotes that the portrait needs to animate on
		//< denotes that the portrait needs to animate off
		//charLeft: Portrait_Farnsworth* 
		//charRight: Portrait_Terry
		//charLine: Good news everyone!
		//charAudio: audio_filename
		currentDialogue = new List<DialogueLine>();
		DialogueLine newDialogue = null;// = new DialogueLine();
		foreach(string thisLine in lines)
        {
			//Left character codon handling=======================================================================================================
			if (thisLine.ToLower().Contains(leftCharCodon.ToLower()))	//This will always lead the section
            {
				if (newDialogue != null)
                {
					currentDialogue.Add(newDialogue);
                }
				newDialogue = new DialogueLine();
				//Lets get all the characters that we should have in our section
				string trimmedLine = thisLine.Remove(0, leftCharCodon.Length); //Remove our codon from the start
				trimmedLine = trimmedLine.Trim();
				//Debug.Log(trimmedLine);
				string[] left_Characters = trimmedLine.Split(new char[] { ' ' });	//Seperate at spaces
				foreach (string newLeftCharacter in left_Characters)
                {
					string newEntry = newLeftCharacter.Trim();
					if (newEntry.Length > 3)    //Quick sanity check
					{
						CharacterPortrait newPortrait = parseCharacterData(newEntry);
						newDialogue.characterPortraits_Left.Add(newPortrait);
					}
                }
            }

			//Right character codon handling=======================================================================================================
			else if (thisLine.ToLower().Contains(rightCharCodon.ToLower()))   //This will always lead the section
			{
				//Lets get all the characters that we should have in our section
				string trimmedLine = thisLine.Remove(0, rightCharCodon.Length); //Remove our codon from the start
				trimmedLine = trimmedLine.Trim();
				//Debug.Log(trimmedLine);
				string[] right_Characters = trimmedLine.Split(new char[] { ' ' });   //Seperate at spaces
				foreach (string newRightCharacter in right_Characters)
				{
					string newEntry = newRightCharacter.Trim();
					if (newEntry.Length > 3)	//Quick sanity check
					{
						CharacterPortrait newPortrait = parseCharacterData(newEntry);
						newDialogue.characterPortraits_Right.Add(newPortrait);
					}
				}
			}

			else if (thisLine.ToLower().Contains(lineCodon.ToLower()))
            {
				string trimmedLine = thisLine.Remove(0, lineCodon.Length); //Remove our codon from the start
				//PROBLEM: In theory we could have hard returns going on with our dialogue, and I'll have to write something to handle that
				newDialogue.spokenLine = trimmedLine;
			}
		}

		//Add the last line we were processing:
		currentDialogue.Add(newDialogue);
		currentLine = 0; //Reset our counters
		nextLine(); //Kick our entire process off after everything is parsed!
	}

	public void nextLine()
    {
		if (currentLine < currentDialogue.Count)
        {
			dialogueLine.text = currentDialogue[currentLine].spokenLine;
			//Now we need to handle our portraits :)
			setPortraits(true, currentPortraits_Left, currentDialogue[currentLine].characterPortraits_Left);
			setPortraits(false, currentPortraits_Right, currentDialogue[currentLine].characterPortraits_Right);
			currentLine++;
        } else
        {
			//We need to close this menu down and go back to whatever we had previously
        }
    }

	void setPortraits(bool bIsLeft, List<GameObject> portraitsList, List<CharacterPortrait> newPortraits)
    {
		//recycle things as necessary

		if (portraitsList.Count < newPortraits.Count)
        {
			while (portraitsList.Count < newPortraits.Count)	//Add spacers until we need them
			{
				//We need to add a few more portraits for this to go ahead
				GameObject newPortrait = Instantiate(portraitPrefab);
				if (bIsLeft)
				{
					newPortrait.transform.SetParent(portraitAnchor_Left.transform);
				} else
                {
					newPortrait.transform.SetParent(portraitAnchor_Right.transform);
				}
				portraitsList.Add(newPortrait);
			}
        }
		//Now setup these portraits
		for (int i=0; i< portraitsList.Count; i++)
        {
			int stackedIndex = portraitsList.Count - i - 1;
			if (newPortraits.Count > i)
			{
				Texture2D portraitTex = Resources.Load(newPortraits[i].characterPortrait) as Texture2D;
				if (!portraitTex)
                {
					Debug.LogError("Portrait not found. Path: " + newPortraits[i].characterPortrait);
                }
				portraitsList[stackedIndex].GetComponent<Image>().sprite = Sprite.Create(portraitTex, new Rect(0, 0, portraitTex.width, portraitTex.height), Vector2.zero);

				portraitsList[stackedIndex].SetActive(true);
				portraitsList[stackedIndex].transform.localPosition = new Vector3(0, i * stackingOffset, 0);    //good idea incorrect outcome
				portraitsList[stackedIndex].transform.localScale = Vector3.one * (newPortraits[i].bCharacterActive ? activeSpeakerScale : 1f);

				if (!bIsLeft)
				{
					//We need to flip the X scale on this image
					portraitsList[stackedIndex].transform.localScale = new Vector3(-portraitsList[stackedIndex].transform.localScale.x, portraitsList[stackedIndex].transform.localScale.y, portraitsList[stackedIndex].transform.localScale.z);
				}

			} else {  //turn off this graphic and keep if for the time being
				portraitsList[stackedIndex].SetActive(false);
			}
        }
    }
}
