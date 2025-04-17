using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_CompareBar : MonoBehaviour {
    public Image Bar_Backing;
    public Image Bar_Top;
    public Image Bar_Bottom;
    public Color Color_Gain;
    public Color Color_Loss;

    public float currentBarValue = 0.75f;
    public float compareValue = 0.75f;

    public void setBackingBarFill(float toThis) //Used to indicate just how much of something we can have
    {
        Bar_Backing.fillAmount = toThis;
    }

    public void SetCurrentValue(float toThis)
    {
        currentBarValue = toThis;
        Bar_Top.fillAmount = toThis;
        Bar_Bottom.fillAmount = toThis;
    }

    void Update()
    {
        //DoCompareValue(compareValue);
    }

    public void DoCompareValue(float compareWith)
    {
        if (compareWith > currentBarValue)
        {
            Bar_Bottom.fillAmount = compareWith;
            Bar_Bottom.color = Color_Gain;
            Bar_Top.fillAmount = currentBarValue;
        } else
        {
            Bar_Bottom.fillAmount = currentBarValue;
            Bar_Bottom.color = Color_Loss;
            Bar_Top.fillAmount = compareWith;
        }
    }
}
