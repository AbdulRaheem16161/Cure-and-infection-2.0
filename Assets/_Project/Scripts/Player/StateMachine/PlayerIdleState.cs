using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    // private Runtime Veriables

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();

    }

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);

    }

    protected override void ChecksForSwitchingState()
    {

        Debug.Log("Checking checks for states Switching" + stateMachine.GetMovementInputVector3());

        // Idle to Walk
        if (stateMachine.GetMovementInputVector3() != Vector3.zero)
        {
            if (!stateMachine.CanMove)
            {
                Debug.Log("canMove is Disabled in the StateMachine");
                return;
            }

            stateMachine.SwitchState(stateMachine.moveState);
        }

        // Idle to Jump
        if (stateMachine.JumpTriggered)
        {
            if (!stateMachine.CanJump)
            {
                Debug.Log("canJump is Disabled in the StateMachine");
                return;
            }
            stateMachine.SwitchState(stateMachine.JumpState);
        }
    }

}
