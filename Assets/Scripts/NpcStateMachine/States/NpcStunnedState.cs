using Game.MyNPC;
using UnityEngine;

public class NpcStunnedState : NPCBaseState
{
	private readonly float stunCooldown = 2f;
	private float stunTimer;

	public NpcStunnedState(NPCStateMachine stateMachine) : base(stateMachine) { }

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
