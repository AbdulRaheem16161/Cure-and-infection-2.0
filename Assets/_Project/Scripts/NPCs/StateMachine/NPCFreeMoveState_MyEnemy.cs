using Game.Core;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Game.MyNPC
{
    public class NPCFreeMoveState : NPCBaseState
    {
        public NPCFreeMoveState(NPCStateMachine stateMachine) : base(stateMachine) { }
        public override void Enter()
        {
            stateMachine.Agent.speed = stateMachine.PatrolSpeed;
        }

        public override void Exit()
        {
            #region Exit Animation
            #endregion
        }

        public override void Tick(float deltaTime)
        {
            if (stateMachine.isDead) return;

            #region State Transitions

            // ----------- Free Move to Chase -------------

            if (stateMachine.DetectionCone.isTargetDetected && stateMachine.EnableChase)
            {
                stateMachine.SwitchState(new NPCChaseState(stateMachine));
                return;
            }

            // ----------- Free Move to Melee Attack -------------

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

            #region Destination Assignment

            // Patrol if MoveOnStraightPath is Selected in the EnemyStateMachine
            if (stateMachine.moveOnPatrolPath && stateMachine.PatrolFollowPoint != null)
            {
                stateMachine.CurrentFollowPoint = stateMachine.PatrolFollowPoint;
                stateMachine.Agent.SetDestination(stateMachine.CurrentFollowPoint.position);
                return;
            }

            // Do Random Movement if MoveOnRandomPath is Selected in the EnemyStateMachine
            if (stateMachine.moveOnRandomPath && stateMachine.RandomFollowPoint != null)
            {
                stateMachine.CurrentFollowPoint = stateMachine.RandomFollowPoint;
                stateMachine.Agent.SetDestination(stateMachine.CurrentFollowPoint.position);
                return;
            }

            #endregion
        }
    }
    
}