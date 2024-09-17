using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIHelpers : MonoBehaviour {

    public static void SetSelectedButton(GameObject toThis)
    {
        Debug.Log("UIHelper Button Select: " + toThis);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(toThis);
        EventSystem.current.SetSelectedGameObject(toThis);
    }

    public void DoBah()
    {
        //EventSystems
    }
}
