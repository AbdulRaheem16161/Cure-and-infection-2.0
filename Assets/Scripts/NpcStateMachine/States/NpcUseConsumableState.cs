using Game.MyNPC;
using UnityEngine;

public class NpcUseConsumableState : NPCBaseState
{
	public NpcUseConsumableState(NPCStateMachine stateMachine) : base(stateMachine) { }

	private readonly float useConsumableCooldown = 2.5f; //could later be set from consumable definition
	private float useConsumableTimer;

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
			if (stateMachine.Beliefs.CanHeal)
				stateMachine.EquipmentHandler.UseConsumable(stateMachine.Beliefs.HealableItem.EquipmentType);
			else if (stateMachine.Beliefs.CanDrink)
				stateMachine.EquipmentHandler.UseConsumable(stateMachine.Beliefs.DrinkableItem.EquipmentType, false);
			else if (stateMachine.Beliefs.CanEat)
				stateMachine.EquipmentHandler.UseConsumable(stateMachine.Beliefs.EatableItem.EquipmentType, false);

			useConsumableTimer = useConsumableCooldown; //should loop till satisfied unless no item or gets alerted
		}
	}
}
