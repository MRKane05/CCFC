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

    public bool bInverted = false;

    public void setBackingBarFill(float toThis) //Used to indicate just how much of something we can have
    {
        Bar_Backing.fillAmount = toThis;
    }

    public void SetCurrentValue(float toThis)
    {
        if (bInverted)
        {
            toThis = 1f - toThis;
        }

        currentBarValue = toThis;
        Bar_Top.fillAmount = toThis;
        Bar_Bottom.fillAmount = toThis;
    }

    void Update()
    {
        //DoCompareValue(compareValue);
    }

    public void DoFlatCompare(float compareValue)
    {
        if (bInverted)
        {
            compareValue = 1f - compareValue;
        }

        if (compareValue > currentBarValue)
        {
            Bar_Bottom.fillAmount = compareValue;
            Bar_Bottom.color = Color_Gain;
            Bar_Top.fillAmount = currentBarValue;
        }
        else
        {
            Bar_Bottom.fillAmount = currentBarValue;
            Bar_Bottom.color = Color_Loss;
            Bar_Top.fillAmount = compareValue;
        }
    }

    public void DoCompareValue(float compareWith)
    {
        //Compare with needs to be "normalized"

        compareWith = currentBarValue + compareWith;
        compareValue = compareWith;
        if (compareValue > currentBarValue)
        {
            Bar_Bottom.fillAmount = compareValue;
            Bar_Bottom.color = Color_Gain;
            Bar_Top.fillAmount = currentBarValue;
        } else
        {
            Bar_Bottom.fillAmount = currentBarValue;
            Bar_Bottom.color = Color_Loss;
            Bar_Top.fillAmount = compareValue;
        }
    }
}
