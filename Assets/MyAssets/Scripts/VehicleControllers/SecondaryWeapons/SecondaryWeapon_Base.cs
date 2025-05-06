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

    void Start()
    {
        ourAudio = gameObject.GetComponent<AudioSource>();
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
