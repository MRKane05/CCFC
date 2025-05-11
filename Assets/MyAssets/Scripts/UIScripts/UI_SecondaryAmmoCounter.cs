using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_SecondaryAmmoCounter : MonoBehaviour {

	public Text ammoCounterText;
    public float ammoCount = 0f;
    //PROBLEM: We need an icon by our ammo counter so that the player can recognise everything with ease
    public void setAmmoCount(float newAmmoCount)
    {
        ammoCount = newAmmoCount;   //I'm sure this could do with some sort of effect on it
        ammoCounterText.text = ammoCount.ToString();
    }
}
