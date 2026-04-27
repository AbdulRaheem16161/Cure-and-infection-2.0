using Game.MyNPC;
using UnityEngine;

public class NpcFleeState : NpcBaseMovementState
{
	private Vector3 directionToLookBackTo;

	private readonly float fleeCooldown = 2f;
	private float fleeTimer;

	public NpcFleeState(NPCStateMachine stateMachine, int priority) : base(stateMachine, priority) { }

	public override bool IsValid()
	{
		if (stateMachine.capabilityOverrides.HasFlag(EntityDefinition.Capability.flee) && Beliefs.TargetFleeingFrom != null) return true;

		return stateMachine.capabilityOverrides.HasFlag(EntityDefinition.Capability.flee) 
			&& Beliefs.TargetInFleeRange && !Beliefs.MeleeWeaponInHands;
	}

	public override void Enter()
	{
		fleeTimer = 0;
		stateMachine.Beliefs.SetNewInvestigateLocation(null);
		lookingAtTarget = false;
	}

	public override void Exit()
	{

	}

	public override void Tick(float deltaTime)
	{
		if (stateMachine.Beliefs.TargetInFleeRange)
		{
			fleeTimer -= deltaTime;

			if (fleeTimer <= 0 || IsWithinDistanceOfDestination(2f))
				UpdateFleeingDirection();
		}
		else
		{
			LookAtDirection(directionToLookBackTo);
			if (!lookingAtTarget) return;

			stateMachine.Beliefs.UpdateTargetFleeingFrom(null);
			return;
		}
	}

	private void UpdateFleeingDirection()
	{
		fleeTimer = fleeCooldown;
		directionToLookBackTo = stateMachine.Beliefs.TargetFleeingFrom.Transform.position;
		FleeToNewDestination(directionToLookBackTo);
	}
}
