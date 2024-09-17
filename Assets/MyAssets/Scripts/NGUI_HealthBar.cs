using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class NGUI_HealthBar : MonoBehaviour {

	public UnityEngine.UI.Image healthBar; //why do we need this?
	//public UISprite healthBar;
	//public Vector2 healthBarFull = new Vector2(225F, 16F); //our bar when it's full size
	public float healthProp; //would be cool if this lerped down...
	
	public void setHealthProp(float newProp) {
		healthProp = newProp;
		//healthBar.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
		healthBar.fillAmount = newProp;
		//= Mathf.RoundToInt(healthBarFull[0]*healthProp); //resize our health bar.
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
