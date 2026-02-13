using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "ScriptableObjects/Item/Weapon")]
public class WeaponDefinition : ItemDefinition
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

	#region weapon projectile properties
	[Header("Projectile Properties")]
	[SerializeField] private ProjectileDefinition ammoType; //can be modified to reference a ProjectileDefinition
	#endregion

	//add fields for accuracy in various states, recoil, sway, move speed while aiming/equipped
	#region weapon behaviour
	#endregion

	//add fields for ui icons, 3d prefab models etc, sfx/vfx specific for weapons etc...
	#region model, vfx, sfx
	#endregion

	#region readonly properties
	public WeaponType Weapon => weaponType;
	public int Damage => damage;
	public int MagazineSize => magazineSize;
	public FireModeType FireMode => fireMode;
	public int FireRateRPM => fireRateRPM;
	public float ReloadTime => reloadTime;
	public int EffectiveRange => effectiveRange;
	public ProjectileDefinition AmmoType => ammoType;
	#endregion
}
