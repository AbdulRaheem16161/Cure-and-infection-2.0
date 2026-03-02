using Game.MyPlayer;
using UnityEngine;

namespace Game.MyNPC
{
    public class NPCRangedAttackState : NPCBaseState
    {
        #region Constructor
        public NPCRangedAttackState(NPCStateMachine stateMachine) : base(stateMachine) { }
        #endregion

        #region Fields
        private float _attackDurationTimer;
        #endregion

        public override void Enter()
        {
            stateMachine.Agent.speed = 0f;
        }

        public override void Tick(float deltaTime)
        {
            if (stateMachine.isDead) return;

            Debug.Log("ranged attack state ticking");
            #region Update Attack Timer
            _attackDurationTimer += deltaTime;
            #endregion

            #region Shoot
            stateMachine.WeaponHolder.GetComponent<NPCWeaponController>().Shoot();
            #endregion

            #region State Transitions 

            // ----------- Attack to Chase -------------
            if (!stateMachine.OpponentInRangedAttackRange || !stateMachine.EnableRangedAttack)
            {
                stateMachine.SwitchState(new NPCChaseState(stateMachine));
                return;
            }

            // ----------- Ranged Attack to Melee Attack -------------
            if (stateMachine.OpponentInMeleeAttackRange && stateMachine.EnableMeleeAttack)
            {
                stateMachine.SwitchState(new NPCMeleeAttackState(stateMachine));
                return;
            }
            #endregion

            #region Rotate Towards Follow Point

            if (stateMachine.DetectionRadius.DetectedTarget != null)
            {
                stateMachine.CurrentFollowPoint = stateMachine.DetectionRadius.DetectedTarget.transform;
            }
            stateMachine.Agent.SetDestination(stateMachine.CurrentFollowPoint.position);

            Vector3 direction = (stateMachine.CurrentFollowPoint.position - stateMachine.transform.position).normalized;
            if (direction != Vector3.zero) // Prevent errors when NPC is exactly at the target
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                stateMachine.transform.rotation = Quaternion.RotateTowards(
                    stateMachine.transform.rotation,
                    targetRotation,
                    stateMachine.RangedAttackRotSpeed * deltaTime * 100f
                );
            }
            #endregion
        }


        public override void Exit()
        {
            
        }
    }
}

