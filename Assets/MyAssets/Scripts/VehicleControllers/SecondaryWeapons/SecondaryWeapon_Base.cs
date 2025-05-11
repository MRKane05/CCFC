using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondaryWeapon_Base : MonoBehaviour {
    public float ammo_max = 30;
    public float ammo_current = 30;

    public float refireRate = 0.5f;
    protected float nextRefireTime = 0f;

    public GameObject weaponOwner;

    //This will have an attached UI component
    //public AudioClip fireSound;
    protected AudioSource ourAudio;

    public void setAmmoAndMax(float newAmmo)
    {
        ammo_max = newAmmo;
        ammo_current = newAmmo;
        NGUI_Base.Instance.setSecondaryAmmoCount(ammo_current, true);
    }

    //So to allow the system to handle all sorts of different ammo volumes with a pickup we're setting an ammo percentage add here
    public void addAmmoPercent(float thisPercentage)
    {
        ammo_current = Mathf.Clamp(ammo_current + Mathf.RoundToInt(ammo_max * thisPercentage), 0, ammo_max);    //Add a rounded percentage
        NGUI_Base.Instance.setSecondaryAmmoCount(ammo_current, true);
    }

    void Start()
    {
        ourAudio = gameObject.GetComponent<AudioSource>();
        ammo_current = ammo_max;
        NGUI_Base.Instance.setSecondaryAmmoCount(ammo_current, true);
    }

    public bool UseAmmo(float ammount)
    {
        if (ammo_current < ammount)
        {
            NGUI_Base.Instance.setSecondaryAmmoCount(ammo_current, true);
            return false;   //We can't take this shot
        }

        ammo_current -= ammount;
        NGUI_Base.Instance.setSecondaryAmmoCount(ammo_current, true);
        return true;
    }

    public virtual void DoTapSecondary()
    {

        DoFire();
    }

    public virtual void DoHoldSecondary()
    {
        DoFire();
    }

    public virtual void DoFire()
    {
        //ourAudio.PlayOneShot(fireSound);
    }
}
