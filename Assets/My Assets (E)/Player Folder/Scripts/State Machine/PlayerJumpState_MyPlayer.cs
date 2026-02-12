using Game.Core;
using UnityEngine;

namespace Game.MyPlayer
{
    public class PlayerJumpState : PlayerBaseState
    {
        private Vector3 airMomentum;

        private float _defaultGravity;

        public PlayerJumpState(PlayerStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            #region Enter Animation
            stateMachine.Animator.SetTrigger("Jump");
            #endregion

            #region Store Gravity
            _defaultGravity = stateMachine.Gravity;
            stateMachine.Gravity = stateMachine.JumpGravity;
            #endregion

            #region Initialize  

            // No Longer in Crouch State
            stateMachine.InputReader.CrouchPressed = false;

            // Reset the input so it won’t spam
            stateMachine.InputReader.JumpTriggered = false;

            #endregion

            #region Jump Physics

            // Calculate vertical jump force
            stateMachine.VerticalVelocity = Mathf.Sqrt(stateMachine.JumpHeight * -2f * stateMachine.Gravity);

            // Store horizontal momentum at jump start
            Vector2 input = stateMachine.InputReader.MovementInput;
            Vector3 camForward = stateMachine.CameraTransform.forward;
            Vector3 camRight = stateMachine.CameraTransform.right;

            // Flatten vectors
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            // Get movement direction
            Vector3 moveDir = (camForward * input.y + camRight * input.x).normalized;

            // Save starting momentum
            airMomentum = moveDir * stateMachine.CurrentSpeed;

            #endregion
        }

        public override void Tick(float deltaTime)
        {
            #region Movement Due To Momentum

            Vector3 velocity = airMomentum;
            velocity.y = stateMachine.VerticalVelocity;

            stateMachine.Controller.Move(velocity * deltaTime);

            #endregion

            #region State Transitions

            // ----------- Jump to Idle ------------- 

            if (stateMachine.IsGrounded && stateMachine.VerticalVelocity < 0f)
            {
                stateMachine.SwitchState(new PlayerMoveState(stateMachine)); /////////////
                return;
            }

            #endregion

        }

        public override void Exit() 
        {
            stateMachine.Gravity = _defaultGravity;
        }
    }
}
