using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIButton_Slider : UIButtonFunction
{
    public Slider buttonSlider;

    float sliderStartValue = 1f;
    float sliderTickValue = 1f / 11f;
    public string multiOptionName = "look_x_sensitivity";

    IEnumerator Start()
    {
        while (!UISettingsHandler.Instance)
        {
            yield return null;
        }
        //So at this stage we should pull our settings value
        float sliderValue = UISettingsHandler.Instance.getSettingFloat(multiOptionName);
        setSliderValue(sliderValue);
    }

    public void setSliderValue(float toThis)
    {
        buttonSlider.value = toThis;
    }

    public void ValueChanged(float toThis)
    {
        if (Mathf.Abs(sliderStartValue - toThis) > sliderTickValue)
        {
            sliderStartValue = toThis;  //We should set this for when it crosses thresholds...
            if (UIInteractionSound.Instance)
            {
                UIInteractionSound.Instance.PlaySelect();
            }
        }

        UISettingsHandler.Instance.addSettingsFloat(multiOptionName, toThis);
    }
}
