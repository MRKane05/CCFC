using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIHelpers : MonoBehaviour {

    public static void SetSelectedButton(GameObject toThis)
    {
#if UNITY_EDITOR
        //Debug.Log("UIHelper Button Select: " + toThis);
#endif
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(toThis);
        EventSystem.current.SetSelectedGameObject(toThis);
    }

    public static GameObject FindChildByName(GameObject parent, string childName)
    {
        if (parent == null)
        {
            Debug.LogError("Parent GameObject is null.");
            return null;
        }

        foreach (Transform child in parent.transform)
        {
            // Check if this child matches the name
            if (child.name.Equals(childName))
                return child.gameObject;

            // Recursively search in this child's children
            GameObject result = FindChildByName(child.gameObject, childName);
            if (result != null)
                return result;
        }

        // No match found
        return null;
    }
}
