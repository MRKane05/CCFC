using UnityEngine;
using System.Collections;

//Generic bullet class...although I'm not sure if the attached gun should set the bullets or if ammo should be attached to the gun...?
public class Bullet : MonoBehaviour {
	Vector3 Movement;
	GameObject owner;
	string team, damageType;
	float damage;
	
	
	
	public void SetBulletAction (Vector3 nMovement, GameObject nOwner, float nDamage, string nDamageType, string nTeam) {
		Movement = nMovement; //Can control our speed etc.
		owner = nOwner; //Who fired this?
		damage = nDamage;
		team = nTeam;
		damageType = nDamageType;
	}
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.position += Movement*Time.deltaTime;
	}
}
