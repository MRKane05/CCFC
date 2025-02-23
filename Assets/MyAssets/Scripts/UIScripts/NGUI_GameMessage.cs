using UnityEngine;
using System.Collections;

//Handles messages sent to the player whenever something important happens
//We'll need a sound that's played here also
public class NGUI_GameMessage : MonoBehaviour {

	protected float stayTime = 10; //how long does this stay on the screen for? (assuming we're not touched out)
	protected float fadeTime = 0.4f; //seriousy snappy!
	protected float pingTime =-float.MaxValue; //when was this activated?

	//and with this we begin the transfer over to the Unity GUI...
	protected CanvasGroup attachedPanel;
	//UIPanel attachedPanel;
	UnityEngine.UI.Text messageLabel;
	//UILabel messageLabel;

	// Use this for initialization
	void Start () {
		attachedPanel = gameObject.GetComponent<CanvasGroup>();
		messageLabel = gameObject.GetComponentInChildren<UnityEngine.UI.Text>();
	}

	public void setMessage(string newMessage) {
		if (messageLabel!=null) {
			messageLabel.text = newMessage;
		}

		pingTime = Time.time;
		attachedPanel.alpha = 1F; //put everything on!
	}

	public void cancelMessage() { //called from the button which we just pressed...
		pingTime = Time.time - stayTime; //as simple as that
	}

	void Update () {
		//handle our fade stuff...
		if (Time.time > pingTime + stayTime) {
			attachedPanel.alpha = Mathf.Clamp01(1f-(Time.time-pingTime-stayTime)/fadeTime);
		}
	}
}
