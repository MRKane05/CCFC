using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_IntBar : MonoBehaviour {
    public UI_UpgradePanelActive ParentUpgradePanel;

    public Image IntDisplaySprite;
    public float IntSegments = 5f;
    public float IntValue = 1f; //What's our current value?

    void Start()
    {
        SetIntFraction();
    }

    void SetIntFraction()
    {
        IntDisplaySprite.fillAmount = IntValue / IntSegments;
    }

    public void SetIntValue(int toThis)
    {
        IntValue = toThis;

        SetIntFraction();
    }

    public void ChangeIntValue(int byThis)
    {
        float tempIntValue = Mathf.Clamp(IntValue + byThis, 0, IntSegments);
        if (ParentUpgradePanel.BarLevelCallback(tempIntValue, byThis))
        {
            IntValue = tempIntValue;
            SetIntFraction();
        }
    }
}
