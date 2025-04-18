using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_UpgradeSelectionPanel : MonoBehaviour {

    public GameObject LinearSelectionPanel;
    public GameObject ItemUpgradePanel;
    public string currentUpgradePath = "";

    public void SelectUpgradePath(string thisPath)
    {
        currentUpgradePath = thisPath;
        UI_UpgradePanelActive thisUpgradePanel;
        //switch case based off of string
        switch (thisPath.ToLower())
        {
            case "airframe":
                LinearSelectionPanel.SetActive(true);
                thisUpgradePanel = LinearSelectionPanel.GetComponent<UI_UpgradePanelActive>();
                thisUpgradePanel.currentUpgradePath = gameManager.Instance.SelectedAircraft.upgrade_airframe;
                thisUpgradePanel.SetupUpgradePanel("Airframe");
                break;
            case "engine":
                LinearSelectionPanel.SetActive(true);
                thisUpgradePanel = LinearSelectionPanel.GetComponent<UI_UpgradePanelActive>();
                thisUpgradePanel.currentUpgradePath = gameManager.Instance.SelectedAircraft.upgrade_engine;
                thisUpgradePanel.SetupUpgradePanel("Engine");
                break;
            case "armor":
                LinearSelectionPanel.SetActive(true);
                thisUpgradePanel = LinearSelectionPanel.GetComponent<UI_UpgradePanelActive>();
                thisUpgradePanel.currentUpgradePath = gameManager.Instance.SelectedAircraft.upgrade_armor;
                thisUpgradePanel.SetupUpgradePanel("Armor");
                break;
        }
    }
}
