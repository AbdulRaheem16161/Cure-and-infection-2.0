using System;
using System.Collections.Generic;
using UnityEngine;
using static DamageContext;

[CreateAssetMenu(fileName = "WeaponRanged", menuName = "ScriptableObjects/Item/WeaponRanged")]
public class WeaponRangedDefinition : ItemDefinition
{
	#region weapon characteristics
	[Header("Weapon Characteristics")]
	[SerializeField] private WeaponType weaponType;
	public enum WeaponType
	{
		unset, handgun, shotgun, smg, assaultRifle, marksmanRifle, boltActionRifle
	}

	[SerializeField] private HitImpact impactType;
	[SerializeField] private int damage;
	[SerializeField] private int magazineSize;

	[SerializeField] private FireModeType allowedFireModes;
	[Flags]
	public enum FireModeType
	{
		none = 0,
		pumpAction = 1 << 0,
		semiAuto = 1 << 1,
		fullAuto = 1 << 2,
		boltAction = 1 << 3
	}
	[SerializeField] private int fireRateRPM;
	[SerializeField] private float reloadTime;
	[SerializeField] private int effectiveRange;

	[SerializeField] private WeaponFlag weaponFlags;
    [Flags]
    public enum WeaponFlag
    {
        none = 0,
        improvised = 1 << 0,
        civilian = 1 << 1,
        police = 1 << 2,
        military = 1 << 3,
        hunting = 1 << 4,
    }
	#endregion

	#region weapon handling
	[Header("Weapon Handling")]
	[SerializeField] private float adsSpeed = 1f;
	[SerializeField] private float hipfireBulletSpreadMultiplier = 1;
	[SerializeField] private float adsBulletSpreadMultiplier = 0.4f;
	[SerializeField] private float hipfireRecoilMultiplier = 1;
	[SerializeField] private float adsRecoilMultiplier = 0.8f;
	#endregion

	#region weapon movement
	[Header("Weapon Movement")]
	[SerializeField] private float moveSpeedModifier = 1f;
	[SerializeField] private float adsMoveSpeedModifer = 0.4f;
	#endregion

	#region weapon accuracy
	[Header("Weapon Accuracy")]
	[SerializeField] private float baseSpread = 0.005f;
	[SerializeField] private float maxSpread = 5; //also limits bullet spread from flinch impact type, consider seperating them later.
	[SerializeField] private float spreadIncreasePerShot = 0.01f;
	#endregion

	#region weapon recoil
	[Header("Weapon Recoil")]
	[SerializeField] private float recoilSnappiness = 15f;
	[SerializeField] private float recoilRecoveryRate = 10f;
	[SerializeField] private List<Vector3> recoilPattern;
	#endregion

	#region weapon projectile properties
	[Header("Projectile Properties")]
	[SerializeField] private ProjectileDefinition ammoType; //can be modified to reference a ProjectileDefinition
	[SerializeField] private GameObject bulletPrefab;
	[SerializeField] private float bulletVisualSpeed;
	#endregion

	#region weapon vfx/sfx
	[Header("Weapon SFX/VFX")]
	[SerializeField] private AudioClip fireSfx;
	[SerializeField] private AudioClip reloadSfx;
	[SerializeField] private GameObject muzzleFlashVfx;
	[SerializeField] private GameObject impactVfx;
	#endregion

	#region readonly properties
	//weapon characteristics
	public WeaponType Weapon => weaponType;
	public HitImpact ImpactType => impactType;
	public int Damage => damage;
	public int MagazineSize => magazineSize;

	public FireModeType AllowedFireModes => allowedFireModes;
	public int FireRateRPM => fireRateRPM;
	public float ReloadTime => reloadTime;
	public int EffectiveRange => effectiveRange;
	public int EffectiveSqrRange => effectiveRange * effectiveRange;

	public WeaponFlag WeaponFlags => weaponFlags;

	//handling
	public float AdsSpeed => adsSpeed;
	public float HipfireBulletSpreadMultiplier => hipfireBulletSpreadMultiplier;
	public float AdsBulletSpreadMultiplier => adsBulletSpreadMultiplier;
	public float HipfireRecoilMultiplier => hipfireRecoilMultiplier;
	public float AdsRecoilMultiplier => adsRecoilMultiplier;

	//movement
	public float MoveSpeedModifier => moveSpeedModifier;
	public float AdsMoveSpeedModifer => adsMoveSpeedModifer;

	//accuracy
	public float BaseSpread => baseSpread;
	public float MaxSpread => maxSpread;
	public float SpreadIncreasePerShot => spreadIncreasePerShot;

	//recoil
	public float RecoilSnappiness => recoilSnappiness;
	public float RecoilRecoveryRate => recoilRecoveryRate;
	public List<Vector3> RecoilPattern => recoilPattern;

	//weapon sfx/vfx
	public AudioClip FireSfx => fireSfx;
	public AudioClip ReloadSfx => reloadSfx;
	public GameObject MuzzleFlashVfx => muzzleFlashVfx;
	public GameObject ImpactVfx => impactVfx;

	//projectile
	public ProjectileDefinition AmmoType => ammoType;
	public GameObject BulletPrefab => bulletPrefab;
	public float BulletVisualSpeed => bulletVisualSpeed;
	#endregion
}
