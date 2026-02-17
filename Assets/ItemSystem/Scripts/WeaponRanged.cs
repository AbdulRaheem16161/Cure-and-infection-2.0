using UnityEngine;

public class WeaponRanged : Item<WeaponRangedDefinition>
{
	private WeaponRangedDefinition weaponDefinition;

	private int currentMagazineAmmo; //track mag ammo count at runtime
	private float accuracyModifer; //adjusted based on weapon definiton + how player is moving or firing
	private float recoilModifer; //adjusted based on weapon definiton + how player is moving or firing

	public override void InitializeItem(WeaponRangedDefinition definition, int itemStack)
	{
		base.InitializeItem(definition, itemStack);
		weaponDefinition = definition;

		// weapon-specific setup here
	}

	public void EquipWeapon()
	{
		//hold in hands
	}

	public void UnEquipWeapon()
	{
		//holster
	}

	public void AimDownSight()
	{
		//ads and modify accuracy and move speed etc...
	}

	//example method behaviours
	public void Shoot()
	{
		///<summery>
		/// check if can shoot
		/// shoot gun adjust recoil and accuracy based on weapon definition,
		/// lower mag ammo, 
		/// create relevent sfx and vfx
		///<summery>
	}

	public void AdjustRecoil()
	{
		//update recoil while firing
	}

	public void AdjustAccuracy()
	{
		//update accuracy while firing
	}

	public void Reload()
	{
		///<summery>
		/// refil mag, wait for reload time
		/// create relevent sfx and vfx
		///<summery>
	}

	public bool HasAmmo()
	{
		//check for ammo in inventory etc for reloading
		return true;
	}

	public bool HasAmmoInMagazine()
	{
		if (currentMagazineAmmo == 0)
			return false;
		else
			return true;
	}
}
