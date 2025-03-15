using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Mission_MapSection;

[System.Serializable]
public class loadedScene
{
    public Scene scene;
    public PanelHandler callingPanel;
    public GameObject callingButton;
    public bool IsValid()
    {
        if (scene != null)
        {
            return scene.IsValid();
        }
        return false;
    }
    public loadedScene(Scene newScene, PanelHandler newCallingPanel, GameObject newCallingButton)
    {
        scene = newScene;
        callingPanel = newCallingPanel;
        callingButton = newCallingButton;
    }
}


//This script handles menu elements, loading menus as scenes and the chatter/functionality between them
public class UIMenuHandler : MonoBehaviour
{
    public List<loadedScene> loadedScenes = new List<loadedScene>();
    private Scene lastLoadedScene;
    public PanelHandler baseMenu;    //This will be our fallback reference menu
    private static UIMenuHandler instance = null;
    public static UIMenuHandler Instance { get { return instance; } }

    void Awake()
    {
        if (instance)
        {
            //Debug.Log("Somehow there's a duplicate UIMenuHandler in the scene");
            //Debug.Log(gameObject.name);
            DestroyImmediate(gameObject);    //Remove ourselves from the scene
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
    }

    //This is called by a menu that's marked as "Base" to set itself as the menu that the entire system can return back to
    public void AssertMenuAsBase(PanelHandler thisMenu)
    {
        //Debug.Log("Menu asserted as base: " + thisMenu.name);
        if (baseMenu) //we should do something about this...
        {

        }
        baseMenu = thisMenu;
    }

    public void OpenBaseScene(string targetSceneName, string targetButtonName)
    {
        //Kick off with the easy way first
        if (baseMenu)
        {
            baseMenu.gameObject.SetActive(true); //Just turn this back on again
            baseMenu.DoEnable(targetButtonName);
        }
    }

    //Function to unload menus that have been loaded into the scene. This'll be used with the menu close function
    public void UnloadMenu(GameObject targetMenu)
    {
        Scene menuScene = targetMenu.scene;
        List<int> removeEntries = new List<int>();
        if (menuScene.isLoaded)
        {
            SceneManager.UnloadSceneAsync(menuScene.name);
        }
        else
        {
            Debug.LogError("Menu scene is not loaded, cannot unload.");
        }
    }

    public void TransitionLoadScene(string sceneName)
    {
        //there'll be no menu stuff here, it's simply doing a load
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void setSelectScreenPromptText(enMissionType missionType)
    {
        switch (missionType)
        {
            case enMissionType.TURRET:
                gameManager.Instance.panelTitle = "Tailgunner";
                gameManager.Instance.panelContent = "Defend your bomber against enemy attacks while you fly to do a bombing run. Mission will fail if your bomber is shot down.";
                break;
            case enMissionType.BOMBING:
                gameManager.Instance.panelTitle = "Bombing Run";
                gameManager.Instance.panelContent = "Avoid anti aircraft fire and destroy all the buildings you can see. Mission will fail if you are destroyed or too many enemy buildings remain.";
                break;
            case enMissionType.STRAFING:
                gameManager.Instance.panelTitle = "Strafing Run";
                gameManager.Instance.panelContent = "Strafe and destroy enemy vehicles while avoiding anti aircraft fire. Mission will fail if you are destroyed or insufficient vehicles remain.";
                break;
            case enMissionType.BASEDEFENCE:
                gameManager.Instance.panelTitle = "Base Defence";
                gameManager.Instance.panelContent = "Defend friendly base against enemy bombers. Mission wil fail if too many bombs are dropped.";
                break;
            case enMissionType.BASEATTACK:
                gameManager.Instance.panelTitle = "Base Attack";
                gameManager.Instance.panelContent = "Escort enemy bombers as they conduct bombing runs on the enemy base. Mission will fail if too many bombers are shot down before they complete their run.";
                break;
            case enMissionType.SKIRMISH:
                gameManager.Instance.panelTitle = "Skirmish";
                gameManager.Instance.panelContent = "Vye for air superiority against waves of enemy aircraft.";
                break;
        }
    }

    // Function to load a scene additively and store its reference
    public void LoadMenuSceneAdditively(string sceneName, PanelHandler caller, GameObject callingButton)
    {
        StartCoroutine(LoadMenuSceneAsync(sceneName, caller, callingButton));
    }

    private IEnumerator LoadMenuSceneAsync(string sceneName, PanelHandler caller, GameObject callingButton)
    {
        // Begin loading the scene additively
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        // Wait until the scene loading is complete
        while (!asyncLoad.isDone)
        {
            yield return null;
        }


        // Once loaded, store the scene reference
        lastLoadedScene = SceneManager.GetSceneByName(sceneName);
        loadedScene newScene = new loadedScene(lastLoadedScene, caller, callingButton);

        //PROBLEM: We're getting a flash here while switching menus. I'm not sure how to resolve this within the limitations of Unity and what it's giving me

        if (caller != null)
        {
            caller.LoadMenuSceneCallback(newScene, true);
        }
    }
}