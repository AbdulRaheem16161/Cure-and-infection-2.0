using System.Collections;
using System.Threading;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class WeaponRanged : Item<WeaponRangedDefinition>
{
	public RangedWeaponView WeaponView;

	public bool IsReloading { get; private set; }
	public bool MagazineFull => currentMagazineAmmo == TypedDefinition.MagazineSize;
	public bool MagazineEmpty => currentMagazineAmmo <= 0;
	public int currentMagazineAmmo; //track mag ammo count at runtime

	public bool CanShoot => fireRateCooldownTimer <= 0;

	public float FireRateCooldown;
	public float fireRateCooldownTimer;

	public float accuracyModifer;
	private float recoilModifer; //adjusted based on weapon definiton + how player is moving or firing

	#region Initialize Item Override
	public override void InitializeItem(WeaponRangedDefinition definition, int itemStack)
	{
		base.InitializeItem(definition, itemStack);

		//weapon-specific setup here
		if (ModelReference == null)
		{
			Debug.LogError($"{TypedDefinition.ItemName} lacks a needed (or temporary) model with component: {nameof(RangedWeaponView)}");
			return;
		}

		WeaponView = ModelReference.GetComponent<RangedWeaponView>();

		if (WeaponView == null)
		{
			Debug.LogError($"{TypedDefinition.ItemName} missing component {nameof(RangedWeaponView)} in its ModelReference.\n");
			return;
		}

		FireRateCooldown = 60 / (float)TypedDefinition.FireRateRPM;
		accuracyModifer = TypedDefinition.HipFireSpread;
	}
	#endregion

	#region equipping/unequipping weapon override methods (TODO: may need updating when finalizing how behaviour should work)
	public override void EquipItem(EquipmentHandler equipmentHandler, Transform parentTransform)
	{
		base.EquipItem(equipmentHandler, parentTransform);
		WeaponView.ChangeAnimation("Equip", 0, true);

		if (equipmentHandler.StatsHandler.Definition.Player)
		{
			//player manually reloads when unholstering equipped weapon
		}
		else
		{
			Reload(equipmentHandler.InventoryHandler, true); //npcs auto reload for free when equipping weapon
		}
	}
	public override void UnEquipItem(EquipmentHandler equipmentHandler)
	{
		base.UnEquipItem(equipmentHandler);
		WeaponView.ChangeAnimation("Unequip", 0, true);

		if (equipmentHandler.StatsHandler.Definition.Player)
		{
			equipmentHandler.InventoryHandler.AddNewItem(new(TypedDefinition.AmmoType, currentMagazineAmmo)); //return ammo in mag to inventory
		}
		else
		{
			//npc have unlimited ammo at the moment so doesnt need this
		}
	}
	#endregion

	#region on owner hit event listener override
	public override void OnHit(DamageContext damageContext)
	{
		if (damageContext.ImpactType == DamageContext.HitImpact.flinch)
			accuracyModifer += 0.25f;
		else if (damageContext.ImpactType == DamageContext.HitImpact.knockback)
			accuracyModifer += 5f;
	}
	#endregion

	private void Update()
	{
		if (!IsInHands) return;
		HandleFireRate();
		HandleBulletAccuracyRecovery();
	}

	public void AimDownSight()
	{
		//ads and modify accuracy and move speed etc...
	}

	#region weapon shooting (TODO add sfx, vfx and animations + recoil and accuracy adjustments)
	public void Shoot()
	{
		if (MagazineEmpty) { WeaponView.ChangeAnimation("DryFire", 0, true); return; }
		if (IsReloading) return;
		if (!CanShoot) return;

		currentMagazineAmmo--;
		accuracyModifer += TypedDefinition.SpreadIncreasePerShot;
		SimulateBulletSpread(); //uses raycast hitscan + visual bullet representation

		if (currentMagazineAmmo == 0)
			WeaponView.ChangeAnimation("FireToEmpty", 0, true);
		else
			WeaponView.ChangeAnimation("Fire", 0, true);
	}
	#endregion

	#region reloading (TODO add sfx, vfx and animations)
	public void Reload(IAmmoGiver ammoGiver, bool hasUnlimitedAmmo)
	{
		if (MagazineFull) return;
		if (IsReloading) return;

		if (!hasUnlimitedAmmo && !ammoGiver.AmmoAvailable(TypedDefinition.AmmoType)) return; //no ammo in inventory

		StartCoroutine(ReloadAmmo(ammoGiver, hasUnlimitedAmmo));
	}

	private IEnumerator ReloadAmmo(IAmmoGiver ammoGiver, bool hasUnlimitedAmmo)
	{
		WeaponView.ChangeAnimation("Reload", 0, true);

		IsReloading = true;
		yield return new WaitForSeconds(TypedDefinition.ReloadTime);

		if (hasUnlimitedAmmo)
			currentMagazineAmmo = ammoGiver.GetAmmo(TypedDefinition.AmmoType, TypedDefinition.MagazineSize);
		else
			currentMagazineAmmo = ammoGiver.TakeAmmo(TypedDefinition.AmmoType, TypedDefinition.MagazineSize);

		IsReloading = false;
	}
	#endregion

	public void AdjustRecoil()
	{
		//update recoil while firing
	}

	#region Handle Fire Rate
	private void HandleFireRate()
	{
		if (CanShoot) return;

		fireRateCooldownTimer -= Time.deltaTime;
		if (fireRateCooldownTimer > 0f) return;
		fireRateCooldownTimer = FireRateCooldown;
	}
	#endregion

	#region Handle Bullet Accuracy Recovery
	private void HandleBulletAccuracyRecovery()
	{
		accuracyModifer -= Time.deltaTime;
		accuracyModifer = Mathf.Clamp(accuracyModifer, TypedDefinition.AdsSpread, TypedDefinition.MaxSpread);
	}
	#endregion

	#region simulate bullet spread
	private void SimulateBulletSpread()
	{
		Vector3 startPos = WeaponView.MuzzlePoint.position;
		Vector3 direction = WeaponView.MuzzlePoint.forward;
		float distance = TypedDefinition.EffectiveRange;

		//apply random spread
		direction = Quaternion.Euler(
			Random.Range(-accuracyModifer, accuracyModifer),
			Random.Range(-accuracyModifer, accuracyModifer),
			0) * direction;

		if (Physics.Raycast(startPos, direction, out RaycastHit hit, 
			TypedDefinition.EffectiveRange, LayerMask.GetMask("CharacterHitbox", "Environment")))
		{
			//handle hit
			if (hit.collider.TryGetComponent(out HitCollider hitCollider))
				hitCollider.OnHit(TypedDefinition.Damage, TypedDefinition.ImpactType, CurrentOwner);

			distance = hit.distance;
		}

		SpawnVisualBullet(startPos, direction, distance);
	}
	#endregion

	#region visual bullet spawning + moving
	private void SpawnVisualBullet(Vector3 startPos, Vector3 direction, float maxDistance)
	{
		GameObject bullet = Instantiate(TypedDefinition.BulletPrefab, startPos, Quaternion.LookRotation(direction));
		StartCoroutine(MoveBullet(bullet, direction, maxDistance));
	}

	private IEnumerator MoveBullet(GameObject bullet, Vector3 direction, float maxDistance)
	{
		float traveled = 0f;
		float speed = TypedDefinition.BulletVisualSpeed;

		direction.Normalize(); // make sure it's a unit vector

		while (bullet != null && traveled < maxDistance)
		{
			float step = speed * Time.deltaTime;
			bullet.transform.position += direction * step;
			traveled += step;

			yield return null;
		}

		if (bullet != null)
			Destroy(bullet);
	}
	#endregion

	#region Editor Debug Options
	public void ResetAnimation()
	{
		WeaponView.ResetAnimation();
	}
	public void PlayAnimation(string animation)
	{
		WeaponView.PlayAnimation(animation);
	}
	#endregion
}
