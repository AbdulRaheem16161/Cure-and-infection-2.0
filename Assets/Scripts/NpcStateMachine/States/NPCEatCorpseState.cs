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

	private bool eatingCorpse;
	private bool ateCorpse;
	private readonly float eatCorpseDuration = 5f;
	private float eatCorpseTimer;

	public override void Enter()
	{
		eatingCorpse = false;
		ateCorpse = false;
		eatCorpseTimer = eatCorpseDuration;
		MoveToDestination(stateMachine.NpcDefinition.WalkSpeed, stateMachine.Beliefs.EatableTarget.Transform.position);
	}

	public override void Exit()
	{
		stateMachine.Agent.isStopped = false;
	}

	public override void Tick(float deltaTime)
	{
		if (stateMachine.StatsHandler.LifeState == NpcDefinition.LifeState.dead) return;

		// move to position of cropse
		if (HasReachedCorpse()) //needs a litle more room then agent stopping distance 
		{
			eatingCorpse = true;
		}
		if (eatingCorpse)
		{
			eatCorpseTimer -= deltaTime;
			stateMachine.Agent.isStopped = true;
			if (eatCorpseTimer > 0) return;

			if (!ateCorpse)
			{
				ateCorpse = true;
				stateMachine.Beliefs.Target.StatsHandler.CompleteZombification();
			}
		}
	}
}
