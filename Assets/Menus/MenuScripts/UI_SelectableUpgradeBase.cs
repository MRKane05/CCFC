using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SelectableUpgradeType
{
	public string upgradeName = "Standard";
	public float unlockCost = 120f;
	public float applyCost = 10f;
	public bool bIsUnlocked = false;

	public float fireRate = 0.2f;
	public float spread = 0.01f;    //will need to have the bar go backwards for higher spreads...
    public float ammoMax = 60f;
    public float ammoRefil = 20f;   //How much ammo will be refilled when we collect a refil?
	public float damage = 1f;
	public float autoAimAngle = 7f; //This will need to change depending on the spread value so that it doesn't seem as accurate for the player
	public float distance = 50f; //Just in case we want to include this also

	public UpgradePathEffects upgradEffect = new UpgradePathEffects(15, 20, 0, -0.1f, -0.5f, -3f);  //What's the effect of applying this particular upgrade
}


//A grid layout for selectable upgrades, will need communication to allow for radio button selection behavior
public class UI_SelectableUpgradeBase : MonoBehaviour {

	public List<SelectableUpgradeType> CurrentUpgrades = new List<SelectableUpgradeType>();

	public UI_SelectableUpgrade[] ChildSelectables;

    public SelectableUpgradeType nullUpgrade;

    public StatsPanelHandler statsPanel;

    protected SelectableUpgradeType selectedUpgrade;

    // Use this for initialization
    void Start () {
		ChildSelectables = gameObject.GetComponentsInChildren<UI_SelectableUpgrade>();

		//Go through and set each of our selectable panels to one of the assigned types
		for (int i=0; i<ChildSelectables.Length; i++)
        {
			ChildSelectables[i].parentPanel = this;
			if (CurrentUpgrades.Count > i)
			{
				ChildSelectables[i].SetUpgrade(CurrentUpgrades[i]);
			} else
            {
				ChildSelectables[i].SetUpgrade(null);
            }
        }
	}

	//Called from children when the user selects one
	public virtual void SetChildSelected(bool bState, SelectableUpgradeType thisSelectableType, UI_SelectableUpgrade thisSelectable)
    {
        selectedUpgrade = thisSelectableType;
        if (bState)
        {
            statsPanel.SetCannonsComparison(thisSelectableType);
        } else
        {
            statsPanel.SetCannonsComparison(nullUpgrade);
        }
        HandleRadioButtonFunction(thisSelectable, bState);
    }

    public virtual void HandleRadioButtonFunction(UI_SelectableUpgrade thisSelectable, bool bSelectedState)
    {
        for (int i = 0; i < ChildSelectables.Length; i++)
        {
            if (ChildSelectables[i] != thisSelectable)
            {
                ChildSelectables[i].SetCheckSelected(false); //Make sure we turn this off
            } else {
                ChildSelectables[i].SetCheckSelected(bSelectedState); //Make sure we turn this off
            }
        }
    }

	public virtual void ApplySelectedItem()
    {
        //First we need to check if we're over weight or anything like that
        if (false)
        {
            return;
        }

        gameManager.Instance.SelectedAircraft.weight_current += (selectedUpgrade.upgradEffect.finalWeight - gameManager.Instance.SelectedAircraft.AttachedCannons.cannons_weight);
        //gameManager.Instance.SelectedAircraft.armor_current += (selectedUpgrade.upgradEffect.finalArmor = gameManager.Instance.SelectedAircraft;  //Cannons/etc. shouldn't affect armor
        gameManager.Instance.SelectedAircraft.agility_current += (selectedUpgrade.upgradEffect.finalAgility - gameManager.Instance.SelectedAircraft.AttachedCannons.cannons_agility);
        gameManager.Instance.SelectedAircraft.speed_current += (selectedUpgrade.upgradEffect.finalSpeed - gameManager.Instance.SelectedAircraft.AttachedCannons.cannons_speed);
        gameManager.Instance.SelectedAircraft.accel_current += (selectedUpgrade.upgradEffect.finalAccel - gameManager.Instance.SelectedAircraft.AttachedCannons.cannons_accel);

        gameManager.Instance.SelectedAircraft.AttachedCannons.cannons_accel = selectedUpgrade.upgradEffect.finalAccel;
        gameManager.Instance.SelectedAircraft.AttachedCannons.cannons_agility = selectedUpgrade.upgradEffect.finalAgility;
        gameManager.Instance.SelectedAircraft.AttachedCannons.cannons_autoaim_angle = selectedUpgrade.autoAimAngle;
        gameManager.Instance.SelectedAircraft.AttachedCannons.cannons_bullet_range = selectedUpgrade.distance;
        //gameManager.Instance.SelectedAircraft.AttachedCannons.cannons_bullet_speed = selectedUpgrade. //We don't have bullet speed
        gameManager.Instance.SelectedAircraft.AttachedCannons.cannons_damage = selectedUpgrade.damage;
        gameManager.Instance.SelectedAircraft.AttachedCannons.cannons_name = selectedUpgrade.upgradeName;
        gameManager.Instance.SelectedAircraft.AttachedCannons.cannons_refire_time = selectedUpgrade.fireRate;
        gameManager.Instance.SelectedAircraft.AttachedCannons.cannons_speed = selectedUpgrade.upgradEffect.finalSpeed;
        gameManager.Instance.SelectedAircraft.AttachedCannons.cannons_spread = selectedUpgrade.spread;
        gameManager.Instance.SelectedAircraft.AttachedCannons.cannons_weight = selectedUpgrade.upgradEffect.finalWeight;

        //Do money stuff

        //And finally we need to set our stats after the upgrade
        statsPanel.SetStats(gameManager.Instance.SelectedAircraft);

        //And of course we need to do the money thing!
        /*
        if (ourDisplayBar.totalUpgradeCost > 0) //because the lazy system can be negative when undoing upgrades
        {
            gameManager.Instance.playerStats.money -= ourDisplayBar.totalUpgradeCost;
        }*/
    }
}
