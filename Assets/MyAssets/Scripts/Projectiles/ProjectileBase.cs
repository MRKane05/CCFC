using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBase : MonoBehaviour {
	public Vector3 moveDir = Vector3.forward;
	public float moveSpeed = 15f;
	public float damage = 3f;

	public void SetupProjectile(Vector3 newMoveDir, float newMoveSpeed, float newDamage)
    {
		moveDir = newMoveDir;
		moveSpeed = newMoveSpeed;
		damage = newDamage;
    }

	public void Update()
    {
		gameObject.transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

	//Upon collision with another GameObject, this GameObject will reverse direction
	private void OnTriggerEnter(Collider other)
	{
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
