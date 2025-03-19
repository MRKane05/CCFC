using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;


//A script that'll handle panel behaviour with a focus on better supporting touch controls
public class PanelHandler : MonoBehaviour {
    public enum enInteractionType { NONE, BASE, LOADED, RESIDENT }
    public enInteractionType enPanelType = enInteractionType.NONE;  //This'll also be handed as our panel scene types
    public bool IsInitialised { get; protected set; }
    [Space]
    public bool bCanBeDismissed = true;
    public string returnPanel_Scene = "";   //When the user presses close what panel scene do we fall back to?
    public string returnPanel_Button = "";  //What button will we open when we return from this panel?
    public enInteractionType returnPanelType = enInteractionType.NONE;
    public GameObject startButton;
    protected GameObject returnPanel;
    public TextMeshProUGUI buttonDescriptionText;

    public virtual void Init()
    {
        IsInitialised = true;
    }

    public UIButtonFunction[] UIButtons;


    public IEnumerator Start()
    {
        while (UIMenuHandler.Instance == null)  //Pause everything until we've got a handler instance logged
        {
            yield return null;
        }

        if (enPanelType == enInteractionType.BASE)
        {
            UIMenuHandler.Instance.AssertMenuAsBase(this);
        }
        if (startButton)
        {
            DoEnable(startButton.name);
        }

        //Go through and collect our buttons that could have text associated with them, and notify these that we're the thing they should be doing panel about
        UIButtons = gameObject.GetComponentsInChildren<UIButtonFunction>();
        foreach (UIButtonFunction thisButton in UIButtons)
        {
            thisButton.setPanelHandler(this);
        }
    }

    void OnEnable()
    {
        //All of this functionality should be handled by Start or the UIMenuHandler
        //DoEnable();
    }

    //When opening a panel we do it through our currently active one, which will disable the current panel in the process
    public virtual void CloseAndOpenPanel(GameObject targetPanel)
    {
        targetPanel.SetActive(true);    //Turn the panel we're going to open on
        targetPanel.GetComponent<PanelHandler>().SetupOpenPanel(gameObject); //Send this call through to open it
        gameObject.SetActive(false);    //Disable this panel
    }

    public void SetupOpenPanel(GameObject callingPanel)
    {
       
        returnPanel = callingPanel;
        //returnButton = EventSystem.current.currentSelectedGameObject;   //So we know what button we called this from

        DoEnable(startButton.name);
    }

    //All the logic that should be called when our panel is enabled or turned on
    public virtual void DoEnable(string targetStartButton)
    {
        if (!IsInitialised)
        {
            Init();
        }
        if (targetStartButton.Length >3)
        {
            GameObject startButton = UIHelpers.FindChildByName(gameObject, targetStartButton);
            if (startButton)
            {
                UIHelpers.SetSelectedButton(startButton);   //Send a select call through to set this button as start
                //startButton.GetComponent<UIButtonFunction>().bNeedsSelected = true;  //Make sure that this gets selected
            }
        }
    }

    //Called by a button prompt, or something
    public void OnClose()
    {
        DoClose();
    }

    //This is a terrible place to put this, but for the moment...
    public virtual void LoadMenuScene(string sceneName)
    {
        Debug.Log("Loading Scene: " + sceneName);

        if (UIMenuHandler.Instance)
        {
            Debug.Log("Additively loading scene: " + sceneName);
            UIMenuHandler.Instance.LoadMenuSceneAdditively(sceneName, this, null);
        }
    }

    public virtual void LoadScene(string sceneName)
    {
        if (UIMenuHandler.Instance)
        {
            UIMenuHandler.Instance.TransitionLoadScene(sceneName);
        }
    }

    //Handle what our panel does if we get a callback from the loading system. This'll be the menu having loaded a NEW menu
    public virtual void LoadMenuSceneCallback(loadedScene newScene, bool bSuccess)
    {
        
        switch (enPanelType)
        {
            case enInteractionType.NONE:
                RemoveSelfAndContents();
                break;
            case enInteractionType.BASE:
                gameObject.SetActive(false); //simply disable this menu;
                break;
            case enInteractionType.LOADED:
                RemoveSelfAndContents();
                break;
            default:
                RemoveSelfAndContents();
                break;                
        }
    }

    public virtual void RemoveSelfAndContents()
    {
        if (UIMenuHandler.Instance)
        {
            UIMenuHandler.Instance.UnloadMenu(gameObject);
        } else
        {
            Debug.LogError("Could not remove menu: " + gameObject.name + " as there was no UIMenuHandler Instance");
        }
    }

    public virtual void DoClose()
    {
        switch (enPanelType)
        {
            case enInteractionType.NONE:
                RemoveSelfAndContents();
                break;
            case enInteractionType.BASE:
                gameObject.SetActive(false); //simply disable this menu;
                break;
            case enInteractionType.LOADED:
                //RemoveSelfAndContents();
                //So that we don't get a flash we'll get this on the callback
                break;
            default:
                RemoveSelfAndContents();
                break;
        }

        switch (returnPanelType)
        {
            case enInteractionType.NONE:
                RemoveSelfAndContents();
                break;
            case enInteractionType.BASE:
                if (returnPanel_Scene.Length > 3)   //In theory we've got a scene to load, or something to look for here
                {
                    //We need to look for this in the UIMenuHandler
                }
                UIMenuHandler.Instance.OpenBaseScene(returnPanel_Scene, returnPanel_Button);
                break;
            case enInteractionType.LOADED:
                //We need to load a scene for this "return"
                UIMenuHandler.Instance.LoadMenuSceneAdditively(returnPanel_Scene, this, null);  //Do a dumb load to our other scene
                break;
            default:
                break;
        }
        
        //gameObject.SetActive(false); //Turn this panel off
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("Triangle") || Input.GetButtonDown("Circle"))
        {
            if (!bCanBeDismissed) { return; }   //We can't triangle out of this menu
            /*
            if (GameController.Instance)
            {
                GameController.Instance.PlayReturn();
            }*/
            if (bCanBeDismissed)
            {
                OnClose();
            }
        }
    }

    //Used when we've got a button that wants to send a command back to the level controller, and needs setup in its own scene
    public void CallFunctionOnLevelController(string functionName)
    {
        ((LevelController)LevelControllerBase.Instance).Invoke(functionName, 0f);
    }


    //Used when we've got a button that wants to send a command back to the game controller, and needs setup in its own scene
    public void CallFunctionOnGameController(string functionName)
    {
        //PROBLEM: We don't have a GameController to call against yet
        gameManager.Instance.Invoke(functionName, 0f);
    }

    public void setDescriptionText(string newText)
    {
        if (buttonDescriptionText)
        {
            buttonDescriptionText.text = newText;
        }
    }

    public void ConfirmSettingsChanged()
    {
        if (UISettingsHandler.Instance)
        {
            UISettingsHandler.Instance.ConfirmChanges();
        }
    }
}
