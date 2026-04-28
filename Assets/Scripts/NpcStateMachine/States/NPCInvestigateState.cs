using Game.MyNPC;
using UnityEngine;

public class NPCInvestigateState : NpcBaseMovementState
{
	private bool glanceStarted;
	private bool glanceDone;
	private float glancingDelay;

	public NPCInvestigateState(NPCStateMachine stateMachine, int priority) : base(stateMachine, priority) { }

	public override bool IsValid()
	{
		return stateMachine.capabilityOverrides.HasFlag(EntityDefinition.Capability.investigate) && Beliefs.FleeTarget == null &&
			Beliefs.Target == null && Beliefs.EatableTarget == null && Beliefs.FreeToInvestigate;
	}

	public override void Enter()
	{
		glanceStarted = false;
		glanceDone = false;
		glancingDelay = 2f;

		if (Beliefs.InvestigateLocation.HasValue)
			MoveToDestination(Beliefs.InvestigateLocation.Value, MoveType.sprint);
	}

	public override void Exit()
	{
		//null on exit incase enemy spotted before glancing finished
		Beliefs.SetNewInvestigateLocation(null);
	}

	public override void Tick(float deltaTime)
	{
		if (stateMachine.StatsHandler.LifeState == EntityDefinition.LifeState.dead) return;

		// ----------- Investigate to idle after simulated glancing in both directions -------------

		if (HasReachedDestination())
		{
			if (!glanceStarted)
			{
				glanceStarted = true;
				stateMachine.NpcPerception.SimulateNpcGlancing(glancingDelay);
			}
			else
			{
				glancingDelay -= deltaTime;

				if (glancingDelay < 0)
					glanceDone = true;
			}

			if (glanceDone)
				Beliefs.SetNewInvestigateLocation(null);
		}
	}
}
