using Game.MyNPC;
using UnityEngine;

public class NpcHealState : NPCBaseState
{
	public NpcHealState(NPCStateMachine stateMachine, int priority) : base(stateMachine, priority) { }

	private readonly float useConsumableCooldown = 2.5f; //could later be set from consumable definition
	private float useConsumableTimer;

	public override bool IsValid()
	{
		return stateMachine.capabilityOverrides.HasFlag(EntityDefinition.Capability.heal) 
			&& Beliefs.TargetFleeingFrom == null && Beliefs.Target == null && Beliefs.Hurt && Beliefs.CanHeal;
	}

	public override void Enter()
	{
		stateMachine.Agent.isStopped = true;
		useConsumableTimer = useConsumableCooldown;
	}

	public override void Exit()
	{

	}

	public override void Tick(float deltaTime)
	{
		useConsumableTimer -= deltaTime;
		if (useConsumableTimer < 0)
		{
			stateMachine.EquipmentHandler.UseConsumable(stateMachine.Beliefs.HealableItem.EquipmentType);
			useConsumableTimer = useConsumableCooldown; //should loop till satisfied unless no item or gets alerted
		}
	}
}
