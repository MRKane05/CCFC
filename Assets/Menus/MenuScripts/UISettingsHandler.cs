using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//This script will house all of our settings values and be referenced throughout the rest of the system
public class UISettingsHandler : MonoBehaviour {
	private static UISettingsHandler instance = null;
	public static UISettingsHandler Instance { get { return instance; } }

    Dictionary<string, int> settings_Int = new Dictionary<string, int>();
    Dictionary<string, bool> settings_Bool = new Dictionary<string, bool>();
    Dictionary<string, float> settings_Float = new Dictionary<string, float>();

    [System.Serializable]
    public class SettingsChangedEvent : UnityEvent
    {
    }

    public SettingsChangedEvent OnSettingsChanged;

    void Awake()
    {
        if (instance)
        {
            //Debug.Log("Somehow there's a duplicate UISettingsHandler in the scene");
            //Debug.Log(gameObject.name);
            Destroy(gameObject);    //Remove ourselves from the scene
        }
        else
        {

            instance = this;
        }
    }

    //This command must be called when exiting the settings menu to assure that the system sends through information
    //to all scripts needing updates pertaining to settings AND also to save the player preferences
    public void ConfirmChanges()
    {
        OnSettingsChanged.Invoke();
        PlayerPrefs.Save();
    }

    #region Int Handlers
    public void addSettingsInt(string valueName, int value)
    {
        if (settings_Int.ContainsKey(valueName))
        {
            settings_Int[valueName] = value;
        } else
        {
            settings_Int.Add(valueName, value);
        }
        PlayerPrefs.SetInt(valueName, value);
    }

    public int getSettingInt(string valueName)
    {
        if (settings_Int.ContainsKey(valueName))
        {
            return settings_Int[valueName];
        }
        if (PlayerPrefs.HasKey(valueName))
        {
            int keyValue = PlayerPrefs.GetInt(valueName);
            addSettingsInt(valueName, keyValue);
            return keyValue;
        }
        return 0;
    }
    #endregion
    #region Bool handlers
    public void addSettingsBool(string valueName, bool value)
    {
        if (settings_Bool.ContainsKey(valueName))
        {
            settings_Bool[valueName] = value;
        }
        else
        {
            settings_Bool.Add(valueName, value);
        }
        //Player prefs can't set a bool. Lets not do that BUUUTTTT a bool can be stored as an int. I doubt we'll ever use this functionality
        int boolValue = value ? 1:0;
        PlayerPrefs.SetInt(valueName, boolValue);
    }

    public bool getSettingBool(string valueName)
    {
        if (settings_Bool.ContainsKey(valueName))
        {
            return settings_Bool[valueName];
        }
        if (PlayerPrefs.HasKey(valueName))
        {
            bool keyValue = PlayerPrefs.GetInt(valueName) > 0;
            addSettingsBool(valueName, keyValue);
            return keyValue;
        }
        return false;
    }
    #endregion
    #region float handlers

    public void addSettingsFloat(string valueName, float value)
    {
        if (settings_Float.ContainsKey(valueName))
        {
            settings_Float[valueName] = value;
        }
        else
        {
            settings_Float.Add(valueName, value);
        }
        PlayerPrefs.SetFloat(valueName, value);
    }

    public float getSettingFloat(string valueName)
    {
        if (settings_Float.ContainsKey(valueName))
        {
            return settings_Float[valueName];
        }
        if (PlayerPrefs.HasKey(valueName))
        {
            float keyValue = PlayerPrefs.GetFloat(valueName);
            addSettingsFloat(valueName, keyValue);
            return keyValue;
        }
        return 0.5f;    //Return the average value
    }
    #endregion
}
