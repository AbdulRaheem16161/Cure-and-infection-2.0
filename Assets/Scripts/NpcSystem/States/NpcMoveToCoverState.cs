using Game.MyNPC;
using UnityEngine;

public class NpcMoveToCoverState : NpcBaseMovementState
{
	public NpcMoveToCoverState(NPCStateMachine stateMachine, int priority) : base(stateMachine, priority) { }

	private int timeToSpendMovingToCover;
	private float timeSpentMovingToCover;

	private readonly System.Random systemRandom = new();

	public override bool IsValid()
	{
		return stateMachine.capabilityOverrides.HasFlag(EntityDefinition.Capability.rangedAttack) && Beliefs.FleeTarget == null &&
			Beliefs.RangedWeaponInHands && Beliefs.MovingToCover && !Beliefs.ReturnFire;
	}

	public override void Enter()
	{
		timeToSpendMovingToCover = systemRandom.Next(2, 4);
		timeSpentMovingToCover = 0;
		MoveToCover();
	}

	public override void Exit()
	{

	}

	public override void Tick(float deltaTime)
	{
        if (stateMachine.StatsHandler.LifeState == EntityDefinition.LifeState.dead) return;

        HandleReachingCover(deltaTime);
	}

	#region Move To Cover
	private void MoveToCover()
	{
		if (Beliefs.CoverPosition.HasValue)
		{
			Vector3 coverMovePosition = Beliefs.CoverPosition.Value;
			MoveToDestination(coverMovePosition, MoveType.sprint);
			Beliefs.SetNewLookDirection(Beliefs.Target.Position);
		}
	}
	#endregion

	#region Handle Reaching Cover
	private void HandleReachingCover(float deltaTime)
	{
		if (Beliefs.MovingToCover && HasReachedDestination())
		{
			LookAtDirection(Beliefs.LookDirection.Value);
			if (!lookingAtTarget) return;

			Beliefs.InCover = true;
			Beliefs.UpdateCoverPosition(null);

			return;
		}

		timeSpentMovingToCover += deltaTime;

		if (timeSpentMovingToCover >= timeToSpendMovingToCover)
		{
			LookAtDirection(Beliefs.LookDirection.Value);

			if (!lookingAtTarget) return;

			Beliefs.ReturnFire = true;
			timeSpentMovingToCover = 0f;
		}
	}
	#endregion
}
