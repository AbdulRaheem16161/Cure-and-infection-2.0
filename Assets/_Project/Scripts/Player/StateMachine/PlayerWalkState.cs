using UnityEngine;

public class PlayerMoveState : PlayerBaseState
{
    public PlayerMoveState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    // Animation 
    private string IS_MOVING = "isMoving";

    // stats
    private float moveSpeed;

    public override void Enter()
    {
        base.Enter();

        playerController.Animator.SetBool(IS_MOVING, true);
    }
    
    public override void Exit()
    {
        base.Exit();

        playerController.Animator.SetBool(IS_MOVING, false);
    }

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);

        moveSpeed = stateMachine.WalkPressed ? stateMachine.WalkSpeed : stateMachine.RunSpeed;
        stateMachine.PlayerController.Move(stateMachine.GetMovementInputVector3(), moveSpeed);
    }

    protected override void ChecksForSwitchingState()  
    {
        // Move to Idle
        if (stateMachine.MovementVector3 == Vector3.zero)
        {
            stateMachine.SwitchState(stateMachine.IdleState);
        }

        // Move to Jump
        if (stateMachine.JumpTriggered)
        {
            if (!stateMachine.CanJump)
            {
                Debug.Log("canJump is Disabled in the StateMachine");
                return;
            }

            stateMachine.SwitchState(stateMachine.IdleState);
        }
    }
}
