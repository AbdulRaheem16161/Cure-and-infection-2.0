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
	public EntityDefinition Definition { get; private set; }
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
	public bool Hurt => StatsHandler.health <= Definition.MaxHealth * 0.8f;
	public bool Thirsty => StatsHandler.water <= Definition.MaxWater * 0.6f;
	public bool Hungry => StatsHandler.food <= Definition.MaxFood * 0.5f;
	public bool IsExhausted => StatsHandler.IsExhausted;
	#endregion

	#region Investigation Beliefs
	public bool FreeToInvestigate => InvestigateLocation != null && Target == null;
	public Vector3? InvestigateLocation { get; private set; }
	public Vector3? LookDirection { get; private set; }
	#endregion

	#region Cover Beliefs
	public bool MovingToCover => CoverPosition.HasValue;
	[NonSerialized] public bool ReturnFire;
	[NonSerialized] public bool InCover;
	public Vector3? CoverPosition { get; private set; }
	#endregion

	#region Target Beliefs
	public TargetData Target => NpcPerception.Target;
	public TargetData EatableTarget => NpcPerception.EatableTarget;
	public bool TargetInShootingRange => TargetInShootingRangeCheck();
	public bool TargetInMeleeRange => TargetInMeleeRangeCheck();
	#endregion

	#region Flee Beliefs
	public bool TargetInFleeRange => TargetInFleeRangeCheck();
	public TargetData ClosestFleeTarget => NpcPerception.ClosestFleeTarget; //not shown
	public TargetData FleeTarget { get; private set; }
	#endregion

	#region Equipment Beliefs
	public bool RangedWeaponInHands => EquipmentHandler.itemInHands is RangedWeaponItem;
	public bool MeleeWeaponInHands => EquipmentHandler.itemInHands is MeleeWeaponItem;
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
	public void InitializeBeliefs(EntityDefinition definition)
	{
		Definition = definition;
		InvestigateLocation = null;
		LookDirection = null;
	}

	private void OnDestroy()
	{
		EquipmentHandler.OnEquippedItemChanges -= OnEquippedItemChanges;
		StatsHandler.OnHit -= HandleOnHit;
	}

	#region alert belief check
	public bool Alerted()
	{
		if (Target != null || EatableTarget != null || InvestigateLocation != null)
			return true;
		else 
			return false;
	}
	#endregion

	#region target in melee/ranged attack ranges check
	private bool TargetInMeleeRangeCheck()
	{
		if (Target == null || !MeleeWeaponInHands) return false;

		float stoppingDistanceSqr = Agent.stoppingDistance * Agent.stoppingDistance + 0.25f;
		return Target.SquaredDistance < stoppingDistanceSqr;
	}

	private bool TargetInShootingRangeCheck()
	{
		if (Target == null || !RangedWeaponInHands) return false;

		RangedWeaponItem weaponRanged = EquipmentHandler.itemInHands as RangedWeaponItem;
		return Target.SquaredDistance < weaponRanged.TypedDefinition.EffectiveSqrRange;
	}
	#endregion

	#region target in flee range check and updates
	private bool TargetInFleeRangeCheck()
	{
		if (MeleeWeaponInHands) return false;

		if (FleeTarget != null) //flee
		{
			FleeTarget.UpdateTargetDistance(transform.position);

			if (FleeTarget.SquaredDistance < Definition.FleeSqrDistance)
			{
				if (ClosestFleeTarget != null && ClosestFleeTarget.SquaredDistance < FleeTarget.SquaredDistance) //switch to flee from closer threat
					FleeTarget = ClosestFleeTarget;

				return true;
			}
		}

		if (ClosestFleeTarget != null && ClosestFleeTarget.SquaredDistance < Definition.FleeSqrDistance) //start fleeing if not already
		{
			FleeTarget = ClosestFleeTarget;
			return true;
		}

		return false;
	}
	public void UpdateFleeTarget(TargetData targetData)
	{
		FleeTarget = targetData;
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

	#region Update Cover Object
	public void UpdateCoverPosition(Vector3? coverPosition)
	{
		CoverPosition = coverPosition;
	}
	#endregion

	#region Update InvestigateLocation
	public void SetNewInvestigateLocation(Vector3? location)
	{
		InvestigateLocation = location;
	}
	#endregion

	#region Update LookDirection
	public void SetNewLookDirection(Vector3? location)
	{
		LookDirection = location;
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
