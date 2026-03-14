using System;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponMelee", menuName = "ScriptableObjects/Item/WeaponMelee")]
public class WeaponMeleeDefinition : ItemDefinition
{
	#region weapon characteristics
	[Header("Weapon Characteristics")]
	[SerializeField] private WeaponType weaponType;
	public enum WeaponType
	{
		unset, melee, handgun, smg, assaultRifle, marksmanRifle, boltActionRifle
	}

	[SerializeField] private int damage;

	[Header("Swing Behaviour")]
	[Tooltip("How quick the swing is")]
	[SerializeField] private float lightSwingSpeed;
	[Tooltip("How long till you can swing again after swingSpeed")]
	[SerializeField] private float lightSwingCooldown;

	[Tooltip("How quick the swing is")]
	[SerializeField] private float heavySwingSpeed;
	[Tooltip("How long till you can swing again after swingSpeed")]
	[SerializeField] private float heavySwingCooldown;
	#endregion

	#region readonly properties
	public WeaponType Weapon => weaponType;
	public int Damage => damage;

	//swing behaviour
	public float LightSwingSpeed => lightSwingSpeed;
	public float LightSwingCooldown => lightSwingCooldown;

	public float HeavySwingSpeed => heavySwingSpeed;
	public float HeavySwingCooldown => heavySwingCooldown;
	#endregion

	public override void OnEquip(EquipmentHandler handler, EquipmentSlot slot)
	{
		handler.EquipMeleeWeapon(slot);
	}

	public override void OnUnequip(EquipmentHandler handler, EquipmentSlot slot)
	{
		handler.UnEquipMeleeWeapon(slot);
	}
}
