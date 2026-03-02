using Game.Core;
using UnityEngine;

namespace Game.MyPlayer
{
    public class PlayerCrouchIdleState : PlayerBaseState
    {
        public PlayerCrouchIdleState(PlayerStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            #region Enter Animation
            stateMachine.Animator.SetBool("IsCrouched", true);
            #endregion

            #region Change Camera Angle
            stateMachine.PlayerCamera.LevitateCamera = false;
            #endregion
        }

        public override void Exit()
        {
            #region Exit Animation  
            if (!stateMachine.InputReader.CrouchPressed)
            {
                stateMachine.Animator.SetBool("IsCrouched", false);
            }
            #endregion

            #region Change Camera Angle
            stateMachine.PlayerCamera.LevitateCamera = true;
            #endregion
        }

        public override void Tick(float deltaTime)
        {
            if (stateMachine.isDead) return;

            #region State Transitions

            // ----------- Crouch to Crouch Movement -------------
            if (stateMachine.InputReader.MovementInput.sqrMagnitude > 0.01f)
            {
                stateMachine.SwitchState(new PlayerCrouchMovementState(stateMachine));
            }

            // ----------- Crouch to Jump -------------
            if (stateMachine.InputReader.JumpTriggered)
            {
                //stateMachine.InputReader.JumpPressed = false;  
                stateMachine.SwitchState(new PlayerJumpState(stateMachine));
                return;
            }

            // ----------- Crouch to Idle -------------
            if (!stateMachine.InputReader.CrouchPressed)
            {
                stateMachine.SwitchState(new PlayerIdleState(stateMachine));
                return;
            }

            // ----------- Crouch to Attack -------------
            if (stateMachine.InputReader.AttackTriggered)
            {
                //stateMachine.SwitchState(new PlayerAttackState(stateMachine));
                //return;
            }

            #endregion

            #region Speed Deacceleration

            stateMachine.CurrentSpeed = Mathf.Lerp(stateMachine.CurrentSpeed, stateMachine.MinCrouchSpeed, stateMachine.SpeedChangeRate * deltaTime);

            #endregion

            #region Update Animation
            stateMachine.Animator.SetFloat("Speed", stateMachine.CurrentSpeed);
            #endregion
        }

        #region
        #endregion
    }
}
