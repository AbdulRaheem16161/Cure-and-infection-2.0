using Game.MyNPC;
using UnityEngine;

public class NpcFleeState : NpcBaseMovementState
{
	private readonly float fleeCooldown = 2f;
	private float fleeTimer;

	public NpcFleeState(NPCStateMachine stateMachine, int priority) : base(stateMachine, priority) { }

	public override bool IsValid()
	{
		if (stateMachine.capabilityOverrides.HasFlag(EntityDefinition.Capability.flee) && Beliefs.FleeTarget != null)
			return true;

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
        if (stateMachine.StatsHandler.LifeState == EntityDefinition.LifeState.dead) return;

        if (Beliefs.TargetInFleeRange)
		{
			fleeTimer -= deltaTime;

			if (fleeTimer <= 0 || IsWithinDistanceOfDestination(2f))
				UpdateFleeingDirection();
		}
		else
		{
			if (Beliefs.LookDirection.HasValue)
			{
				LookAtDirection(Beliefs.LookDirection.Value);
				if (!lookingAtTarget) return;
			}

			Beliefs.UpdateFleeTarget(null);
			Beliefs.SetNewLookDirection(null);
		}
	}

	private void UpdateFleeingDirection()
	{
		fleeTimer = fleeCooldown;
		Beliefs.SetNewLookDirection(Beliefs.FleeTarget.Transform.position);
		FleeFromPosition(Beliefs.FleeTarget.Transform.position);
	}
}
