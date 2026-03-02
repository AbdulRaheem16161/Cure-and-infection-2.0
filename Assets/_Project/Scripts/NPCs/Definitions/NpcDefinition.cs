using Game.MyNPC;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static NPCSpawner;

/// <summary>
/// plan summery
/// finish this script so making new npcs will be simpler.
/// 
/// repalce weaponholder scripts etc... with EquipmentHandler + inventoryHandler scripts
/// update them to now instantiate and destroy game objects in world.
/// update EquipmentHandler to support currently unholstered weapon (Gameobject currentWeaponObj or EquipmentType currentWeapon)
/// 
/// consider any big changes needed to current state machine for npcs
/// </summary>

[CreateAssetMenu(fileName = "Npc", menuName = "ScriptableObjects/Npc")]
public class NpcDefinition : ScriptableObject
{
	#region npc info
	[Header("Npc Info")]
	private string npcName;
	private Teams defaultNpcTeam;
	#endregion

	#region npc stats
	private int maxHealth;
	private int maxWater;
	private int maxFood;
	private int maxStamina;
	#endregion

	#region npc equipment
	[Header("Npc Equipment")]
	/// <summary>
	/// for zombie ranged attacks we can create a unique WeaponRangedDefinition ZombieSpit as they should work fine if set up like a single shot gun
	/// for zombie melee attacks we can create a unique WeaponMeleeDefinition that will do the same as above
	/// equipment here should be auto equipped via EquipmentHandler if not null
	/// </summary>
	private WeaponRangedDefinition weaponOne;
	private WeaponRangedDefinition weaponTwo;
	private WeaponMeleeDefinition meleeWeapon;

	private ArmourDefinition helmet;
	private ArmourDefinition chest;
	private ArmourDefinition backpack;

	private ConsumableDefinition consumableOne;
	private ConsumableDefinition consumableTwo;
	private ConsumableDefinition consumableThree;
	#endregion

	#region npc behaviour
	/// <summary>
	/// values for various behaviour (eg: fields in DetectionRadius, DetectionCone etc...)
	/// further split up into sections
	/// </summary>
	#endregion
}
