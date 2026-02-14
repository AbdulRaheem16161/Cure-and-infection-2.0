using Game.Core;
using UnityEngine;

namespace Game.MyPlayer
{
    public class PlayerAttackState : PlayerBaseState
    {
        #region Fields
        private float _attackDurationTimer;
        #endregion

        #region Constructor
        public PlayerAttackState(PlayerStateMachine stateMachine) : base(stateMachine) { }
        #endregion

        public override void Enter()
        {
            #region Enter Animation
            stateMachine.Animator.SetTrigger("Attack");
            #endregion

            #region Initialize  
            stateMachine.InputReader.AttackTriggered = false;
            _attackDurationTimer = 0f;
            #endregion

            #region Enable Hitbox
            if (stateMachine.Hitbox != null)
                stateMachine.Hitbox.gameObject.SetActive(true);
            #endregion
        }



        public override void Tick(float deltaTime)
        {
            #region Update Attack Timer
            _attackDurationTimer += deltaTime;
            #endregion

            #region Handle Attack Completion  

            // Check if the attack duration has finished; if so, disable the hitbox
            if (_attackDurationTimer >= stateMachine.AttackDuration)
            {
                if (stateMachine.Hitbox != null)
                    stateMachine.Hitbox.gameObject.SetActive(false);
            }
            #endregion

            #region State Transitions 

            // Determine next state after attack based on player input

            // ----------- Attack to Crouch Idle -------------

            // If the player is holding crouch, transition to Crouch Idle
            if (_attackDurationTimer >= stateMachine.AttackDuration && stateMachine.InputReader.CrouchPressed)
            {
                stateMachine.SwitchState(new PlayerCrouchMovementState(stateMachine));  ///////////////
                return;
            }

            // ----------- Attack to Idle -------------

            // Otherwise, return to standard Idle state
            if (_attackDurationTimer >= stateMachine.AttackDuration && !stateMachine.InputReader.CrouchPressed)
            {
                stateMachine.SwitchState(new PlayerMoveState(stateMachine)); ////////////////////////
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
