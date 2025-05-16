using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_UpgradePanelBase : MonoBehaviour
{
    public UI_UpgradeSelectionPanel ourSelectionPanel;
    public StatsPanelHandler statsPanel;

    public virtual bool hasUpgradesOutstanding()
    {
        return false;
    }

    public virtual void ApplySelectedItem()
    {
    }

    public virtual void ApplyAndCallbackSelectedItem()
    {
        ApplySelectedItem();
    }

    public void confirmApply()
    {
        ApplySelectedItem();
    }

    public virtual void rejectApply()
    {
        //I guess we do nothing and open up our main panel
        //To do that we need a link back to our panel to do the work
        statsPanel.SetStats(gameManager.Instance.SelectedAircraft);
        ourSelectionPanel.blindDoSelectUpgradePath();
    }
}
