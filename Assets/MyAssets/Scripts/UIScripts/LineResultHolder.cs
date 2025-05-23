using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LineResultHolder : MonoBehaviour {
	public GameObject lineResultPrefab;

	public TextMeshProUGUI totalNumber;
	public TextMeshProUGUI totalScore;

	float finalNumber = 0;
	float totalInLevel = 0;
	float finalScore = 0;
	
	// Use this for initialization
	void Start () {
		PopulateLineResults();
	}
	
	void PopulateLineResults()
    {
		foreach (string key in gameManager.Instance.PlayerScore.playerLevelScore.Keys)
        {
			LevelScoreItem ScoreItem = gameManager.Instance.PlayerScore.playerLevelScore[key];
			GameObject newLine = Instantiate(lineResultPrefab, transform);
			UI_ResultsLineEntry newLineScript = newLine.GetComponent<UI_ResultsLineEntry>();
			newLineScript.SetLine(key, ScoreItem.count.ToString() + "/" + ScoreItem.totalInLevel.ToString(), (ScoreItem.count * ScoreItem.itemPoints).ToString());

			finalNumber += ScoreItem.count;
			totalInLevel += ScoreItem.totalInLevel;
			finalScore += (ScoreItem.count * ScoreItem.itemPoints);

		}

		finalScore = Mathf.RoundToInt(finalScore);
		if (finalScore < 0)
        {
			finalScore = 0; //I don't want the player to lose money for a bad performance
        }
		totalNumber.text = finalNumber.ToString() + "/" + totalInLevel.ToString();
		totalScore.text = finalScore.ToString();

		gameManager.Instance.playerStats.money += finalScore;

		gameManager.Instance.savePlayerData();	//So that we keep our money
	}
}
