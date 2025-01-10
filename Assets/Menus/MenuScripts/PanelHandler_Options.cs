using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelHandler_Options : PanelHandler
{
    public override void Init()
    {
        IsInitialised = true;
        //we want to pull all of our settings at this particular stage of the setup to make sure that they're displayed correctly
    }

    public override void DoClose()
    {
        UISettingsHandler.Instance.ConfirmChanges();
        base.DoClose(); //Do the rest of our close function for the panel
    }
}
