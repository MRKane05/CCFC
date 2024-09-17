using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugLog : MonoBehaviour {
    public Text DebugLogText;
    public static DebugLog Instance;
    public string DebugText;

    void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
    }

    public void AddDebugMessage(string thisMessage)
    {
        DebugText += " \n" + thisMessage;
        DebugLogText.text = DebugText;
    }
}
