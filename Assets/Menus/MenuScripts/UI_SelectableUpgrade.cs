using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


//An upgrade class that can be toggled, radiobutton etc.
public class UI_SelectableUpgrade : MonoBehaviour {
    public UI_SelectableUpgradeBase parentPanel;
    public SelectableUpgradeType ourSelectableType;

    public TextMeshProUGUI buttonTitle;

    public bool bIsSelected = false;

    //We've got a button as a child
    public Button ourButton;


    public void SetUpgrade(SelectableUpgradeType thisType)
    {
        ourSelectableType = thisType;
        if (thisType == null)
        {
            ourButton.interactable = false;
            buttonTitle.text = "";
        }
        else
        {
            buttonTitle.text = thisType.upgradeName;
        }
    }

    public void GetUserToggle()
    {
        bIsSelected = !bIsSelected; //Will need to handle some unlock functionality here
        parentPanel.SetChildSelected(bIsSelected, ourSelectableType);
    }
}
