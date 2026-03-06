using Game.Core;
using Game.MyNPC;
using Game.MyPlayer;
using System.Collections;
using temp;
using UnityEngine;

public class WeaponRanged : Item<WeaponRangedDefinition>
{
	[SerializeField] private WeaponRangedDefinition weaponDefinition;
	public WeaponRangedDefinition WeaponDefinition => weaponDefinition;

	public bool IsReloading { get; private set; }
	public bool MagazineFull => currentMagazineAmmo == WeaponDefinition.MagazineSize;
	public bool MagazineEmpty => currentMagazineAmmo <= 0;
	public int currentMagazineAmmo; //track mag ammo count at runtime

	public bool CanShoot => fireRateCooldownTimer <= 0;
	public float FireRateCooldown;
	public float fireRateCooldownTimer;

	private Vector3 LastHitPoint;
	private float NextFireTime;

	private float accuracyModifer; //adjusted based on weapon definiton + how player is moving or firing
	private float recoilModifer; //adjusted based on weapon definiton + how player is moving or firing

	public WeaponView WeaponView { get; private set; }

	public override void InitializeItem(WeaponRangedDefinition definition, int itemStack)
	{
		base.InitializeItem(definition, itemStack);
		weaponDefinition = definition;

		//weapon-specific setup here
		WeaponView = GetComponentInChildren<WeaponView>();
		FireRateCooldown = 60 / WeaponDefinition.FireRateRPM;
	}

	private void Update()
	{
		HandleFireRate();
	}

	public void EquipWeapon()
	{
		//hold in hands
		currentMagazineAmmo = WeaponDefinition.MagazineSize;
	}

	public void UnEquipWeapon()
	{
		//holster
	}

	public void AimDownSight()
	{
		//ads and modify accuracy and move speed etc...
	}

	#region weapon shooting (TODO add sfx, vfx and animations + recoil and accuracy adjustments)
	public void Shoot(string[] HitableTags)
	{
		if (MagazineEmpty) return;
		if (IsReloading) return;
		if (!CanShoot) return;

		currentMagazineAmmo--;

		Vector3 origin = WeaponView.MuzzlePoint.position;
		Vector3 direction = WeaponView.MuzzlePoint.forward;

		if (TryGetAccurateHit(origin, direction, HitableTags, out RaycastHit hit))
		{
			LastHitPoint = hit.point;

			if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
				damageable.RecieveDamage(weaponDefinition.Damage);
		}
		else
		{
			LastHitPoint = origin + direction * weaponDefinition.EffectiveRange;
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

		if (!hasUnlimitedAmmo && !ammoGiver.AmmoAvailable(weaponDefinition.AmmoType)) return; //no ammo in inventory

		StartCoroutine(ReloadAmmo(ammoGiver, hasUnlimitedAmmo));
	}

	private IEnumerator ReloadAmmo(IAmmoGiver ammoGiver, bool hasUnlimitedAmmo)
	{
		#region ReloadAmmo
		IsReloading = true;
		yield return new WaitForSeconds(WeaponDefinition.ReloadTime);

		if (hasUnlimitedAmmo)
			currentMagazineAmmo = ammoGiver.GetAmmo(WeaponDefinition.AmmoType, weaponDefinition.MagazineSize);
		else
			currentMagazineAmmo = ammoGiver.TakeAmmo(WeaponDefinition.AmmoType, weaponDefinition.MagazineSize);

		IsReloading = false;
		#endregion
	}
	#endregion

	#region try get accurate hit
	private bool TryGetAccurateHit(Vector3 origin, Vector3 direction, string[] hitableTags, out RaycastHit finalHit)
	{
		#region Summary
		/// <summary>
		/// Uses Raycast first for accuracy,
		/// then SphereCast as fallback aim assist
		/// </summary>
		#endregion

		#region TryGetAccurateHit

		finalHit = new RaycastHit();

		#region Raycast
		if (Physics.Raycast(origin, direction, out RaycastHit rayHit, weaponDefinition.EffectiveRange))
		{
			if (IsValidTarget(rayHit.collider, hitableTags))
			{
				finalHit = rayHit;
				return true;
			}
		}
		#endregion

		#region SphereCast
		RaycastHit[] hits = Physics.SphereCastAll(
			origin,
			weaponDefinition.BeamRadius,
			direction,
			WeaponDefinition.EffectiveRange
		);

		float closestDistance = float.MaxValue;
		bool hitFound = false;

		for (int i = 0; i < hits.Length; i++)
		{
			RaycastHit hit = hits[i];

			if (!IsValidTarget(hit.collider, hitableTags))
				continue;

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

	#region valid target check
	private bool IsValidTarget(Collider collider, string[] hitableTags)
	{
		#region Summary
		/// <summary>
		/// Checks if the collider matches any allowed target tags
		/// </summary>
		#endregion

		#region IsValidTarget

		for (int i = 0; i < hitableTags.Length; i++)
		{
			if (collider.CompareTag(hitableTags[i]))
				return true;
		}

		return false;
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

		GameObject bullet = Instantiate(WeaponDefinition.BulletPrefab, 
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
		float travelTime = distance / WeaponDefinition.BulletVisualSpeed;

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
