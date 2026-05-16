using Game.MyNPC;
using System.IO;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.AI;

public class NpcBaseMovementState : NPCBaseState
{
	private Door doorInMovePath;

	private LayerMask interactablesLineOfSightMask = LayerMask.GetMask("Environment", "EnvironmentCover", "Interactable");

    protected bool lookingAtTarget;
	public enum MoveType { walk, sprint }
	private readonly float[] navMeshSamplingRadius = { 1f, 5f, 10f};
	public NpcBaseMovementState(NPCStateMachine stateMachine, int priority) : base(stateMachine, priority) { }

	public override bool IsValid()
	{
		return false;
	}

	public override void Enter()
	{

	}

	public override void Exit()
	{

	}

	public override void Tick(float deltaTime)
	{

	}

	#region Handle Movement Types Logic
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
			MoveToDestination(destination, MoveType.walk);
		}
		else if (HasValidAreaMove())
			MoveToDestination(stateMachine.RandomAreaMoveManager.GetRandomLocationInArea(), MoveType.walk);
		else
			MoveToDestination(GetMoveLocationAroundNpc(), MoveType.walk);
	}
	#endregion

	#region Flee Position Logic
	/// <summary>
	/// flee from passed in vector 3 argument, with a randomization angle
	/// </summary>
	protected void FleeFromPosition(Vector3 positionToFleeFrom)
	{
		//calculate flee direction with some randomization with angle
		Vector3 fleeDirection = (stateMachine.transform.position - positionToFleeFrom).normalized;
		float randomAngle = Random.Range(-45f, 45f);
		Quaternion rotation = Quaternion.Euler(0, randomAngle, 0);
		Vector3 randomFleeDirection = rotation * fleeDirection;
		Vector3 fleeDestination = stateMachine.transform.position + randomFleeDirection * 10;

		MoveToDestination(fleeDestination, MoveType.sprint);
	}
	#endregion

	#region Shared Move To Destination Method
	/// <summary>
	/// move to destination
	/// </summary>
	protected void MoveToDestination(Vector3 newDestination, MoveType moveIntent)
	{
		if (!TrySampleMovePosition(newDestination, out Vector3 sampledPosition))
		{
			Debug.LogError($"NavMesh sampling failed for destination: {newDestination}\n" +
				$"Agent Pos: {stateMachine.Agent.transform.position}\n Max Radius Tried: {navMeshSamplingRadius[^1]}");
			return;
		}

		if (!GetValidMovePath(sampledPosition, out NavMeshPath path))
		{
			Debug.LogWarning($"No valid path found for destination {newDestination}");
			return;
		}

		stateMachine.Beliefs.InCover = false;
		stateMachine.CurrentDestination = sampledPosition;
		stateMachine.Agent.isStopped = false;
		stateMachine.Agent.updatePosition = true;

		UpdateMoveSpeed(moveIntent);
		stateMachine.Agent.SetPath(path);
	}
	private bool GetValidMovePath(Vector3 sampledPosition, out NavMeshPath path)
	{
		path = new NavMeshPath();
		bool pathCalculated = NavMesh.CalculatePath(stateMachine.transform.position, sampledPosition, NavMesh.AllAreas, path);

		if (!pathCalculated || path.status != NavMeshPathStatus.PathComplete) //path invalid or incomplete
			return false;

		return true;
	}
	private bool TrySampleMovePosition(Vector3 newDestination, out Vector3 sampledPosition)
	{
		foreach (float radius in navMeshSamplingRadius)
		{
			if (NavMesh.SamplePosition(newDestination, out NavMeshHit hit, radius, NavMesh.AllAreas))
			{
				sampledPosition = hit.position;
				return true;
			}
		}
		sampledPosition = Vector3.zero;
		return false;
	}
	/// <summary>
	/// set move speed to walk/sprint speed based on intent and npc exhaustion state
	/// </summary>
	public void UpdateMoveSpeed(MoveType moveIntent)
	{
		bool canSprint = moveIntent == MoveType.sprint && !stateMachine.StatsHandler.IsExhausted;

		stateMachine.Agent.speed = moveIntent switch
		{
			MoveType.sprint when canSprint => stateMachine.Definition.SprintSpeed,
			_ => stateMachine.Definition.WalkSpeed
		};

		stateMachine.moveIntent = moveIntent;
		stateMachine.IsSprinting = canSprint;
	}
	#endregion

	#region Valid Patrol and Area Move Point Checks
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

	#region Look At Angle/Direction Logic
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
		stateMachine.Agent.isStopped = true;
		stateMachine.Agent.updateRotation = false;
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
				stateMachine.transform.rotation, targetRotation, stateMachine.Definition.RotationSpeed * Time.deltaTime);
		}

		float angle = Quaternion.Angle(stateMachine.transform.rotation, targetRotation);
		lookingAtTarget = angle < 5f; // If angle below 5 degrees, its looking at target
	}
	#endregion

	#region Destination Reached and Distance Checks
	protected bool HasReachedDestination()
	{
		if (stateMachine.Agent.remainingDistance <= stateMachine.Agent.stoppingDistance)
			return true;
		else
			return false;
	}
	protected bool IsWithinDistanceOfDestination(float distanceThreshold)
	{
		if (stateMachine.Agent.remainingDistance <= distanceThreshold)
			return true;
		else
			return false;
	}
	#endregion

	#region Backup Move Locations (useful for drag and drop testing when not using a npc spawner)
	private Vector3 GetMoveLocationAroundNpc()
	{
		float radius = 25f;
		Vector3 randomDirection = Random.insideUnitSphere * radius + stateMachine.transform.position;

		while (true)
		{
			if (NavMesh.SamplePosition(randomDirection, out NavMeshHit navHit, radius, NavMesh.AllAreas))
				return navHit.position;  // Valid position found, return it
		}
	}
    #endregion

    #region Door In Move Path Check
    public void DoorInMovePathCheck()
	{
		if (!IsTurning()) return;

        Vector3 direction = stateMachine.Agent.desiredVelocity.normalized;
		Debug.DrawRay(stateMachine.transform.position, direction * 2f, Color.green);

        if (Physics.Raycast(stateMachine.transform.position, direction, out RaycastHit hit, 1000, interactablesLineOfSightMask))
        {
			if (hit.transform.TryGetComponent(out Door door))
			{
                doorInMovePath = door;
                Debug.Log($"Hit: {hit.collider.name}");
            }
        }
	}
    private bool IsTurning(float angleThreshold = 1f)
    {
        Vector3 desiredDir = stateMachine.Agent.desiredVelocity;

        if (desiredDir.sqrMagnitude < 0.01f)
            return false;

        float angle = Vector3.Angle(stateMachine.transform.forward, desiredDir.normalized);
        return angle > angleThreshold;
    }
    #endregion
}
