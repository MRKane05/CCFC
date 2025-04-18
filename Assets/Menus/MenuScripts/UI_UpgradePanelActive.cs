using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//This is an interface class to the other systems and will pass things back and forward
public class UI_UpgradePanelActive : MonoBehaviour {
    public TextMeshProUGUI titleText;
    public UI_IntBar ourDisplayBar;
    public StatsPanelHandler staticsPanel;
    public UI_UpgradeSelectionPanel upgradeSelectionPanel;


    public string upgradePathName = "";
    public UpgradePathEffects currentUpgradePath;

    float currentUpgradeLevel = 1; //A quick cache for ease of use

    //Called after the system has hard-set our upgrade details, and to make sure that everything is displayed as expected
    public void SetupUpgradePanel(string upgradePath)
    {
        ourDisplayBar.SetIntValue(Mathf.FloorToInt(currentUpgradePath.upgradeLevel));
        upgradePathName = upgradePath;
        titleText.text = upgradePath;
    }

    //Called when the user interacts with our bar, and the system will squawk if there's something preventing this interaction (money, weight, whatever)
    public void BarLevelCallback(float currentLevel, float thisLevel)
    {
        //Do a check for the current cost
        //Do a check for the current weight
        //Pipe everything back through our system

        //Lets start with the comparitors
        currentUpgradeLevel = currentLevel;
        staticsPanel.SetComparison(currentUpgradePath, currentLevel);
    }

    public void ApplySelectedUpgrades()
    {
        float upgradeDifference = (currentUpgradeLevel - currentUpgradePath.upgradeLevel)/5f;

        gameManager.Instance.SelectedAircraft.weight_current += currentUpgradePath.finalWeight * upgradeDifference;
        gameManager.Instance.SelectedAircraft.armor_current += currentUpgradePath.finalArmor * upgradeDifference;
        gameManager.Instance.SelectedAircraft.agility_current += currentUpgradePath.finalAgility * upgradeDifference;
        gameManager.Instance.SelectedAircraft.speed_current += currentUpgradePath.finalSpeed * upgradeDifference;
        gameManager.Instance.SelectedAircraft.accel_current += currentUpgradePath.finalAccel * upgradeDifference;

        //Do money stuff

        //Do data storage stuff
        currentUpgradePath.upgradeLevel = currentUpgradeLevel;  //This needs to be applied back to our game controller
        //And apply the necessary levels to our gameManager reference
        switch (upgradePathName.ToLower())
        {
            case "airframe":
                gameManager.Instance.SelectedAircraft.upgrade_airframe.upgradeLevel = currentUpgradeLevel;
                break;
            case "engine":
                gameManager.Instance.SelectedAircraft.upgrade_engine.upgradeLevel = currentUpgradeLevel;
                break;
            case "armor":
                gameManager.Instance.SelectedAircraft.upgrade_armor.upgradeLevel = currentUpgradeLevel;
                break;
        }
        //And finally we need to set our stats after the upgrade
        staticsPanel.SetStats(gameManager.Instance.SelectedAircraft);
    }
}
