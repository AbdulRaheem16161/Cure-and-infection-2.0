using Game.Core;
using UnityEngine;

namespace Game.MyPlayer
{
    public class PlayerMoveState : PlayerBaseState
    {
        public PlayerMoveState(PlayerStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
   
        }

        public override void Exit()
        {
             
        }

        public override void Tick(float deltaTime)
        {
            if (stateMachine.isDead) return;

            #region State Transitions

            if (stateMachine.InputReader.IsSprinting)
            {
                stateMachine.SwitchState(new PlayerSprintState(stateMachine));
                return;
            }

            if (stateMachine.InputReader.JumpTriggered)
            {
                stateMachine.SwitchState(new PlayerJumpState(stateMachine));
                return;
            }

            if (stateMachine.InputReader.CrouchPressed)
            {
                stateMachine.SwitchState(new PlayerCrouchMovementState(stateMachine)); ///////////
                return;
            }

            if (stateMachine.InputReader.AttackTriggered)
            {
               // stateMachine.SwitchState(new PlayerAttackState(stateMachine));
               // return;
            }

            if (stateMachine.IsNearLadder && stateMachine.InputReader.InteractTriggered)
            {
                stateMachine.SwitchState(new PlayerLadderClimbState(stateMachine));
                return;
            }
            #endregion

            #region Camera-Oriented Movement
            Vector2 input = stateMachine.InputReader.MovementInput;

            Vector3 camForward = stateMachine.CameraTransform.forward;
            Vector3 camRight = stateMachine.CameraTransform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDir = camForward * input.y + camRight * input.x;
            moveDir.Normalize();

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

            stateMachine.CurrentSpeed = Mathf.Lerp(
                stateMachine.CurrentSpeed,
                stateMachine.InputReader.MovementInput.sqrMagnitude < 0.01f ? stateMachine.MinMoveSpeed : stateMachine.MaxMoveSpeed,
                stateMachine.SpeedChangeRate * deltaTime
            );
            #endregion

            #region Movement
            stateMachine.Controller.Move(moveDir * stateMachine.CurrentSpeed * deltaTime);
            #endregion

            #region Update Animation
            stateMachine.Animator.SetFloat("Speed", stateMachine.CurrentSpeed);
            #endregion
        }
    }
}
