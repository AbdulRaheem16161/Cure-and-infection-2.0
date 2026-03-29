using Game.MyNPC;
using UnityEngine;
using UnityEngine.AI;

public class NpcBaseMovementState : NPCBaseState
{
	protected bool lookingAtTarget;

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
		if (stateMachine.StatsHandler.LifeState == NpcDefinition.LifeState.dead) return;

		if (npcMoveType == NpcMoveType.regularMove)
		{
			if (HasValidPatrolFollowPoint())
			{
				//loop through control points and grab next if has reached current point, else continue to current one
				if (stateMachine.reachedCurrentControlPoint)
				{
					stateMachine.reachedCurrentControlPoint = false;
					stateMachine.currentPatrolPoint = (stateMachine.currentPatrolPoint + 1) % stateMachine.PatrolPoints.TrackPoints.Count;
				}

				Vector3 destination = stateMachine.PatrolPoints.GetNextPatrolPointLocation(stateMachine.currentPatrolPoint);
				MoveToNewDestination(stateMachine.PatrolSpeed, destination);
			}
			else
			{
				if (HasValidRandomFollowPoint())
					MoveToNewDestination(stateMachine.PatrolSpeed, stateMachine.RandomMovementManager.GetRandomLocationInArea());
				else if (stateMachine.useBackupMovement)
					MoveToNewDestination(stateMachine.PatrolSpeed, GetBackUpMovementLocationAroundNpc());
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

		else if (npcMoveType == NpcMoveType.moveToInvestigate)
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

	#region move to position at speed
	private void MoveToNewDestination(float speed, Vector3 newDestination)
	{
		stateMachine.Agent.isStopped = false;
		stateMachine.Agent.speed = speed;
		stateMachine.Agent.SetDestination(newDestination);
	}
	#endregion

	#region look around logic
	/// <summary>
	/// calculate the direction vector to look at based on given angle
	/// </summary>
	protected void LookAtDirection(float newLookAngle)
	{
		Vector3 directionToLookAt = Quaternion.Euler(0, newLookAngle, 0) * Vector3.forward;
		RotateTowardsDirection(directionToLookAt);
	}
	/// <summary>
	/// calculate direction vector to look at
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
				stateMachine.transform.rotation, targetRotation, stateMachine.RotationSpeed * Time.deltaTime);
		}

		float angle = Quaternion.Angle(stateMachine.transform.rotation, targetRotation);
		lookingAtTarget = angle < 2f; // If angle below 2 degrees, its looking at target
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

	#region backup movement location (useful for drag and drop testing when not using a npc spawner)
	private Vector3 GetBackUpMovementLocationAroundNpc()
	{
		float radius = 10f;
		Vector3 randomDirection = Random.insideUnitSphere * radius;
		randomDirection += stateMachine.transform.position;

		if (NavMesh.SamplePosition(randomDirection, out NavMeshHit navHit, radius, NavMesh.AllAreas))
			return navHit.position;
		return stateMachine.transform.position;
	}
	#endregion
}
