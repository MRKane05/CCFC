using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI_ResultsLineEntry : MonoBehaviour {
    public TextMeshProUGUI LineTitle;
    public TextMeshProUGUI NumberOfTargets;
    public TextMeshProUGUI LineScore;

    public void SetLine(string newTitle, string newNumberOfTargets, string newLineScore)
    {
        LineTitle.text = newTitle;
        NumberOfTargets.text = newNumberOfTargets;
        LineScore.text = newLineScore;
    }
}
