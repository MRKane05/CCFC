using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBase : MonoBehaviour {
	public Vector3 moveDir = Vector3.forward;
	public float moveSpeed = 15f;
	public float damage = 3f;
	public GameObject Instigator;
	public virtual void SetupProjectile(GameObject newInstigator, Vector3 newMoveDir, float newMoveSpeed, float newDamage)
	{
		Instigator = newInstigator;
		moveDir = newMoveDir;
		//We'll send through -1 if we want to keep our momentium 
		if (newMoveSpeed > 0)
		{
			moveSpeed = newMoveSpeed;
		}
		if (newDamage > 0)
		{
			damage = newDamage;
		}
	}

	public void Update()
	{
		DoUpdate();
	}
	public virtual void DoUpdate() {
		gameObject.transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

	//Upon collision with another GameObject, this GameObject will reverse direction
	private void OnTriggerEnter(Collider other)
	{
		DoOnTriggerEnter(other);
	}

	public virtual void DoOnTriggerEnter(Collider other) { 
		if (other.gameObject)
        {
			StrafingAircraftHandler ourPlayer = other.gameObject.GetComponent<StrafingAircraftHandler>();
			if (ourPlayer)
            {
				ourPlayer.TakeDamage(damage);
            }
        }
		Destroy(gameObject);	//Kill this particle regardless
	}
}
