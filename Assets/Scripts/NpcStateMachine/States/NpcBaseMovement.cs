using Game.MyNPC;
using UnityEngine;
using UnityEngine.AI;

public class NpcBaseMovementState : NPCBaseState
{
	protected bool lookingAtTarget;

	public enum NpcMoveType
	{
		regularMove, moveToTarget, moveToInvestigate, moveToCorpse, fleeFromTarget
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

	#region move to destination methods
	/// <summary>
	/// logic to handle what movement type to use
	/// </summary>
	protected void HandleMovementLogic()
	{
		if (HasValidPatrolPoints())
		{
			//loop through control points and grab next if has reached current point, else continue to current one
			if (stateMachine.reachedCurrentControlPoint)
			{
				stateMachine.reachedCurrentControlPoint = false;
				stateMachine.currentPatrolPoint = (stateMachine.currentPatrolPoint + 1) % stateMachine.PatrolPathManager.TrackPoints.Count;
			}

			Vector3 destination = stateMachine.PatrolPathManager.GetNextPatrolPointLocation(stateMachine.currentPatrolPoint);
			MoveToDestination(stateMachine.NpcDefinition.WalkSpeed, destination);
		}
		else if (HasValidAreaMove())
			MoveToDestination(stateMachine.NpcDefinition.WalkSpeed, stateMachine.RandomAreaMoveManager.GetRandomLocationInArea());
		else
			MoveToDestination(stateMachine.NpcDefinition.WalkSpeed, GetMoveLocationAroundNpc());
	}

	/// <summary>
	/// move to destination
	/// </summary>
	protected void MoveToDestination(float speed, Vector3 newDestination)
	{
		stateMachine.CurrentDestination = newDestination;
		stateMachine.Agent.isStopped = false;
		stateMachine.Agent.speed = speed;
		stateMachine.Agent.SetDestination(newDestination);
	}
	#endregion

	#region move to follow point checks
	private bool HasValidPatrolPoints()
	{
		if (stateMachine.movementType == NPCStateMachine.MovementType.patrolMove && stateMachine.PatrolPathManager != null)
			return true;
		else
			return false;

	}
	private bool HasValidAreaMove()
	{
		if (stateMachine.movementType == NPCStateMachine.MovementType.randomAreaMove && stateMachine.RandomAreaMoveManager != null)
			return true;
		else
			return false;
	}
	#endregion

	#region move to flee position at speed
	/// <summary>
	/// flee from passed in vector 3 argument, with a randomization angle
	/// </summary>
	protected void FleeToNewDestination(float speed, Vector3 positionToFleeFrom)
	{
		stateMachine.Agent.isStopped = false;
		stateMachine.Agent.speed = speed;

		//calculate flee direction with some randomization with angle
		Vector3 fleeDirection = (stateMachine.transform.position - positionToFleeFrom).normalized;
		float randomAngle = Random.Range(-45f, 45f);
		Quaternion rotation = Quaternion.Euler(0, randomAngle, 0);
		Vector3 randomFleeDirection = rotation * fleeDirection;

		Vector3 fleeDestination = stateMachine.transform.position + randomFleeDirection * (stateMachine.NpcDefinition.FleeDistance * 2);
		stateMachine.Agent.SetDestination(fleeDestination);
	}
	#endregion

	#region look around logic
	/// <summary>
	/// adjust look direction based on angle given in 360 degrees
	/// </summary>
	protected void LookAtDirection(float newLookAngle)
	{
		Vector3 directionToLookAt = Quaternion.Euler(0, newLookAngle, 0) * Vector3.forward;
		RotateTowardsDirection(directionToLookAt);
	}
	/// <summary>
	/// look at given position
	/// </summary>
	protected void LookAtDirection(Vector3 positionToLookAt)
	{
		Vector3 directionToLookAt = positionToLookAt - stateMachine.transform.position;
		directionToLookAt.y = 0f;
		RotateTowardsDirection(directionToLookAt);
	}
	private void RotateTowardsDirection(Vector3 directionToLookAt)
	{
		Quaternion targetRotation = Quaternion.LookRotation(directionToLookAt);

		if (directionToLookAt.sqrMagnitude > 0.01f)
		{
			stateMachine.transform.rotation = Quaternion.RotateTowards(
				stateMachine.transform.rotation, targetRotation, stateMachine.NpcDefinition.RotationSpeed * Time.deltaTime);
		}

		float angle = Quaternion.Angle(stateMachine.transform.rotation, targetRotation);
		lookingAtTarget = angle < 5f; // If angle below 2 degrees, its looking at target
	}
	#endregion

	#region destination reached check
	protected bool HasReachedDestination()
	{
		if (stateMachine.Agent.remainingDistance <= stateMachine.Agent.stoppingDistance)
			return true;
		else
			return false;
	}
	#endregion

	#region backup movement location (useful for drag and drop testing when not using a npc spawner)
	private Vector3 GetMoveLocationAroundNpc()
	{
		float radius = 10f;
		Vector3 randomDirection = Random.insideUnitSphere * radius + stateMachine.transform.position;

		while (true)
		{
			if (NavMesh.SamplePosition(randomDirection, out NavMeshHit navHit, radius, NavMesh.AllAreas))
				return navHit.position;  // Valid position found, return it
		}
	}
	#endregion
}
