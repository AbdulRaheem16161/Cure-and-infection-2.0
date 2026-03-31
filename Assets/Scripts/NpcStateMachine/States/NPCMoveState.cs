using Game.Core;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Game.MyNPC
{
    public class NPCMoveState : NpcBaseMovementState
    {
        public NPCMoveState(NPCStateMachine stateMachine) : base(stateMachine) { }
        public override void Enter()
        {
			HandleMovementLogic();
        }

        public override void Exit()
        {

        }

        public override void Tick(float deltaTime)
        {
			if (stateMachine.StatsHandler.LifeState == NpcDefinition.LifeState.dead) return;

			if (HasReachedDestination())
			{
				stateMachine.reachedCurrentControlPoint = true;
                stateMachine.Beliefs.Idling = true;
			}
		}
	}

}