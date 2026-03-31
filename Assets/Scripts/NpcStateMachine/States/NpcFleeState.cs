using Game.MyNPC;
using UnityEngine;

public class NpcFleeState : NpcBaseMovementState
{
	Vector3 directionToLookBackTo;

	public NpcFleeState(NPCStateMachine stateMachine) : base(stateMachine) { }

	public override void Enter()
	{
		directionToLookBackTo = stateMachine.NpcPerception.DetectedTarget.Transform.position;
		FleeToNewDestination(stateMachine.NpcDefinition.SprintSpeed, directionToLookBackTo);
	}

	public override void Exit()
	{

	}

	public override void Tick(float deltaTime)
	{
		// ----------- Move to Idle -------------
		if (HasReachedDestination())
		{
			lookingAtTarget = false;
			LookAtDirection(directionToLookBackTo);

			if (!lookingAtTarget) return;

			stateMachine.Beliefs.SafeFromFleeTarget = true;
			return;
		}
	}
}
