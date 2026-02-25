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
	[SerializeField] private float attackSpeed;
	#endregion

	#region readonly properties
	public WeaponType Weapon => weaponType;
	public int Damage => damage;
	public float AttackSpeed => attackSpeed;
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
