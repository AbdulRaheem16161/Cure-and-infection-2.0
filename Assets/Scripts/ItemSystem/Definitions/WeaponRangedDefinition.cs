using System;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponRanged", menuName = "ScriptableObjects/Item/WeaponRanged")]
public class WeaponRangedDefinition : ItemDefinition
{
	#region weapon characteristics
	[Header("Weapon Characteristics")]
	[SerializeField] private WeaponType weaponType;
	public enum WeaponType
	{
		unset, melee, handgun, shotgun, smg, assaultRifle, marksmanRifle, boltActionRifle
	}

	[SerializeField] private int damage;
	[SerializeField] private int magazineSize;

	[SerializeField] private FireModeType fireMode;
	[Flags]
	public enum FireModeType
	{
		pumpAction = 1, semiAuto = 2, fullAuto = 4, boltAction = 8
	}
	[SerializeField] private int fireRateRPM;
	[SerializeField] private float reloadTime;
	[SerializeField] private int effectiveRange;
	#endregion

	#region weapon handling
	[Header("Weapon Handling")]
	[SerializeField] private float aimTime;
	[SerializeField] private float moveSpeedModifier;
	[SerializeField] private float aimMoveSpeedModifer;
	#endregion

	#region weapon accuracy
	[Header("Weapon Accuracy")]
	[SerializeField] private float hipFireSpread;
	[SerializeField] private float adsSpread;
	[SerializeField] private float spreadIncreasePerShot;
	[SerializeField] private float spreadRecoveryRate;
	[SerializeField] private float maxSpread;

	[SerializeField] private float beamRadius;
	#endregion

	#region weapon recoil
	[Header("Weapon Recoil")]
	[SerializeField] private Vector2 recoilPerShot;
	[SerializeField] private AnimationCurve recoilPattern;
	[SerializeField] private float recoilRecoveryRate;
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
	public int Damage => damage;
	public int MagazineSize => magazineSize;

	public FireModeType FireMode => fireMode;
	public int FireRateRPM => fireRateRPM;
	public float ReloadTime => reloadTime;
	public int EffectiveRange => effectiveRange;

	//handling
	public float AimTime => aimTime;
	public float MoveSpeedModifier => moveSpeedModifier;
	public float AimMoveSpeedModifer => aimMoveSpeedModifer;

	//accuracy
	public float HipFireSpread => hipFireSpread;
	public float AdsSpread => adsSpread;
	public float SpreadIncreasePerShot => spreadIncreasePerShot;
	public float SpreadRecoveryRate => spreadRecoveryRate;
	public float MaxSpread => maxSpread;

	public float BeamRadius => beamRadius;

	//recoil
	public Vector2 RecoilPerShot => recoilPerShot;
	public AnimationCurve RecoilPattern => recoilPattern;
	public float RecoilRecoveryRate => recoilRecoveryRate;

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

	public override void OnEquip(EquipmentHandler handler, EquipmentSlot slot)
	{
		handler.EquipRangedWeapon(slot);
	}

	public override void OnUnequip(EquipmentHandler handler, EquipmentSlot slot)
	{
		handler.UnEquipRangedWeapon(slot);
	}
}
