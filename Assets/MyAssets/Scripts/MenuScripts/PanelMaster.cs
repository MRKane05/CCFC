using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Insert some sort of clever way to handle our panels
public class PanelMaster : MonoBehaviour {
	private static PanelMaster instance = null;
	public static PanelMaster Instance { get { return instance; } }

	public List<PanelHandler> OpenPanels = new List<PanelHandler>();

	// Use this for initialization
	void Start () {
		if (instance)
		{
			Debug.Log("Duplicate attempt to create PanelMaster");
			Debug.Log(gameObject.name);
			Destroy(this);
			return; //cancel this
		}

		instance = this;
		DontDestroyOnLoad(this); //this is about the only thing that's not cycled around.
	}

	public void AddOpenPanel(PanelHandler thisPanel)
    {
		OpenPanels.Add(thisPanel);
    }

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("Triangle") || Input.GetButtonDown("Circle"))
		{
			//See if we've got a panel that's open at the end of our list, and close it then remove it from the list
			if (OpenPanels.Count > 0)
			{
				//Grab the one off the end of our list
				PanelHandler endPanel = OpenPanels[OpenPanels.Count - 1];
				endPanel.OnClose();
				OpenPanels.Remove(endPanel);
			}
		}
	}

	//This'll callback every time a panel is closed at the moment :/
	public void CheckPanelRemoved(PanelHandler thisPanel)
    {
		if (OpenPanels.Contains(thisPanel))
        {
			OpenPanels.Remove(thisPanel);
        }
    }
}
