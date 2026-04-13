using Game.MyNPC;
using UnityEngine;

public class NpcEatState : NPCBaseState
{
	public NpcEatState(NPCStateMachine stateMachine, int priority) : base(stateMachine, priority) { }

	private readonly float useConsumableCooldown = 2.5f; //could later be set from consumable definition
	private float useConsumableTimer;

	public override bool IsValid()
	{
		return stateMachine.EnableConsumableUse && Beliefs.TargetFleeingFrom == null && Beliefs.Target == null && Beliefs.Hungry && Beliefs.CanEat;
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
			stateMachine.EquipmentHandler.UseConsumable(stateMachine.Beliefs.EatableItem.EquipmentType, false);
			useConsumableTimer = useConsumableCooldown; //should loop till satisfied unless no item or gets alerted
		}
	}
}
