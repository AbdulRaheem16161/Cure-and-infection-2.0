using Game.Core;
using UnityEngine;

namespace Game.MyPlayer
{
    public class PlayerSprintState : PlayerBaseState
    {
        public PlayerSprintState(PlayerStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {

        }

        public override void Exit()
        {

        }

        public override void Tick(float deltaTime)
        {
            #region State Transitions

            // ----------- Sprint to Idle -------------
            Vector2 input = stateMachine.InputReader.MovementInput;

            if (input.sqrMagnitude < 0.01f || !stateMachine.InputReader.IsSprinting)
            {
                stateMachine.SwitchState(new PlayerMoveState(stateMachine)); //////////////////
                return;
            }

            // ----------- Sprint to Jump -------------
            if (stateMachine.InputReader.JumpTriggered)
            {
                stateMachine.SwitchState(new PlayerJumpState(stateMachine));
                return;
            }

            // ----------- Sprint to Crouch -------------
            if (stateMachine.InputReader.CrouchPressed)
            {
                stateMachine.SwitchState(new PlayerCrouchMovementState(stateMachine)); //////////////
                return;
            }

            // ----------- Sprint to Attack -------------
            if (stateMachine.InputReader.AttackTriggered)
            {
                stateMachine.SwitchState(new PlayerAttackState(stateMachine));
                return;
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
            stateMachine.CurrentSpeed = Mathf.Lerp(stateMachine.CurrentSpeed, stateMachine.SprintSpeed, stateMachine.SpeedChangeRate * deltaTime); ////////// changed sprintSpeed to MaxSprintSpeed

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
