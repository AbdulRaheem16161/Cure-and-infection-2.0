using Game.MyNPC;
using UnityEngine;

public class NPCInvestigateState : NPCBaseState
{
	public Vector3 locationToInvestigate;

	public NPCInvestigateState(NPCStateMachine stateMachine) : base(stateMachine) { }

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

		if (stateMachine.HasInvestigatedLocation)
		{
			stateMachine.SwitchState(new NPCIdleState(stateMachine));
			return;
		}
		// ----------- Investigate to Chase -------------

		if (stateMachine.DetectionRadius.isTargetDetected || stateMachine.DetectionCone.isTargetDetected)
		{
			stateMachine.SwitchState(new NPCChaseState(stateMachine));
			return;
		}
		#endregion

		// Do Random Movement if MoveOnRandomPath is Selected in the EnemyStateMachine
		if (stateMachine.HasLocationToInvestigate && !stateMachine.HasInvestigatedLocation)
		{
			stateMachine.CurrentFollowPoint = stateMachine.RandomFollowPoint.transform;
			stateMachine.Agent.SetDestination(stateMachine.CurrentFollowPoint.position);
			return;
		}
	}
}
