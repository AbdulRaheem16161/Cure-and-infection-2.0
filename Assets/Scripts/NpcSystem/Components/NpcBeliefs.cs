using Game.Core;
using Game.MyNPC;
using System;
using UnityEngine;
using UnityEngine.AI;
using static EquipmentHandler;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NPCStateMachine))]
[RequireComponent(typeof(NpcPerception))]
[RequireComponent(typeof(StatsHandler))]
[RequireComponent(typeof(EquipmentHandler))]
public class NpcBeliefs : MonoBehaviour
{
	public NavMeshAgent Agent { get; private set; }
	public NPCStateMachine StateMachine { get; private set; }
	public NpcDefinition NpcDefinition { get; private set; }
	public NpcPerception NpcPerception { get; private set; }
	public StatsHandler StatsHandler { get; private set; }
	public EquipmentHandler EquipmentHandler { get; private set; }

	#region Alert Beliefs
	[NonSerialized] public bool Alert;
	public bool InAlertState => Alerted();
	#endregion

	#region Movement Beliefs
	[NonSerialized] public bool Stunned;
	[NonSerialized] public bool Idling;
	public bool Moving => Agent.velocity.sqrMagnitude > 0.1f;
	#endregion

	#region Stat Beliefs
	public bool Hurt => StatsHandler.health <= NpcDefinition.MaxHealth * 0.8f;
	public bool Thirsty => StatsHandler.water <= NpcDefinition.MaxWater * 0.6f;
	public bool Hungry => StatsHandler.food <= NpcDefinition.MaxFood * 0.5f;
	public bool Exhausted => StatsHandler.stamina <= NpcDefinition.MaxStamina * 0.1f;
	#endregion

	#region Investigation Beliefs
	public bool FreeToInvestigate => InvestigateLocation != null && !HasTarget;
	public Vector3? InvestigateLocation { get; private set; }
	#endregion

	#region Target Beliefs
	public bool HasEatableTarget => NpcPerception.IsEatableTargetDetected;
	public bool HasTarget => NpcPerception.IsTargetDetected;
	public bool TargetInShootingRange => TargetInShootingRangeCheck();
	public bool TargetInMeleeRange => TargetInMeleeRangeCheck();
	#endregion

	#region Flee Beliefs
	public bool TargetInFleeRange => TargetInFleeRangeCheck();
	[NonSerialized] public bool SafeFromFleeTarget;
	#endregion

	#region Equipment Beliefs
	public bool RangedWeaponInHands => EquipmentHandler.itemInHands is WeaponRanged;
	public bool MeleeWeaponInHands => EquipmentHandler.itemInHands is WeaponMelee;
	public bool CanHeal => Hurt && HealableItem != null;
	public bool CanDrink => Thirsty && DrinkableItem != null;
	public bool CanEat => Hungry && EatableItem != null;
	#endregion

	//internal belifs that should probably stay hidden
	#region tracked Equipment slots updated on events
	private EquipmentSlot ConsumableOne;
	private EquipmentSlot ConsumableTwo;
	private EquipmentSlot ConsumableThree;
	#endregion

	#region track usable consumables
	public EquipmentSlot HealableItem => CanUseConsumableItem(ConsumableDefinition.RestorationType.health);
	public EquipmentSlot DrinkableItem => CanUseConsumableItem(ConsumableDefinition.RestorationType.water);
	public EquipmentSlot EatableItem => CanUseConsumableItem(ConsumableDefinition.RestorationType.food);
	#endregion

	private void Awake()
	{
		Agent = GetComponent<NavMeshAgent>();
		StateMachine = GetComponent<NPCStateMachine>();
		NpcPerception = GetComponent<NpcPerception>();
		StatsHandler = GetComponent<StatsHandler>();
		EquipmentHandler = GetComponent<EquipmentHandler>();

		EquipmentHandler.OnEquippedItemChanges += OnEquippedItemChanges;
		StatsHandler.OnHit += HandleOnHit;
	}
	public void InitializeBeliefs(NpcDefinition npcDefinition)
	{
		NpcDefinition = npcDefinition;
		InvestigateLocation = null;
	}

