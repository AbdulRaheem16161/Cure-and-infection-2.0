using Game.MyNPC;
using UnityEngine;

public class NPCInvestigateState : NPCBaseState
{
	public NPCInvestigateState(NPCStateMachine stateMachine) : base(stateMachine) { }

	private bool LocationReached => ReachedInvestigationLocation();

	public override void Enter()
	{
		stateMachine.Agent.speed = stateMachine.PatrolSpeed;
		stateMachine.HasInvestigatedLocation = false;
		stateMachine.HasLocationToInvestigate = true;
	}

	public override void Exit()
	{
		stateMachine.HasInvestigatedLocation = true;
		stateMachine.HasLocationToInvestigate = false;
	}

	public override void Tick(float deltaTime)
	{
		#region State Transitions

		// ----------- Investigate to idle -------------

		if (LocationReached)
		{
			stateMachine.SwitchState(new NPCIdleState(stateMachine));
			return;
		}
		// ----------- Investigate to Chase -------------

		if (stateMachine.NpcPerception.isTargetDetected)
		{
			stateMachine.SwitchState(new NPCChaseState(stateMachine));
			return;
		}
		#endregion

		// Do Random Movement if MoveOnRandomPath is Selected in the EnemyStateMachine
		if (stateMachine.HasLocationToInvestigate && !stateMachine.HasInvestigatedLocation)
		{
			stateMachine.Agent.SetDestination(stateMachine.locationToInvestigate);
			return;
		}
	}

	private bool ReachedInvestigationLocation()
	{
		float distanceToLocation = Vector3.Distance(stateMachine.transform.position, stateMachine.locationToInvestigate);
		if (distanceToLocation < 2f)
			return true;
		else
			return false;
	}
}
