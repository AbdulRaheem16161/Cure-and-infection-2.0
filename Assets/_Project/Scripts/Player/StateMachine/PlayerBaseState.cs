using Unity.VisualScripting;
using UnityEngine;

public abstract class PlayerBaseState : State
{
    protected PlayerStateMachine stateMachine;

    protected PlayerController playerController;


    public PlayerBaseState(PlayerStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;

        playerController = stateMachine.PlayerController; 
    }

    public override void Enter()
    {
        stateMachine.setCurrentStateString(this.GetType().Name);
    }

    public override void Exit()
    {
    }

    public override void Tick(float deltaTime)
    {
        ChecksForSwitchingState();
    }

    protected abstract void ChecksForSwitchingState();


}
