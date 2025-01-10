using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIResultsLine : MonoBehaviour {
    public TextMeshProUGUI lineTitle;
    public TextMeshProUGUI lineValue;

    public void setLineDetails(string newTitle, string newValue)
    {
        lineTitle.text = newTitle;
        lineValue.text = newValue;
    }
}
