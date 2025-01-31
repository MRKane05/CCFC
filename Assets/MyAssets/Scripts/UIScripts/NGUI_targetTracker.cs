using UnityEngine;
using System.Collections;

//used for anything that'll fade out over distance of tracking.
public class NGUI_targetTracker : NGUI_ObjectTracker {

	float visibleDepth=17; //at what point is this visible from?

	//I assume here that we want to do something with this
	//Actually when I look more at it I'm not doing anything with this actually
	public override GameObject trackObject {
		get { return trackingObject; }
		set { trackingObject = value; }
	}

	//I want these to fade for us, so lets set all of that up.
	Color m_bracketColor = new Color(1, 0, 0, 0.8F);
	Color m_bracketFaded = new Color(1,0,0,0);
	public Color m_bracketCurrent = new Color(1, 0, 0, 0.8F);
	float bracketFade = 1, priorBracketFade=-1; //our graduation

	public virtual Color bracketColor {
		get { return bracketColor; }
		set { m_bracketColor = new Color(value[0], value[0], value[0], 0.8F); 
			m_bracketFaded = new Color(value[0], value[0], value[0], 0F);
			m_bracketCurrent = Color.Lerp(m_bracketFaded, m_bracketColor, bracketFade);
		}
	}

	//UIWidget[] ourWidgets;
	UnityEngine.UI.Image[] ourWidgets;

	void Start() {
		ourWidgets = gameObject.GetComponentsInChildren<UnityEngine.UI.Image>();
	}
	
	void LateUpdate () {
		if (trackingObject!=null) {
			depthPositionObject();
			foreach (Transform thisTran in transform)
				thisTran.gameObject.SetActive(true);
		}
		else { //we need to annull this objects visibility
			foreach (Transform thisTran in transform)
				thisTran.gameObject.SetActive(false);
		}
	}
	
	
	//position our sight visually.
	public void depthPositionObject() {
		// Get screen location of node
		Vector3 screenPos = gameCamera.WorldToScreenPoint(trackingObject.transform.position);
		
		if (screenPos.z > visibleDepth) { //this is true.
			bracketFade  = Mathf.Lerp (bracketFade, 1, Time.deltaTime*4F);
		}
		else {
			bracketFade  = Mathf.Lerp (bracketFade, 0, Time.deltaTime*4F);
		}

		m_bracketCurrent = Color.Lerp (m_bracketFaded, m_bracketColor, bracketFade);

		foreach (UnityEngine.UI.Image thisWidget in ourWidgets) {
			thisWidget.color = m_bracketCurrent;
			//thisWidget.enabled = (screenPos.z > 0); //set it's activity
		}


		/*
		foreach (Transform thisTran in transform)
			thisTran.gameObject.SetActive(screenPos.z > 0);

		foreach (Transform thisTran in transform)
			thisTran.gameObject.SetActive(screenPos.z > visibleDepth); //checker to see if this is visible
		*/

		screenPos.z = 0F;
		
		// Move to node
		transform.position = NGUICamera.ScreenToWorldPoint(screenPos);
	}
}
