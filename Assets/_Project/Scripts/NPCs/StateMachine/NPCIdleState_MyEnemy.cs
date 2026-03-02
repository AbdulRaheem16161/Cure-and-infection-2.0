using Game.Core;
using UnityEngine;

namespace Game.MyNPC
{
    public class NPCIdleState : NPCBaseState
    {
        private float WaitBeforeFreeMove;
        private float timer;

        public NPCIdleState(NPCStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            #region Enter Animation
            // Pick a random wait time between 2 and 5 seconds (customize these)
            WaitBeforeFreeMove = Random.Range(stateMachine.MinWaitBeforeFreeMove, stateMachine.MaxWaitBeforeFreeMove);
            timer = 0f;

            // Set idle animation speed to 0
            stateMachine.Animator.SetFloat("Speed", 0f);
            #endregion

            #region Change Speed
            stateMachine.Agent.speed = 0f;
            #endregion
        }

        public override void Exit()
        {
   
        }

        public override void Tick(float deltaTime)
        {
            if (stateMachine.isDead) return;

            #region State Transitions

            // ----------- Idle to Free Move -------------
            timer += deltaTime;

            // Wait until random time is over
            if (timer >= WaitBeforeFreeMove && stateMachine.EnableFreeMove) 
            {
                stateMachine.SwitchState(new NPCFreeMoveState(stateMachine));
                return;
            }

            // ----------- Idle to Chase -------------

            if (stateMachine.DetectionCone.isTargetDetected && stateMachine.EnableChase)
            {
                stateMachine.SwitchState(new NPCChaseState(stateMachine));
                return;
            }

            // ----------- Idle to Melee Attack -------------

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

            #region Update Animation
            // Keep idle anim updated (optional if Animator handles it)
            stateMachine.Animator.SetFloat("Speed", stateMachine.CurrentSpeed);
            #endregion
        }
    }
}
