using UnityEngine;

/// <summary>
/// will help describe points on weapon models
/// </summary>
public class MeleeWeaponView : MonoBehaviour
{
	private WeaponMelee weaponMelee;

	public BoxCollider hitCollider;

	public void EnableHitCollider(WeaponMelee weaponMelee)
	{
		this.weaponMelee = weaponMelee;
		hitCollider.enabled = true;
	}

	public void DisableHitCollider()
	{
		hitCollider.enabled = false;
	}

	public void OnTriggerEnter(Collider other)
	{
		weaponMelee.OnColliderHit(other);
	}
}
