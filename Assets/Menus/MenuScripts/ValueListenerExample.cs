using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValueListenerExample : MonoBehaviour {

	// Use this for initialization
	IEnumerator Start () {
		while (UISettingsHandler.Instance == null)
        {
			yield return null;
        }
		UISettingsHandler.Instance.OnSettingsChanged.AddListener(UpdateInternalSettings);
	}
	
	void UpdateInternalSettings()
    {
		Debug.Log("Checking settings changed");
	}

	//We need to remember to remove this from our settings watcher
	void OnDestroy()
    {
		UISettingsHandler.Instance.OnSettingsChanged.RemoveListener(UpdateInternalSettings);
	}
}
