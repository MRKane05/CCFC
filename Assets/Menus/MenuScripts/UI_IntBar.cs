using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_IntBar : MonoBehaviour {
    public UI_UpgradePanelActive ParentUpgradePanel;
    public TextMeshProUGUI applyCost;
    public TextMeshProUGUI unapplyCost;
    public TextMeshProUGUI totalCost;
    public Image IntDisplaySprite;
    public float IntSegments = 5f;
    public float IntValue = 1f; //What's our current value?
    public float totalUpgradeCost = 0f;

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
        applyCost.text = ParentUpgradePanel.currentUpgradePath.costPerLevel.ToString();
        unapplyCost.text = "0";
        totalCost.text = "0/" + gameManager.Instance.playerStats.money.ToString();
        SetIntFraction();
    }

    public void ChangeIntValue(int byThis)
    {
        float tempIntValue = Mathf.Clamp(IntValue + byThis, 0, IntSegments);

        //We need to handle costs
        float intDifference = tempIntValue - ParentUpgradePanel.currentUpgradePath.upgradeLevel;
        totalUpgradeCost = intDifference * ParentUpgradePanel.currentUpgradePath.costPerLevel;
        if (totalUpgradeCost > gameManager.Instance.playerStats.money)
        {
            //This is a false
            return;
        }

        totalCost.text = Mathf.Clamp(totalUpgradeCost, 0, 9999999).ToString() + "/" + gameManager.Instance.playerStats.money.ToString();

        if (ParentUpgradePanel.BarLevelCallback(tempIntValue, byThis))
        {
            IntValue = tempIntValue;
            SetIntFraction();
        }
    }
}
