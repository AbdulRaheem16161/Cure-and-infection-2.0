using Game.MyPlayer;
using UnityEngine;

namespace Game.MyNPC
{
    public class NPCMeleeAttackState : NPCBaseState
    {
        #region Constructor
        public NPCMeleeAttackState(NPCStateMachine stateMachine) : base(stateMachine) { }
        #endregion

        #region Fields
        private float _attackDurationTimer;
        #endregion

        public override void Enter()
        {
            #region Enter Animation
            stateMachine.Animator.SetTrigger("Attack");
            #endregion
        }

        public override void Tick(float deltaTime)
        {
            if (stateMachine.isDead) return;

            #region Update Attack Timer
            _attackDurationTimer += deltaTime;
            #endregion

            #region Enable Hitbox
            if (_attackDurationTimer >= stateMachine.HitboxActivationDelay)
            {
                if (stateMachine.Hitbox != null)
                    stateMachine.Hitbox.gameObject.SetActive(true);
            }

            #endregion

            #region State Transitions 

            // ----------- Attack to Idle -------------

            // Otherwise, return to Chase state
            if (_attackDurationTimer >= stateMachine.AttackDuration)
            {
                stateMachine.SwitchState(new NPCChaseState(stateMachine));
                return;
            }
            #endregion
        }

        public override void Exit()
        {
            #region Disable Hitbox
            if (stateMachine.Hitbox != null)
                stateMachine.Hitbox.gameObject.SetActive(false);
            #endregion
        }
    }
}

