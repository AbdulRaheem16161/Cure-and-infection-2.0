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
		unset, melee, handgun, smg, assaultRifle, marksmanRifle, boltActionRifle
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
	#endregion

	#region weapon vfx/sfx
	[Header("Weapon SFX/VFX")]
	[SerializeField] private AudioClip fireSFX;
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

	//recoil
	public Vector2 RecoilPerShot => recoilPerShot;
	public AnimationCurve RecoilPattern => recoilPattern;
	public float RecoilRecoveryRate => recoilRecoveryRate;

	//weapon sfx/vfx
	public AudioClip FireSfx => FireSfx;
	public AudioClip ReloadSfx => reloadSfx;
	public GameObject MuzzleFlashVfx => muzzleFlashVfx;
	public GameObject ImpactVfx => impactVfx;

	//projectile
	public ProjectileDefinition AmmoType => ammoType;
	#endregion

	#region ranged weapon behaviour methods
	public override void UseItem()
	{
		//not supported
		Debug.LogError("UseItem method not implemented for this item type");
	}
	public override void EquipItem()
	{
		//instantiate in a holster of some kind
		Debug.Log($"equipped ranged weapon: {ItemName}");
	}
	public override void UnEquipItem()
	{
		//Destroy from holster of some kind
		Debug.Log($"unequipped ranged weapon: {ItemName}");
	}
	public override void Holster()
	{
		//unequip from hands back into holster of some kind
		Debug.Log($"holstered ranged weapon: {ItemName}");
	}
	public override void UnHolster()
	{
		//unequip from holster into hands
		Debug.Log($"unholstered ranged weapon: {ItemName}");
	}
	#endregion
}
