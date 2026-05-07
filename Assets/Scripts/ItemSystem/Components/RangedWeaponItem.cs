using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor.Experimental.GraphView;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class RangedWeaponItem : Item<WeaponRangedDefinition>
{
	public RangedWeaponView WeaponView;

	public bool IsShooting { get; private set; }

	#region aiming down sights
	public AimState Aim;
	public enum AimState
	{
		hipfire, ads, EnterAds, ExitAds
	}
	public float enterAdsTimer;
	public float exitAdsTimer;
	#endregion

	#region reloading and magazine count fields
	public bool IsReloading { get; private set; }
	public bool MagazineFull => currentMagazineAmmo == TypedDefinition.MagazineSize;
	public bool MagazineEmpty => currentMagazineAmmo <= 0;
	public int currentMagazineAmmo;
	#endregion

	#region fire rate fields
	public bool canShoot;
	private bool CanShoot => fireRateCooldownTimer <= 0 && (Aim == AimState.hipfire || Aim == AimState.ads);
	public float FireRateCooldown;
	public float fireRateCooldownTimer;
	#endregion

	#region accuracy and recoil fields
	public float BulletSpreadMultiplier => CurrentBulletSpreadMultipler();
	public float accuracyModifier;

	public float RecoilMultiplier => CurrentRecoilMultipler();
	public Vector3 targetRecoil;
	public Vector3 currentRecoil;
	private int recoilIndex = 0;
	#endregion

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
		accuracyModifier = TypedDefinition.BaseSpread;
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
			accuracyModifier += 0.25f;
		else if (damageContext.ImpactType == DamageContext.HitImpact.knockback)
			accuracyModifier += 5f;
	}
	#endregion

	private void Update()
	{
		if (!IsInHands) return;

		canShoot = CanShoot;
		HandleFireRate();
		HandleBulletSpreadRecovery();
		NormalizeRecoil();
		EnterAimDownSightsTimer();
		ExitAimDownSightsTimer();
	}

	#region Handle Begin/End Adsing + transition states
	public void EnterAimDownSights()
	{
		if (Aim != AimState.hipfire) return;

		enterAdsTimer = TypedDefinition.AdsTime;
		Aim = AimState.EnterAds;
	}
	public void ExitAimDownSights()
	{
		if (Aim != AimState.ads) return;

		exitAdsTimer = TypedDefinition.AdsTime * 0.5f; //quicker
		Aim = AimState.ExitAds;
	}
	private void EnterAimDownSightsTimer()
	{
		if (Aim != AimState.EnterAds) return;

		enterAdsTimer -= Time.deltaTime;

		if (enterAdsTimer <= 0f)
			Aim = AimState.ads;
	}

	private void ExitAimDownSightsTimer()
	{
		if (Aim != AimState.ExitAds) return;

		exitAdsTimer -= Time.deltaTime;

		if (exitAdsTimer <= 0f)
			Aim = AimState.hipfire;
	}
	#endregion

	#region Handle Shooting and Stop Shooting Weapon Behaviour
	public void Shoot()
	{
		if (MagazineEmpty) { WeaponView.ChangeAnimation("DryFire", 0, true); return; }
		if (IsReloading) return;
		if (!CanShoot) return;

		IsShooting = true;
		currentMagazineAmmo--;
		fireRateCooldownTimer = FireRateCooldown;
		SimulateBulletSpread(); //uses raycast hitscan + visual bullet representation

		HandleBulletSpreadIncrease();
		AdjustRecoilOnShoot();

		if (currentMagazineAmmo == 0)
			WeaponView.ChangeAnimation("FireToEmpty", 0, true);
		else
			WeaponView.ChangeAnimation("Fire", 0, true);
	}
	public void StopShooting()
	{
		IsShooting = false;
		recoilIndex = 0;
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

	#region Handle Fire Rate
	private void HandleFireRate()
	{
		if (fireRateCooldownTimer > 0f)
			fireRateCooldownTimer -= Time.deltaTime;
	}
	#endregion

	#region handle Tracking Bullet spread and recoil multipliers
	private float CurrentBulletSpreadMultipler()
	{
		float multiplier = 1f;

		multiplier *= Aim == AimState.ads ? TypedDefinition.AdsBulletSpreadMultiplier : TypedDefinition.HipfireBulletSpreadMultiplier;

		return multiplier;
	}
	private float CurrentRecoilMultipler()
	{
		float multiplier = 1f;

		multiplier *= Aim == AimState.ads ? TypedDefinition.AdsRecoilMultiplier : TypedDefinition.HipfireRecoilMultiplier;

		return multiplier;
	}
	#endregion

	#region Handle Bullet Spread Changes
	private void HandleBulletSpreadIncrease()
	{
		accuracyModifier += TypedDefinition.SpreadIncreasePerShot * BulletSpreadMultiplier;
	}
	private void HandleBulletSpreadRecovery()
	{
		accuracyModifier -= Time.deltaTime;
		accuracyModifier = Mathf.Clamp(accuracyModifier, TypedDefinition.BaseSpread, TypedDefinition.MaxSpread);
	}
	#endregion

	#region Handle Physical gun recoil
	/// <summary>
	/// will need updating to handle player input and allow them to counteract recoil, something like this
	/// currentPitch += mouseY;			currentYaw += mouseX;
	/// modelReference.localRotation = Quaternion.Euler(currentPitch, currentYaw, 0f) * Quaternion.Euler(currentRecoil);
	/// </summary>
	public void AdjustRecoilOnShoot()
	{
		if (TypedDefinition.RecoilPattern.Count == 0) return;

		recoilIndex = Mathf.Min(recoilIndex, TypedDefinition.RecoilPattern.Count - 1);
		targetRecoil += TypedDefinition.RecoilPattern[recoilIndex] * RecoilMultiplier;
		recoilIndex++;
	}
	public void NormalizeRecoil()
	{
		targetRecoil = Vector3.Lerp(targetRecoil, Vector3.zero, Time.deltaTime * TypedDefinition.RecoilRecoveryRate);
		currentRecoil = Vector3.Lerp(currentRecoil, targetRecoil, Time.deltaTime * TypedDefinition.RecoilSnappiness);
		ModelReference.transform.localRotation = Quaternion.Euler(currentRecoil);
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
			Random.Range(-accuracyModifier, accuracyModifier),
			Random.Range(-accuracyModifier, accuracyModifier),
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
