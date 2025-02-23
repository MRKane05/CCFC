using UnityEngine;
using System.Collections;

//Handles messages sent to the player whenever something important happens
//We'll need a sound that's played here also
public class NGUI_AIMessage : NGUI_GameMessage {
	public UnityEngine.UI.Text mesName, mesText; //character name, and character text
	public UnityEngine.UI.Image aiImage; //might have to do something interesting with this

	public void setMessage(string newName, string newText, Color teamColor) {
		mesName.text = newName;
		mesName.color = teamColor;
		mesText.text = newText;

		pingTime = Time.time;
		attachedPanel.alpha = 1F; //put everything on!
	}
}
