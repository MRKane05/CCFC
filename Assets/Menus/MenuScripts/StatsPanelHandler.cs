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

	public UI_CompareBar FireRate;	//Inverted stat
	public float FireRateMax = 4f;
	public UI_CompareBar Accuracy;  //Another nverted stat
	public float AccuracyMax = 0.1f;    //I guess?
	public UI_CompareBar Damage;    //Not inverted :)
	public float DamageMax = 5f;	//Which is a mental amount of damage


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

		FireRate.SetCurrentValue(thisAircraft.AttachedCannons.cannons_refire_time / FireRateMax);
		Accuracy.SetCurrentValue(thisAircraft.AttachedCannons.cannons_spread / AccuracyMax);
		Damage.SetCurrentValue(thisAircraft.AttachedCannons.cannons_damage / DamageMax);
	}

	public bool SetComparison(UpgradePathEffects currentUpgradePath, float lerpFactor)
    {
		lerpFactor -= currentUpgradePath.upgradeLevel;
		lerpFactor /= 5f;

		if (gameManager.Instance.SelectedAircraft.weight_current + currentUpgradePath.finalWeight * lerpFactor > gameManager.Instance.SelectedAircraft.weight_max)
        {
			return false;
        }

		//Because this is a smooth gradient we can apply values to the airframe and take differences with this
		Weight.DoCompareValue(currentUpgradePath.finalWeight * lerpFactor / WeightMax);
		Armor.DoCompareValue(currentUpgradePath.finalArmor * lerpFactor / ArmorMax);
		Agility.DoCompareValue(currentUpgradePath.finalAgility * lerpFactor / AgilityMax);
		Speed.DoCompareValue(currentUpgradePath.finalSpeed * lerpFactor / SpeedMax);
		Accel.DoCompareValue(currentUpgradePath.finalAccel * lerpFactor / AccelMax);

		return true;
    }

	public bool SetCannonsComparison(SelectableUpgradeType thisSelectableType)
    {
		//except this isn't quite the same as far as comparisons go (because of course it's not)
		//bool bNormalCompare = SetComparison(thisSelectableType.upgradEffect, 5f);
		Weight.DoCompareValue((thisSelectableType.upgradEffect.finalWeight - gameManager.Instance.SelectedAircraft.AttachedCannons.cannons_weight) / WeightMax);
		//Armor.DoCompareValue((thisSelectableType.upgradEffect.finalArmor - thisSelectableType.upgradEffect.finalArmor) / ArmorMax);
		//Armor.DoCompareValue(currentUpgradePath.finalArmor * lerpFactor / ArmorMax);
		
		Agility.DoFlatCompare((gameManager.Instance.SelectedAircraft.agility_current - gameManager.Instance.SelectedAircraft.AttachedCannons.cannons_agility + thisSelectableType.upgradEffect.finalAgility) / AgilityMax);
		Speed.DoFlatCompare((gameManager.Instance.SelectedAircraft.speed_current - gameManager.Instance.SelectedAircraft.AttachedCannons.cannons_speed + thisSelectableType.upgradEffect.finalSpeed) / SpeedMax);
		Accel.DoFlatCompare((gameManager.Instance.SelectedAircraft.accel_current - gameManager.Instance.SelectedAircraft.AttachedCannons.cannons_accel + thisSelectableType.upgradEffect.finalAccel) / AccelMax);

		//Accel.DoCompareValue(currentUpgradePath.finalAccel * lerpFactor / AccelMax);

		FireRate.DoFlatCompare(thisSelectableType.fireRate / FireRateMax);
		Accuracy.DoFlatCompare(thisSelectableType.spread / AccuracyMax);
		Damage.DoFlatCompare(thisSelectableType.damage / DamageMax);


		return true;
    }
}
