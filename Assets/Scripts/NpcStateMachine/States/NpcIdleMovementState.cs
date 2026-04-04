using Game.Core;
using UnityEngine;

namespace Game.MyNPC
{
    public class NpcIdleMovementState : NpcBaseMovementState
	{
		private float idleTimer;

		public NpcIdleMovementState(NPCStateMachine stateMachine) : base(stateMachine) { }
        public override void Enter()
        {

        }

        public override void Exit()
        {

        }

        public override void Tick(float deltaTime)
        {
			if (stateMachine.StatsHandler.LifeState == NpcDefinition.LifeState.dead) return;

			if (stateMachine.Animator.speed != stateMachine.CurrentSpeed)
				stateMachine.Animator.SetFloat("Speed", stateMachine.CurrentSpeed);

			if (stateMachine.Beliefs.Idling)
			{
				idleTimer -= deltaTime;

				if (idleTimer <= 0)
					BeginMove();
			}
			else
			{
				if (HasReachedDestination())
					BeginIdle();
			}
		}

		private void BeginMove()
		{
			stateMachine.Beliefs.Idling = false;
			HandleMovementLogic();
			idleTimer = Random.Range(stateMachine.NpcDefinition.MinIdleTime, stateMachine.NpcDefinition.MaxIdleTime);
		}
		private void BeginIdle()
		{
			stateMachine.Beliefs.Idling = true;
			stateMachine.Agent.isStopped = true;
			if (stateMachine.Agent.hasPath)
				stateMachine.reachedCurrentControlPoint = true;
		}
	}
}