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

			if (!stateMachine.EnableMovement)
                stateMachine.SwitchState(new NPCIdleState(stateMachine));

			// ----------- Move to Idle -------------
			if (HasReachedDestination())
			{
				stateMachine.reachedCurrentControlPoint = true;
				stateMachine.SwitchState(new NPCIdleState(stateMachine));
				return;
			}

			#region State Transitions
			// ----------- Move Ranged Attack -------------
			if (stateMachine.TargetInShootingRange && stateMachine.EnableRangedAttack)
			{
				stateMachine.SwitchState(new NPCRangedAttackState(stateMachine));
				return;
			}

			// ----------- Move to Melee Attack -------------
			if (stateMachine.TargetInMeleeRange && stateMachine.EnableMeleeAttack)
			{
				stateMachine.SwitchState(new NPCMeleeAttackState(stateMachine));
				return;
			}

			// ----------- Move to Chase -------------
			if (stateMachine.NpcPerception.IsTargetDetected && stateMachine.EnableChase)
			{
				stateMachine.SwitchState(new NPCChaseState(stateMachine));
				return;
			}

			// ----------- Move to Eat Corpse -------------
			if (stateMachine.NpcPerception.IsEatableTargetDetected && stateMachine.EnableEatCorpseState)
			{
				stateMachine.SwitchState(new NPCEatCorpseState(stateMachine));
				return;
			}
			#endregion
		}
	}

}