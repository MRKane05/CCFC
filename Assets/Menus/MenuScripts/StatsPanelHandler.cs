using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsPanelHandler : MonoBehaviour {
	public UI_CompareBar Weight;
	public float WeightMax = 200f;	//totally dunno here...
	public UI_CompareBar Armor;
	public float ArmorMax = 230f;
	public UI_CompareBar Agility;
	public float AgilityMax = 2.3f;
	public UI_CompareBar Speed;
	public float SpeedMax = 15f;
	public UI_CompareBar Accel;
	public float AccelMax = 80f;



	// Use this for initialization
	void Start () {
		SetStats(gameManager.Instance.SelectedAircraft);
	}

	public void SetStats(AircraftDescription thisAircraft)
    {
		//Set the sizes of the backing bars
		Weight.setBackingBarFill(thisAircraft.weight_max / WeightMax);
		Armor.setBackingBarFill(thisAircraft.armor_max / ArmorMax);
		Agility.setBackingBarFill(thisAircraft.agility_max / AgilityMax);
		Speed.setBackingBarFill(thisAircraft.speed_max / SpeedMax);
		Accel.setBackingBarFill(thisAircraft.accel_max / AccelMax);

		//Set the current values against the backing bars
		Weight.SetCurrentValue(thisAircraft.weight_current / WeightMax);
		Armor.SetCurrentValue(thisAircraft.armor_current / ArmorMax);
		Agility.SetCurrentValue(thisAircraft.agility_current / AgilityMax);
		Speed.SetCurrentValue(thisAircraft.speed_current / SpeedMax);
		Accel.SetCurrentValue(thisAircraft.accel_current / AccelMax);
	}

}
