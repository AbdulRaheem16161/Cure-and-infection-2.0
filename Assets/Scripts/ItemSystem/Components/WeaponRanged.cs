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
		SimulateBulletSpread(); //uses raycast hitscan + visual bullet representation
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

	#region simulate simple bullet spread for now
	private void SimulateBulletSpread()
	{
		float hardcodedSpread = 0.01f;

		Vector3 startPos = WeaponView.MuzzlePoint.position;
		Vector3 direction = WeaponView.MuzzlePoint.forward;
		float distance = TypedDefinition.EffectiveRange;

		//apply random spread
		direction = Quaternion.Euler(
			Random.Range(-hardcodedSpread, hardcodedSpread),
			Random.Range(-hardcodedSpread, hardcodedSpread),
			0) * direction;

		if (Physics.Raycast(startPos, direction, out RaycastHit hit, 
			TypedDefinition.EffectiveRange, LayerMask.GetMask("CharacterHitbox", "Environment")))
		{
			//handle hit
			if (hit.collider.TryGetComponent(out HitCollider hitCollider))
				hitCollider.OnHit(TypedDefinition.Damage, CurrentOwner);

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
}
