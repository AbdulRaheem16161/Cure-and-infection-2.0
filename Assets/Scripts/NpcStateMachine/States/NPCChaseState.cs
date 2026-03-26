using UnityEngine;
using Game.Core;

namespace Game.MyNPC
{
    public class NPCChaseState : NpcBaseMovementState
    {
        public NPCChaseState(NPCStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            MoveToNewDestination(NpcMoveType.moveToTarget);
        }

        public override void Tick(float deltaTime)
        {
            if (stateMachine.StatsHandler.IsDead) return;

            if (stateMachine.TargetInChaseRange)
			    MoveToNewDestination(NpcMoveType.moveToTarget);

			#region State Transitions
			// ----------- Chase to Ranged Attack -------------

			if (stateMachine.TargetInShootingRange && stateMachine.HasEquippedRangedWeapon && stateMachine.EnableRangedAttack)
			{
				stateMachine.SwitchState(new NPCRangedAttackState(stateMachine));
				return;
			}

			// ----------- Chase to Melee Attack -------------

			if (stateMachine.TargetInMeleeRange && stateMachine.HasEquippedMeleeWeapon && stateMachine.EnableMeleeAttack)
			{
				stateMachine.SwitchState(new NPCMeleeAttackState(stateMachine));
				return;
			}

			// ----------- Chase to idle -------------

			if (!stateMachine.NpcPerception.IsTargetDetected)
            {
                stateMachine.SwitchState(new NPCIdleState(stateMachine));
                return;
            }
            #endregion
        }

        public override void Exit()
        {

        }
    }
}
