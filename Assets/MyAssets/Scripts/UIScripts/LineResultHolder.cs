using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LineResultHolder : MonoBehaviour {
	public GameObject lineResultPrefab;

	public TextMeshProUGUI totalNumber;
	public TextMeshProUGUI totalScore;

	float finalNumber = 0;
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
			newLineScript.SetLine(key, ScoreItem.count.ToString(), (ScoreItem.count * ScoreItem.itemPoints).ToString());

			finalNumber += ScoreItem.count;
			finalScore += (ScoreItem.count * ScoreItem.itemPoints);

		}

		finalScore = Mathf.RoundToInt(finalScore);

		totalNumber.text = finalNumber.ToString();
		totalScore.text = finalScore.ToString();

		gameManager.Instance.playerStats.money += finalScore;

		gameManager.Instance.savePlayerData();	//So that we keep our money
	}
}
