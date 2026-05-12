using Game.Core;
using UnityEngine;

namespace Game.MyNPC
{
    public class NpcIdleMovementState : NpcBaseMovementState
	{
		public NpcIdleMovementState(NPCStateMachine stateMachine, int priority) : base(stateMachine, priority) { }

		private float idleTimer;

		public override bool IsValid()
		{
			return true; //always valid, with lowest priority
		}

		public override void Enter()
        {
			stateMachine.Agent.ResetPath();
        }

        public override void Exit()
        {

        }

        public override void Tick(float deltaTime)
        {
			if (stateMachine.StatsHandler.LifeState == EntityDefinition.LifeState.dead) return;

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
			idleTimer = Random.Range(stateMachine.Definition.MinIdleTime, stateMachine.Definition.MaxIdleTime);
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