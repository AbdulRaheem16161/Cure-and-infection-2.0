using UnityEngine;
using Game.Core;

namespace Game.MyNPC
{
    public class NPCChaseState : NpcBaseMovementState
    {
        public NPCChaseState(NPCStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
			MoveToDestination(stateMachine.NpcDefinition.ChaseSpeed, stateMachine.NpcPerception.DetectedTarget.Transform.position);
        }

        public override void Tick(float deltaTime)
        {
			if (stateMachine.StatsHandler.LifeState == NpcDefinition.LifeState.dead) return;

			if (stateMachine.TargetInChaseRange && stateMachine.NpcPerception.DetectedTarget != null)
				MoveToDestination(stateMachine.NpcDefinition.ChaseSpeed, stateMachine.NpcPerception.DetectedTarget.Transform.position);

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

			// ----------- Chase to Flee if no melee weapon -------------
			if (stateMachine.EnableFlee && stateMachine.TargetInFleeRange && !stateMachine.HasEquippedMeleeWeapon)
			{
				stateMachine.SwitchState(new NpcFleeState(stateMachine));
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
