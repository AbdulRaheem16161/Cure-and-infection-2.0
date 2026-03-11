using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    public PlayerJumpState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    private float walkSpeed;
    private const string JUMP_TRIGGER = "triggerJump";

    public override void Enter()
    {
        base.Enter();
        walkSpeed = stateMachine.RunSpeed;
        playerController.Animator.SetTrigger(JUMP_TRIGGER);

        playerController.Jump(stateMachine.JumpHeight);
        stateMachine.JumpTriggered = false;   
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);

        stateMachine.PlayerController.Move(stateMachine.GetMovementInputVector3(), walkSpeed);
    }

    protected override void ChecksForSwitchingState()
    {
        // Jump to Idle
        if ((stateMachine.PlayerController.IsGrounded()) && (playerController.VerticalVelocity.y < 0f))
        {
            stateMachine.SwitchState(stateMachine.IdleState);
        }
    }

}

