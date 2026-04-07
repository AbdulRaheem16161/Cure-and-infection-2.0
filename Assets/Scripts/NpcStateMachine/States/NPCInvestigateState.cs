using Game.MyNPC;
using UnityEngine;

public class NPCInvestigateState : NpcBaseMovementState
{
	private bool glanceStarted;
	private bool glanceDone;
	private float glancingDelay;

	public NPCInvestigateState(NPCStateMachine stateMachine) : base(stateMachine) { }

	public override void Enter()
	{
		glanceStarted = false;
		glanceDone = false;
		glancingDelay = 2f;
		MoveToDestination(stateMachine.NpcDefinition.SprintSpeed, (Vector3)stateMachine.Beliefs.InvestigateLocation);
	}

	public override void Exit()
	{
		//null on exit incase enemy spotted before glancing finished
		stateMachine.Beliefs.InvestigateLocation = null;
	}

	public override void Tick(float deltaTime)
	{
		if (stateMachine.StatsHandler.LifeState == NpcDefinition.LifeState.dead) return;

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
				stateMachine.Beliefs.InvestigateLocation = null;
		}
	}
}
