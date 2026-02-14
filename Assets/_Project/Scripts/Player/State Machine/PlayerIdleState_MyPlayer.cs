using Game.Core;
using UnityEngine;

namespace Game.MyPlayer
{
    public class PlayerIdleState : PlayerBaseState
    {
        public PlayerIdleState(PlayerStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            #region Enter Animation
            #endregion
        }

        public override void Exit()
        {
            #region Exit Animation
            #endregion
        }

        public override void Tick(float deltaTime)
        {
            if (stateMachine.isDead) return;

            #region State Transitions
            if (stateMachine.InputReader.MovementInput.sqrMagnitude > 0.01f)
            {
                stateMachine.SwitchState(new PlayerMoveState(stateMachine));
                return;
            }

            if (stateMachine.InputReader.JumpTriggered)
            {
                stateMachine.SwitchState(new PlayerJumpState(stateMachine));
                return;
            }

            if (stateMachine.InputReader.CrouchPressed)
            {
                 stateMachine.SwitchState(new PlayerCrouchIdleState(stateMachine));  
                return;
            }

            if (stateMachine.InputReader.AttackTriggered)
            {
              //  stateMachine.SwitchState(new PlayerAttackState(stateMachine));
              //  return;
            }

            if (stateMachine.IsNearLadder && stateMachine.InputReader.InteractTriggered)
            {
                stateMachine.SwitchState(new PlayerLadderClimbState(stateMachine));
                return;
            }
            #endregion

            #region Speed Deacceleration
            stateMachine.CurrentSpeed = Mathf.Lerp(stateMachine.CurrentSpeed, stateMachine.MinMoveSpeed, stateMachine.SpeedChangeRate * deltaTime);
            #endregion

            #region Update Animation
            stateMachine.Animator.SetFloat("Speed", stateMachine.CurrentSpeed);
            #endregion
        }
    }
}
