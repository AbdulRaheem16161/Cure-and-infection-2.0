using System.Collections;
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

	private Vector3 LastHitPoint;
	private float NextFireTime;

	private float accuracyModifer; //adjusted based on weapon definiton + how player is moving or firing
	private float recoilModifer; //adjusted based on weapon definiton + how player is moving or firing

	public override void InitializeItem(WeaponRangedDefinition definition, GameObject itemModel, int itemStack)
	{
		base.InitializeItem(definition, itemModel, itemStack);

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
	}

	#region equipping/unequipping weapon override methods (TODO: may need updating when finalizing how behaviour should work)
	public override void EquipItem(EquipmentHandler equipmentHandler, Transform parentTransform)
	{
		base.EquipItem(equipmentHandler, parentTransform);
		if (equipmentHandler.StatsHandler.NpcDefinition.Player)
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
		if (equipmentHandler.StatsHandler.NpcDefinition.Player)
		{
			equipmentHandler.InventoryHandler.AddNewItem(new(TypedDefinition.AmmoType, currentMagazineAmmo)); //return ammo in mag to inventory
		}
		else
		{
			//npc have unlimited ammo at the moment so doesnt need this
		}
	}
	#endregion

	private void Update()
	{
		HandleFireRate();
	}

	public void AimDownSight()
	{
		//ads and modify accuracy and move speed etc...
	}

	#region weapon shooting (TODO add sfx, vfx and animations + recoil and accuracy adjustments)
	public void Shoot()
	{
		if (MagazineEmpty) return;
		if (IsReloading) return;
		if (!CanShoot) return;

		currentMagazineAmmo--;

		Vector3 origin = WeaponView.MuzzlePoint.position;
		Vector3 direction = WeaponView.MuzzlePoint.forward;

		if (TryGetAccurateHit(origin, direction, out RaycastHit hit))
		{
			LastHitPoint = hit.point;

			if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
				damageable.RecieveDamage(TypedDefinition.Damage);
		}
		else
		{
			LastHitPoint = origin + direction * TypedDefinition.EffectiveRange;
		}

		SpawnVisualBullet(LastHitPoint);
	}
	#endregion

	#region handle fire rate
	private void HandleFireRate()
	{
		if (CanShoot) return;

		fireRateCooldownTimer -= Time.deltaTime;
		if (fireRateCooldownTimer > 0f) return;
		fireRateCooldownTimer = FireRateCooldown;
	}
	#endregion

	public void AdjustRecoil()
	{
		//update recoil while firing
	}

	public void AdjustAccuracy()
	{
		//update accuracy while firing
	}

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
		#region ReloadAmmo
		IsReloading = true;
		yield return new WaitForSeconds(TypedDefinition.ReloadTime);

		if (hasUnlimitedAmmo)
			currentMagazineAmmo = ammoGiver.GetAmmo(TypedDefinition.AmmoType, TypedDefinition.MagazineSize);
		else
			currentMagazineAmmo = ammoGiver.TakeAmmo(TypedDefinition.AmmoType, TypedDefinition.MagazineSize);

		IsReloading = false;
		#endregion
	}
	#endregion

	#region try get accurate hit
	private bool TryGetAccurateHit(Vector3 origin, Vector3 direction, out RaycastHit finalHit)
	{
		#region Summary
		/// <summary>
		/// Uses Raycast first for accuracy,
		/// then SphereCast as fallback aim assist
		/// </summary>
		#endregion

		#region TryGetAccurateHit

		finalHit = new RaycastHit();

		#region SphereCast
		RaycastHit[] hits = Physics.SphereCastAll(
			origin,
			TypedDefinition.BeamRadius,
			direction,
			TypedDefinition.EffectiveRange
		);

		float closestDistance = float.MaxValue;
		bool hitFound = false;

		for (int i = 0; i < hits.Length; i++)
		{
			RaycastHit hit = hits[i];

			float distance = Vector3.Distance(origin, hit.point);

			if (distance >= closestDistance)
				continue;

			closestDistance = distance;
			finalHit = hit;
			hitFound = true;
		}

		return hitFound;
		#endregion

		#endregion
	}
	#endregion

	#region visual bullet spawning + moving
	private void SpawnVisualBullet(Vector3 hitPoint)
	{
		#region Summary
		/// <summary>
		/// Spawns and animates a visual bullet toward the hit point
		/// </summary>
		#endregion

		#region SpawnVisualBullet

		GameObject bullet = Instantiate(TypedDefinition.BulletPrefab, 
			WeaponView.MuzzlePoint.position, Quaternion.LookRotation(hitPoint - WeaponView.MuzzlePoint.position));

		StartCoroutine(MoveBullet(bullet, hitPoint));
		#endregion
	}

	private IEnumerator MoveBullet(GameObject bullet, Vector3 hitPoint)
	{
		#region Summary
		/// <summary>
		/// Smoothly moves the bullet toward the target point
		/// </summary>
		#endregion

		#region MoveBullet

		Vector3 startPos = bullet.transform.position;
		float distance = Vector3.Distance(startPos, hitPoint);
		float travelTime = distance / TypedDefinition.BulletVisualSpeed;

		float t = 0f;

		while (t < 1f)
		{
			if (bullet == null)
				yield break;

			bullet.transform.position = Vector3.Lerp(startPos, hitPoint, t);
			t += Time.deltaTime / travelTime;

			yield return null;
		}

		if (bullet != null)
			Destroy(bullet);
		#endregion
	}
	#endregion
}
