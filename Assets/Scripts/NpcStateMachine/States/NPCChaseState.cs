using UnityEngine;
using Game.Core;

namespace Game.MyNPC
{
    public class NPCChaseState : NpcBaseMovementState
    {
        public NPCChaseState(NPCStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
			stateMachine.Beliefs.InvestigateLocation = null;
			MoveToDestination(stateMachine.NpcDefinition.SprintSpeed, stateMachine.NpcPerception.DetectedTarget.Transform.position);
        }

        public override void Tick(float deltaTime)
        {
			if (stateMachine.StatsHandler.LifeState == NpcDefinition.LifeState.dead) return;

			if (stateMachine.Beliefs.HasTarget)
				MoveToDestination(stateMachine.NpcDefinition.SprintSpeed, stateMachine.NpcPerception.DetectedTarget.Transform.position);
        }

        public override void Exit()
        {

        }
    }
}
