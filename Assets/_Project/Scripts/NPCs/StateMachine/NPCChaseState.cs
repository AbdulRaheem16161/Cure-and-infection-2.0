using UnityEngine;
using Game.Core;

namespace Game.MyNPC
{
    public class NPCChaseState : NPCBaseState
    {
        public NPCChaseState(NPCStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            stateMachine.Agent.speed = stateMachine.ChaseSpeed;
        }

        public override void Tick(float deltaTime)
        {
            if (stateMachine.StatsHandler.IsDead) return;

            #region State Transitions

            // ----------- Chase to idle -------------

            if (!stateMachine.NpcPerception.isTargetDetected)
            {
                stateMachine.SwitchState(new NPCIdleState(stateMachine));
                return;
            }
            // ----------- Chase to Melee Attack -------------

            if (stateMachine.OpponentInMeleeAttackRange && stateMachine.HasEquippedMeleeWeapon && stateMachine.EnableMeleeAttack)
            {
                stateMachine.SwitchState(new NPCMeleeAttackState(stateMachine));
                return;
            }

            // ----------- Idle to Ranged Attack -------------

            if (stateMachine.OpponentInRangedAttackRange && stateMachine.HasEquippedRangedWeapon && stateMachine.EnableRangedAttack)
            {
                stateMachine.SwitchState(new NPCRangedAttackState(stateMachine));
                return;
            }
            #endregion

            if (!stateMachine.StatsHandler.IsDead)
            {
                stateMachine.CurrentFollowPoint = stateMachine.NpcPerception.DetectedTarget.transform;
                stateMachine.Agent.SetDestination(stateMachine.CurrentFollowPoint.position);
            }
        }

        public override void Exit()
        {
        }
    }
}
