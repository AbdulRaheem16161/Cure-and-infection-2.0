using Game.MyNPC;
using UnityEngine;

public class NpcDrinkState : NPCBaseState
{
	public NpcDrinkState(NPCStateMachine stateMachine, int priority) : base(stateMachine, priority) { }

	private readonly float useConsumableCooldown = 2.5f; //could later be set from consumable definition
	private float useConsumableTimer;

	public override bool IsValid()
	{
		return stateMachine.capabilityOverrides.HasFlag(EntityDefinition.Capability.drink) && Beliefs.FleeTarget == null &&
			Beliefs.Target == null && Beliefs.Thirsty && Beliefs.CanDrink;
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
			stateMachine.EquipmentHandler.UseConsumable(stateMachine.Beliefs.DrinkableItem.EquipmentType, false);
			useConsumableTimer = useConsumableCooldown; //should loop till satisfied unless no item or gets alerted
		}
	}
}
