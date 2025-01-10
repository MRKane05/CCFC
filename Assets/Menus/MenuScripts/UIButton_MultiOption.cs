using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIButton_MultiOption : UIButtonFunction {
	public TMP_Text textOption;
    public List<string> textOptions = new List<string>();
	public int optionsIndex = 0;
	public string multiOptionName = "handedness";

    IEnumerator Start()
    {
        while (!UISettingsHandler.Instance)
        {
            yield return null;
        }
        //So at this stage we should pull our settings value
        int optionsIndex = UISettingsHandler.Instance.getSettingInt(multiOptionName);
        SelectOption(optionsIndex);
    }

    void SelectOption(int thisOption)
    {
        thisOption = Mathf.Clamp(thisOption, 0, textOptions.Count-1);   //Just a quick check to make sure that we won't pull an error
        textOption.text = textOptions[thisOption];
    }

    public void DoSelect()
    {
        //We'll just cycle through our options here
        optionsIndex++;
        if (optionsIndex >= textOptions.Count)
        {
            optionsIndex = 0;
        }
        SelectOption(optionsIndex);
        //We need to set our properties
        if (UISettingsHandler.Instance)
        {
            UISettingsHandler.Instance.addSettingsInt(multiOptionName, optionsIndex);
        }
    }
}
