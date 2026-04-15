using UnityEngine;
using Game.Core;

namespace Game.MyNPC
{
    public class NPCChaseState : NpcBaseMovementState
    {
		public NPCChaseState(NPCStateMachine stateMachine, int priority) : base(stateMachine, priority) { }

		public override bool IsValid()
		{
			return stateMachine.capabilityOverrides.HasFlag(EntityDefinition.Capability.chase) 
				&& Beliefs.TargetFleeingFrom == null && Beliefs.Target != null && !Beliefs.TargetInShootingRange && !Beliefs.TargetInMeleeRange;
		}

		public override void Enter()
        {
			stateMachine.Beliefs.SetNewInvestigateLocation(null);
        }

        public override void Tick(float deltaTime)
        {
			if (stateMachine.StatsHandler.LifeState == EntityDefinition.LifeState.dead) return;

			if (stateMachine.Beliefs.Target != null)
				MoveToDestination(stateMachine.Beliefs.Target.Transform.position, MoveType.sprint);
        }

        public override void Exit()
        {

        }
    }
}
