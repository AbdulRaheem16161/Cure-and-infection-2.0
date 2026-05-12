using Game.MyNPC;
using UnityEngine;

public class NpcStunnedState : NPCBaseState
{
	private readonly float stunCooldown = 2f;
	private float stunTimer;

	public NpcStunnedState(NPCStateMachine stateMachine, int priority) : base(stateMachine, priority) { }

	public override bool IsValid()
	{
		return stateMachine.Beliefs.Stunned;
	}

	public override void Enter()
	{
		//play stun animation etc...
		stateMachine.Agent.isStopped = true;
		stunTimer = stunCooldown;
	}

	public override void Exit()
	{

	}

	public override void Tick(float deltaTime)
	{
		stunTimer -= deltaTime;
		if (stunTimer <= 0)
			stateMachine.Beliefs.Stunned = false;
	}
}
