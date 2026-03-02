using Game.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.MyPlayer
{
    public class PlayerLadderClimbState : PlayerBaseState
    {
        public PlayerLadderClimbState(PlayerStateMachine stateMachine) : base(stateMachine) { }

        private float _defaultGravity;
        private Vector3 _ladderUpDirection;

        public override void Enter()
        {
            #region Enter Animation
            stateMachine.Animator.SetBool("LadderClimbing", true);
            #endregion

            #region Store Gravity
            _defaultGravity = stateMachine.Gravity;
            stateMachine.Gravity = 0f;
            #endregion

            #region Snap To Ladder & Align
            if (stateMachine.LadderTrigger != null && stateMachine.LadderTrigger.ClosestLadderCenter != null)
            {
                // Temporarily disable controller to set position
                stateMachine.Controller.enabled = false;
                stateMachine.transform.position = stateMachine.LadderTrigger.ClosestLadderCenter.position;
                stateMachine.Controller.enabled = true;

                // Ladder up direction
                _ladderUpDirection = stateMachine.LadderTrigger.ClosestLadderCenter.up;

                // Face ladder front
                Vector3 ladderForward = stateMachine.LadderTrigger.ClosestLadderCenter.forward;
                ladderForward.y = 0f;
                ladderForward.Normalize();
                stateMachine.Body.rotation = Quaternion.LookRotation(ladderForward);
            }
            #endregion

            #region Reset Vertical Velocity
            stateMachine.VerticalVelocity = 0f;
            #endregion
        }

        public override void Tick(float deltaTime)
        {
            #region State Transitions
            bool pressedDown = stateMachine.InputReader.MovementInput.y < -0.1f && stateMachine.IsGrounded;

            if (!stateMachine.IsNearLadder || !stateMachine.InputReader.InteractTriggered || stateMachine.IsGrounded)
            {
                stateMachine.SwitchState(new PlayerMoveState(stateMachine)); /////////////////
                return;
            }
            #endregion

            #region Movement
            float verticalInput = stateMachine.InputReader.MovementInput.y;
            Vector3 climbMove = _ladderUpDirection * verticalInput * stateMachine.LadderClimbingSpeed * deltaTime;
            stateMachine.Controller.Move(climbMove);

            if (Mathf.Approximately(verticalInput, 0f))
                stateMachine.VerticalVelocity = 0f;

            stateMachine.CurrentSpeed = Mathf.Lerp(stateMachine.CurrentSpeed, Mathf.Abs(verticalInput) * stateMachine.LadderClimbingSpeed, stateMachine.SpeedChangeRate * deltaTime);
            #endregion

            #region Update Animation
            stateMachine.Animator.SetFloat("Speed", stateMachine.CurrentSpeed);
            #endregion
        }

        public override void Exit()
        {
            #region Reset Gravity
            stateMachine.Gravity = _defaultGravity;
            #endregion

            #region Exit Animation
            stateMachine.Animator.SetBool("LadderClimbing", false);
            #endregion
        }
    }
}