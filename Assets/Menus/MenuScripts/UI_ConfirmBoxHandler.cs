using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI_ConfirmBoxHandler : MonoBehaviour {
    public TextMeshProUGUI messageText;

    string confirmFunction = "";
    string rejectFunction = "";
    MonoBehaviour callerObject;
    public GameObject startingButton;
    public void setupCallback(MonoBehaviour newCaller, string confirmMessage, string newConfirmFunction, string newRejectFunction)
    {
        messageText.text = confirmMessage;
        callerObject = newCaller;
        confirmFunction = newConfirmFunction;
        rejectFunction = newRejectFunction;

        UIHelpers.SetSelectedButton(startingButton);    //Of course this'll break things...
    }

    public void doReturnCall(bool bState)
    {
        if (callerObject && confirmFunction.Length > 3 && rejectFunction.Length > 3)
        {
            if (bState)
            {
                callerObject.Invoke(confirmFunction, 0f);
            } else
            {
                callerObject.Invoke(rejectFunction, 0f);
            }
        }

        //Clear our setup calls
        callerObject = null;
        confirmFunction = "";
        rejectFunction = "";
        gameObject.SetActive(false);
        //Should we be keeping an eye on this through the gameManager?
    }
}
