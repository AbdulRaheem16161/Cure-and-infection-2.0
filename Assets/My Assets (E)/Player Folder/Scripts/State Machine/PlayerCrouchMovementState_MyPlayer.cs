using Game.Core;
using UnityEngine;

namespace Game.MyPlayer
{
    public class PlayerCrouchMovementState : PlayerBaseState
    {
        public PlayerCrouchMovementState(PlayerStateMachine stateMachine) : base(stateMachine) { }

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
            #region Change Camera Angle
            stateMachine.PlayerCamera.LevitateCamera = true;
            #endregion

            #region Exit Animation  
            if (!stateMachine.InputReader.CrouchPressed)
            {
                stateMachine.Animator.SetBool("IsCrouched", false);
            }
            #endregion
        }

        public override void Tick(float deltaTime)
        {
            if (stateMachine.isDead) return;

            Vector2 input = stateMachine.InputReader.MovementInput;

            #region State Transitions

            // ----------- Crouch Movement to Crouch Idle ------------- 


            if (input.sqrMagnitude < 0.01f) 
            {
                stateMachine.SwitchState(new PlayerCrouchIdleState(stateMachine));
                return;
            }

            // ----------- Crouch Movement to Jump -------------
            if (stateMachine.InputReader.JumpTriggered)
            {
                //stateMachine.InputReader.JumpPressed = false; // Consume the input
                stateMachine.SwitchState(new PlayerJumpState(stateMachine));
                return;
            }

            // ----------- Crouch Movement to Idle -------------
            if (!stateMachine.InputReader.CrouchPressed)
            {
                stateMachine.SwitchState(new PlayerMoveState(stateMachine)); /////////////////
                return;
            }

            // ----------- Crouch Movement to Attack -------------
            if (stateMachine.InputReader.AttackTriggered)
            {
                //stateMachine.SwitchState(new PlayerAttackState(stateMachine));
                //return;
            }

            #endregion

            #region Camera-Oriented Movement
            /// <summary>
            /// Aligns player input with the camera’s facing direction, 
            /// so movement feels natural relative to what the player sees.
            /// Also rotates the character’s body to face the direction of movement.
            /// </summary>

            // Get camera direction (ignoring vertical tilt like looking up/down)
            Vector3 camForward = stateMachine.CameraTransform.forward;
            Vector3 camRight = stateMachine.CameraTransform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            // Combine input with camera orientation
            Vector3 moveDir = camForward * input.y + camRight * input.x;
            moveDir.Normalize();

            // Rotate body towards movement direction if input is significant
            if (moveDir.magnitude > 0.1f)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                stateMachine.Body.rotation = Quaternion.Slerp(
                    stateMachine.Body.rotation,
                    targetRot,
                    stateMachine.RotationSpeed * deltaTime
                );
            }

            #endregion

            #region Speed Acceleration

            // Smoothly accelerate current speed
            stateMachine.CurrentSpeed = Mathf.Lerp(stateMachine.CurrentSpeed, stateMachine.MaxCrouchSpeed, stateMachine.SpeedChangeRate * deltaTime);

            #endregion

            #region Movement

            // Actual Movement
            stateMachine.Controller.Move(moveDir * stateMachine.CurrentSpeed * deltaTime);

            #endregion

            #region Update Animation
            stateMachine.Animator.SetFloat("Speed", stateMachine.CurrentSpeed);
            #endregion
        }
    }
}
