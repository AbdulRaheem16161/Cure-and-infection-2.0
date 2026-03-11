using UnityEngine;

public abstract class NPCBaseState : State
{
    protected NPCStateMachine stateMachine;

    public NPCBaseState(NPCStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }
}
