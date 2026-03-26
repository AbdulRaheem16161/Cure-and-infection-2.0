using Game.MyNPC;
using UnityEngine;
using UnityEngine.AI;

public class NpcBaseMovementState : NPCBaseState
{
	public enum NpcMoveType
	{
		regularMove, moveToTarget, moveToInvestigate, moveToCorpse
	}

	public NpcBaseMovementState(NPCStateMachine stateMachine) : base(stateMachine) { }

	public override void Enter()
	{

	}

	public override void Exit()
	{

	}

	public override void Tick(float deltaTime)
	{

	}

	#region move to destinations based on move type
	protected void MoveToNewDestination(NpcMoveType npcMoveType)
	{
		if (npcMoveType == NpcMoveType.regularMove)
		{
			if (HasValidPatrolFollowPoint())
			{
				//loop through control points
				stateMachine.currentPatrolPoint = (stateMachine.currentPatrolPoint + 1) % stateMachine.PatrolPoints.TrackPoints.Count;

				Vector3 destination = stateMachine.PatrolPoints.GetNextPatrolPointLocation(stateMachine.currentPatrolPoint);
				MoveToNewDestination(stateMachine.PatrolSpeed, destination);
			}
			else
			{
				if (HasValidRandomFollowPoint())
					MoveToNewDestination(stateMachine.PatrolSpeed, stateMachine.RandomMovementManager.GetRandomLocationInArea());
				else
				{
					if (!stateMachine.moveOnPatrolPath || !stateMachine.moveOnRandomPath)
						Debug.LogWarning($"{stateMachine.gameObject} has no valid movement options, enable one in inspector");
					else
						Debug.LogError($"{stateMachine.gameObject} has no valid movement options, " +
							$"follow points likely failed to be assigned when initializing");
				}
			}
		}
		else if (npcMoveType == NpcMoveType.moveToTarget)
			MoveToNewDestination(stateMachine.ChaseSpeed, stateMachine.NpcPerception.DetectedTarget.Transform.position);

		else if (npcMoveType == NpcMoveType.moveToTarget)
			MoveToNewDestination(stateMachine.ChaseSpeed, stateMachine.locationToInvestigate);

		else if (npcMoveType == NpcMoveType.moveToCorpse)
			MoveToNewDestination(stateMachine.PatrolSpeed, stateMachine.NpcPerception.EatableTarget.Transform.position);
	}
	#endregion

	#region basic move to follow point checks
	private bool HasValidPatrolFollowPoint()
	{
		if (stateMachine.moveOnPatrolPath && stateMachine.PatrolPoints != null)
			return true;
		else
			return false;

	}
	private bool HasValidRandomFollowPoint()
	{
		if (stateMachine.moveOnRandomPath && stateMachine.RandomMovementManager != null)
			return true;
		else
			return false;
	}
	#endregion

	#region destination setting
	private void MoveToNewDestination(float speed, Vector3 newDestination)
	{
		stateMachine.Agent.isStopped = false;
		stateMachine.Agent.speed = speed;
		stateMachine.Agent.SetDestination(newDestination);
	}
	#endregion

	#region destination reached check
	protected bool HasReachedDestination()
	{
		//Debug.LogError($"distance {stateMachine.Agent.remainingDistance}");

		if (stateMachine.Agent.remainingDistance <= stateMachine.Agent.stoppingDistance)
			return true;
		else
			return false;
	}
	#endregion
}
