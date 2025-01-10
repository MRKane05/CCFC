using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugLogger : MonoBehaviour {
    public TMP_Text DebugLogText;
    public string TextLog = "";
    public bool bDisplayActive = false;
    void Awake()
    {
        Application.logMessageReceived += LogCallback;
    }

    void Start()
    {
        DebugLogText.gameObject.SetActive(bDisplayActive);
    }

    void OnEnable()
    {
        Application.logMessageReceived += LogCallback;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= LogCallback;
    }

    void LogCallback(string logString, string stackTrace, LogType type)
    {
        if (!logString.Contains("UnityWaterMark"))
        {
            TextLog += '\n' + logString;
        }
        DebugLogText.text = TextLog;
        //Or Append the log to the old one
        //logText.text += logString + "\r\n";
    }

    public void Update()
    {
        //Check to see if we should toggle our display on or off
        if (Input.GetButtonDown("Dup") || Input.GetKeyDown(KeyCode.L))
        {
            bDisplayActive = !bDisplayActive;
            DebugLogText.gameObject.SetActive(bDisplayActive);
            if (bDisplayActive)
            {
                DebugLogText.text = TextLog;    //Remember to assign this
            }
        }
    }
}
