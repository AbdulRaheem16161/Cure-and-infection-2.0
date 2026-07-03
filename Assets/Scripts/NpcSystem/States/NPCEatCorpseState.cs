using Game.MyNPC;
using UnityEngine;

public class NPCEatCorpseState : NpcBaseMovementState
{
	public NPCEatCorpseState(NPCStateMachine stateMachine, int priority) : base(stateMachine, priority) { }

	/// <summary>
	/// for now just use timer, later it would be better to have a stat in StateHandler like float ZombificationProgress.
	/// basically multiple zombies allowed to eat a corpse, speading up progress and they only stop once 
	/// ZombificationProgress is complete and ignore other state switching (subject to change)
	/// </summary>

	private bool eatingCorpse;
	private readonly float zombificationTickCooldown = 1f;
	private float zombificationTickTimer;

	public override bool IsValid()
	{
		return stateMachine.capabilityOverrides.HasFlag(EntityDefinition.Capability.eatCorpse) && Beliefs.FleeTarget == null && 
			Beliefs.Target == null && stateMachine.StatsHandler.LifeState == EntityDefinition.LifeState.zombified && Beliefs.EatableTarget != null;
	}

	public override void Enter()
	{
		eatingCorpse = false;
		zombificationTickTimer = zombificationTickCooldown;
		MoveToDestination(stateMachine.Beliefs.EatableTarget.Position, MoveType.walk);
	}

	public override void Exit()
	{
		stateMachine.Agent.isStopped = false;
	}

	public override void Tick(float deltaTime)
	{
        if (stateMachine.StatsHandler.LifeState == EntityDefinition.LifeState.dead) return;

        // move to position of cropse
        if (IsWithinDistanceOfDestination(2f))
			eatingCorpse = true;

		if (eatingCorpse)
		{
			zombificationTickTimer -= deltaTime;
			stateMachine.Agent.isStopped = true;

			if (zombificationTickTimer > 0) return;
			if (Beliefs.EatableTarget == null) return;

			zombificationTickTimer = zombificationTickCooldown;
			Beliefs.EatableTarget.StatsHandler.TickZombificationProcess();
		}
	}
}
