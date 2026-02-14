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

	#region melee weapon behaviour methods
	public override void UseItem()
	{
		//not supported
		Debug.LogError("UseItem method not implemented for this item type");
	}
	public override void EquipItem()
	{
		//instantiate in a holster of some kind
		Debug.Log($"equipped melee weapon: {ItemName}");
	}
	public override void UnEquipItem()
	{
		//Destroy from holster of some kind
		Debug.Log($"unequipped melee weapon: {ItemName}");
	}
	public override void Holster()
	{
		//unequip from hands back into holster of some kind
		Debug.Log($"holstered melee weapon: {ItemName}");
	}
	public override void UnHolster()
	{
		//unequip from holster into hands
		Debug.Log($"unholstered melee weapon: {ItemName}");
	}
	#endregion
}