	private void OnDestroy()
	{
		EquipmentHandler.OnEquippedItemChanges -= OnEquippedItemChanges;
		StatsHandler.OnHit -= HandleOnHit;
	}

	#region alert belief check
	public bool Alerted()
	{
		if (HasTarget || HasEatableTarget || InvestigateLocation != null)
			return true;
		else 
			return false;
	}
	#endregion

	#region target in melee/ranged attack ranges check
	private bool TargetInMeleeRangeCheck()
	{
		if (!NpcPerception.IsTargetDetected || !MeleeWeaponInHands) return false;

		if (NpcPerception.DetectedTarget.Distance > Agent.stoppingDistance + 0.1f)
			return false;
		else
			return true;
	}

	private bool TargetInShootingRangeCheck()
	{
		if (!NpcPerception.IsTargetDetected || !RangedWeaponInHands) return false;

		WeaponRanged weaponRanged = EquipmentHandler.itemInHands as WeaponRanged;

		if (NpcPerception.DetectedTarget.Distance > weaponRanged.TypedDefinition.EffectiveRange)
			return false;
		else
			return true;
	}
	#endregion

	#region target in flee range check
	private bool TargetInFleeRangeCheck()
	{
		if (!NpcPerception.IsTargetDetected || MeleeWeaponInHands) return false;

		if (NpcPerception.DetectedTarget.Distance > NpcDefinition.FleeDistance)
			return false;
		else
		{
			SafeFromFleeTarget = false;
			return true;
		}
	}
	#endregion

	#region Track Equipment Changes
	private void OnEquippedItemChanges(EquipmentSlot equipmentSlot, bool wasEquipped)
	{
		static EquipmentSlot UpdateOrNullEquipmentReference(EquipmentSlot equipmentSlot, bool wasEquipped)
		{
			return wasEquipped ? equipmentSlot : null;
		}

		switch (equipmentSlot.EquipmentType)
		{
			case EquipmentType.consumableOne:
			ConsumableOne = UpdateOrNullEquipmentReference(equipmentSlot, wasEquipped);
			return;
			case EquipmentType.consumableTwo:
			ConsumableTwo = UpdateOrNullEquipmentReference(equipmentSlot, wasEquipped);
			return;
			case EquipmentType.consumableThree:
			ConsumableThree = UpdateOrNullEquipmentReference(equipmentSlot, wasEquipped);
			return;
		}
	}
	#endregion

	#region Update InvestigateLocation
	public void SetNewInvestigateLocation(Vector3? location)
	{
		InvestigateLocation = location;
	}
	#endregion

	#region complex equipment checks
	private EquipmentSlot CanUseConsumableItem(ConsumableDefinition.RestorationType restorationType)
	{
		if (restorationType == ConsumableDefinition.RestorationType.health && !Hurt) return null;
		if (restorationType == ConsumableDefinition.RestorationType.water && !Thirsty) return null;
		if (restorationType == ConsumableDefinition.RestorationType.food && !Hungry) return null;

		static EquipmentSlot CanRestore(EquipmentSlot consumableSlot, ConsumableDefinition.RestorationType restorationType)
		{
			if (consumableSlot == null || consumableSlot.ItemDefinitionNull ||
			consumableSlot.Item.ItemDefinition is not ConsumableDefinition consumable || 
			!consumable.RestorationTypes.HasFlag(restorationType)) return null;
			else
				return consumableSlot;
		}
		EquipmentSlot cachedConsumableSlot = CanRestore(ConsumableOne, restorationType) ??
								   CanRestore(ConsumableTwo, restorationType) ??
								   CanRestore(ConsumableThree, restorationType);

		return cachedConsumableSlot;
	}
	#endregion

	#region On Hit Stunned Event Listener
	private void HandleOnHit(DamageContext damageContext)
	{
		if (damageContext.ImpactType == DamageContext.HitImpact.knockback)
			Stunned = true;
	}
	#endregion
}
