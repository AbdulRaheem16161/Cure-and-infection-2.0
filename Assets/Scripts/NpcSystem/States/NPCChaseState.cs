using UnityEngine;
using Game.Core;

namespace Game.MyNPC
{
    public class NPCChaseState : NpcBaseMovementState
    {
		public NPCChaseState(NPCStateMachine stateMachine, int priority) : base(stateMachine, priority) { }

		public override bool IsValid()
		{
			return stateMachine.capabilityOverrides.HasFlag(EntityDefinition.Capability.chase) &&
				Beliefs.FleeTarget == null && Beliefs.Target != null;
		}

		public override void Enter()
        {
			stateMachine.Beliefs.SetNewInvestigateLocation(null);
        }

        public override void Tick(float deltaTime)
        {
            if (stateMachine.StatsHandler.LifeState == EntityDefinition.LifeState.dead) return;

            if (stateMachine.Beliefs.Target != null)
				MoveToDestination(stateMachine.Beliefs.Target.Position, MoveType.sprint);
        }

        public override void Exit()
        {

        }
    }
}
