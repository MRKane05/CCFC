using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//A script that'll handle panel behaviour with a focus on better supporting touch controls
public class PanelHandler : MonoBehaviour {
    [Space]
    [Header ("Linked Funcionality")]
    public GameObject returnPanel, returnButton, startButton;
    public GameObject floatingStartButton;
    //public GameController.enGameControllerState panelGameState = GameController.enGameControllerState.MENU;
    //public GameController.enGameControllerState necessaryCloseState = GameController.enGameControllerState.NULL;    //We can only close the menu if we're in this state
    //public GameController.enGameControllerState optionalReturnState = GameController.enGameControllerState.NULL;
    void OnEnable()
    {
        if (PanelMaster.Instance)
        {
            PanelMaster.Instance.AddOpenPanel(this);
        }

        if (floatingStartButton)
        {
            UIHelpers.SetSelectedButton(floatingStartButton);
            floatingStartButton = null; //clear this button after we've returned to it
        }
        else if (startButton)
        {
            UIHelpers.SetSelectedButton(startButton);   //This isn't quite right as we might be getting a command to return to a particular button....
        }
    }

    //Called by a button prompt, or something
    public void OnClose()
    {
        //This can be called from a button, so we'd better check if this needs cleared off of the PanelMaster
        if (PanelMaster.Instance)
        {
            PanelMaster.Instance.CheckPanelRemoved(this);
        }
        if (returnPanel)
        {
            returnPanel.SetActive(true);
            PanelHandler returnHandler = returnPanel.GetComponent<PanelHandler>();
            if (returnHandler) //When exactly is "OnEnable" called?
            {
                returnHandler.floatingStartButton = returnButton;
            }
        } else if (returnButton)
        {
            UIHelpers.SetSelectedButton(returnButton);
        }

        gameObject.SetActive(false); //Turn this panel off
    }

    void Update()
    {
        //if (necessaryCloseState == GameController.enGameControllerState.NULL || GameController.Instance.GameControllerState == necessaryCloseState) {
            //if (Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("Triangle") || Input.GetButtonDown("Circle"))
            //{
                //if (GameController.Instance)
                //{
                //    GameController.Instance.PlayReturn();
                //}
            //    OnClose();
            //}
        //}
    }
}
