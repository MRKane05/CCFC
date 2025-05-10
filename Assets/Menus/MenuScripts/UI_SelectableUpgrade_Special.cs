using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_SelectableUpgrade_Special : UI_SelectableUpgradeBase {
    public override void SetChildSelected(bool bState, SelectableUpgradeType thisSelectableType, UI_SelectableUpgrade thisSelectable)
    {
        
        if (bState) {
            selectedUpgrade = thisSelectableType;
            statsPanel.SetSpecialComparison(thisSelectableType);
        } else
        {
            selectedUpgrade = nullUpgrade;
            statsPanel.SetCannonsComparison(nullUpgrade);
        }
        HandleRadioButtonFunction(thisSelectable, bState);
    }

    public override void ApplySelectedItem()
    {
        //First we need to check if we're over weight or anything like that
        if (false)
        {
            return;
        }

        gameManager.Instance.SelectedAircraft.weight_current += (selectedUpgrade.upgradEffect.finalWeight - gameManager.Instance.SelectedAircraft.AttachedSpecial.cannons_weight);
        //gameManager.Instance.SelectedAircraft.armor_current += selectedUpgrade.upgradEffect.finalArmor;
        gameManager.Instance.SelectedAircraft.agility_current += (selectedUpgrade.upgradEffect.finalAgility - gameManager.Instance.SelectedAircraft.AttachedSpecial.cannons_agility);
        gameManager.Instance.SelectedAircraft.speed_current += (selectedUpgrade.upgradEffect.finalSpeed - gameManager.Instance.SelectedAircraft.AttachedSpecial.cannons_speed);
        gameManager.Instance.SelectedAircraft.accel_current += (selectedUpgrade.upgradEffect.finalAccel - gameManager.Instance.SelectedAircraft.AttachedSpecial.cannons_accel);

        
        gameManager.Instance.SelectedAircraft.AttachedSpecial.cannons_accel = selectedUpgrade.upgradEffect.finalAccel;
        gameManager.Instance.SelectedAircraft.AttachedSpecial.cannons_agility = selectedUpgrade.upgradEffect.finalAgility;
        gameManager.Instance.SelectedAircraft.AttachedSpecial.cannons_autoaim_angle = selectedUpgrade.autoAimAngle;
        gameManager.Instance.SelectedAircraft.AttachedSpecial.cannons_bullet_range = selectedUpgrade.distance;
        //gameManager.Instance.SelectedAircraft.AttachedCannons.cannons_bullet_speed = selectedUpgrade. //We don't have bullet speed
        gameManager.Instance.SelectedAircraft.AttachedSpecial.cannons_damage = selectedUpgrade.damage;
        gameManager.Instance.SelectedAircraft.AttachedSpecial.cannons_name = selectedUpgrade.upgradeName;
        gameManager.Instance.SelectedAircraft.AttachedSpecial.cannons_refire_time = selectedUpgrade.fireRate;
        gameManager.Instance.SelectedAircraft.AttachedSpecial.cannons_speed = selectedUpgrade.upgradEffect.finalSpeed;
        gameManager.Instance.SelectedAircraft.AttachedSpecial.cannons_spread = selectedUpgrade.spread;
        gameManager.Instance.SelectedAircraft.AttachedSpecial.cannons_weight = selectedUpgrade.upgradEffect.finalWeight;

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
