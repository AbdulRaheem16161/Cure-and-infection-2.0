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
            if (stateMachine.isDead) return;

            #region State Transitions

            // ----------- Chase to idle -------------

            if (!stateMachine.DetectionRadius.isTargetDetected && !stateMachine.DetectionCone.isTargetDetected)
            {
                stateMachine.SwitchState(new NPCIdleState(stateMachine));
                return;
            }
            // ----------- Chase to Melee Attack -------------

            if (stateMachine.OpponentInMeleeAttackRange && stateMachine.EnableMeleeAttack)
            {
                stateMachine.SwitchState(new NPCMeleeAttackState(stateMachine));
                return;
            }

            // ----------- Idle to Ranged Attack -------------

            if (stateMachine.OpponentInRangedAttackRange && stateMachine.EnableRangedAttack)
            {
                stateMachine.SwitchState(new NPCRangedAttackState(stateMachine));
                return;
            }
            #endregion

            if (!stateMachine.isDead)
            {
                stateMachine.CurrentFollowPoint = stateMachine.DetectionRadius.DetectedTarget.transform;
                stateMachine.Agent.SetDestination(stateMachine.CurrentFollowPoint.position);
            }
        }

        public override void Exit()
        {
        }
    }
}
