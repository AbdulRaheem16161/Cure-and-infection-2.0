using Game.MyNPC;
using UnityEngine;

public class NPCEatCorpseState : NpcBaseMovementState
{
	public NPCEatCorpseState(NPCStateMachine stateMachine) : base(stateMachine) { }

	/// <summary>
	/// for now just use timer, later it would be better to have a stat in StateHandler like float ZombificationProgress.
	/// basically multiple zombies allowed to eat a corpse, speading up progress and they only stop once 
	/// ZombificationProgress is complete and ignore other state switching (subject to change)
	/// </summary>

	private readonly float eatCorpseDuration = 5f;
	private float eatCorpseTimer;

	public override void Enter()
	{
		eatCorpseTimer = eatCorpseDuration;
		MoveToNewDestination(NpcMoveType.moveToCorpse);
	}

	public override void Exit()
	{

	}

	public override void Tick(float deltaTime)
	{
		if (stateMachine.StatsHandler.IsDead) return;

		//return to idle state
		if (!stateMachine.NpcPerception.IsEatableTargetDetected)
		{
			stateMachine.SwitchState(new NPCIdleState(stateMachine));
			return;
		}

		// move to position of cropse
		if (HasReachedDestination())
		{
			eatCorpseTimer -= deltaTime;
			if (eatCorpseTimer > 0) return;

			Debug.LogError("timer done");
			stateMachine.NpcPerception.EatableTarget.StatsHandler.GetComponent<NPCStateMachine>().CompleteZombification();
			stateMachine.SwitchState(new NPCIdleState(stateMachine));
			return;
		}
	}
}
