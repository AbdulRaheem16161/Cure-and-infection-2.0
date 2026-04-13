using UnityEngine;

[CreateAssetMenu(fileName = "Humanoid", menuName = "ScriptableObjects/Entities/Humanoids")]
public class HumanoidDefinition : EntityDefinition
{
	#region Equipment
	[Header("Equipment")]
	/// <summary>
	/// for zombie ranged attacks we can create a unique WeaponRangedDefinition ZombieSpit as they should work fine if set up like a single shot gun
	/// for zombie melee attacks we can create a unique WeaponMeleeDefinition that will do the same as above
	/// </summary>
	[SerializeField] private WeaponRangedDefinition weaponOne;
	[SerializeField] private WeaponRangedDefinition weaponTwo;
	[SerializeField] private WeaponMeleeDefinition meleeWeapon;

	[SerializeField] private ArmourDefinition helmet;
	[SerializeField] private ArmourDefinition chest;
	[SerializeField] private ArmourDefinition backpack;

	[SerializeField] private ConsumableDefinition consumableOne;
	[SerializeField] private ConsumableDefinition consumableTwo;
	[SerializeField] private ConsumableDefinition consumableThree;
	#endregion

	#region read only
	public WeaponRangedDefinition WeaponOne => weaponOne;
	public WeaponRangedDefinition WeaponTwo => weaponTwo;
	public WeaponMeleeDefinition MeleeWeapon => meleeWeapon;

	public ArmourDefinition Helmet => helmet;
	public ArmourDefinition Chest => chest;
	public ArmourDefinition Backpack => backpack;

	public ConsumableDefinition ConsumableOne => consumableOne;
	public ConsumableDefinition ConsumableTwo => consumableTwo;
	public ConsumableDefinition ConsumableThree => consumableThree;
	#endregion
}
