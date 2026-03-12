using Game.MyNPC;
using UnityEngine;

public class NPCEatCorpseState : NPCBaseState
{
	public NPCEatCorpseState(NPCStateMachine stateMachine) : base(stateMachine) { }

	private bool LocationReached => ReachedCorpseLocation();

	/// <summary>
	/// for now just use timer, later it would be better to have a stat in StateHandler like float ZombificationProgress.
	/// basically multiple zombies allowed to eat a corpse, speading up progress and they only stop once 
	/// ZombificationProgress is complete and ignore other state switching (subject to change)
	/// </summary>

	private float eatCorpseDuration = 5f;
	private float eatCorpseTimer;

	public override void Enter()
	{
		eatCorpseTimer = eatCorpseDuration;
	}

	public override void Exit()
	{

	}

	public override void Tick(float deltaTime)
	{
		if (stateMachine.StatsHandler.IsDead) return;

		// move to position of cropse
		if (stateMachine.NpcPerception.EatableTarget)
		{
			if (!LocationReached)
			{
				stateMachine.Agent.SetDestination(stateMachine.NpcPerception.EatableTarget.transform.position);
				return;
			}
			else
			{
				//call animator eat animation, vfs,sfx call ZombificationProgress on eatable targets StatsHandler
				eatCorpseTimer -= deltaTime;
				if (eatCorpseTimer < 0)
				{
					stateMachine.NpcPerception.EatableTarget.GetComponent<NPCStateMachine>().CompleteZombification();
					stateMachine.SwitchState(new NPCIdleState(stateMachine));
				}
			}
		}
		else // no corpse to eat
		{
			stateMachine.SwitchState(new NPCIdleState(stateMachine));
		}
	}

	private bool ReachedCorpseLocation()
	{
		float distanceToLocation = Vector3.Distance(stateMachine.transform.position, stateMachine.NpcPerception.EatableTarget.transform.position);
		if (distanceToLocation < 1f)
			return true;
		else
			return false;
	}
}
