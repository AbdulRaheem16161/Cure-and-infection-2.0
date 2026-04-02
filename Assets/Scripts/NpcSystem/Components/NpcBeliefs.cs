using Game.Core;
using Game.MyNPC;
using System;
using UnityEngine;
using UnityEngine.AI;

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
	[NonSerialized] public bool Idling;
	public bool Moving => Agent.velocity.sqrMagnitude > 0.1f;
	#endregion

	#region Stat Beliefs
	public bool Hurt => StatsHandler.health <= NpcDefinition.MaxHealth * 0.5f;
	public bool Thirsty => StatsHandler.water <= NpcDefinition.MaxWater * 0.3f;
	public bool Hungry => StatsHandler.food <= NpcDefinition.MaxFood * 0.2f;
	public bool Exhausted => StatsHandler.stamina <= NpcDefinition.MaxStamina * 0.1f;
	#endregion

	#region Investigation Beliefs
	public bool FreeToInvestigate => InvestigateLocation != null && !HasTarget;
	public Vector3? InvestigateLocation;
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
	public bool RangedWeaponInHands => EquipmentHandler.HasRangedWeaponInHands;
	public bool MeleeWeaponInHands => EquipmentHandler.HasMeleeWeaponInHands;
	#endregion

	private void Awake()
	{
		Agent = GetComponent<NavMeshAgent>();
		StateMachine = GetComponent<NPCStateMachine>();
		NpcPerception = GetComponent<NpcPerception>();
		StatsHandler = GetComponent<StatsHandler>();
		EquipmentHandler = GetComponent<EquipmentHandler>();
	}
	public void InitializeBeliefs(NpcDefinition npcDefinition)
	{
		NpcDefinition = npcDefinition;
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

		if (NpcPerception.DetectedTarget.Distance > EquipmentHandler.rangedWeaponInHands.TypedDefinition.EffectiveRange)
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
}
