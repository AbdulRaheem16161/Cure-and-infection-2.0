using Game.Core;

namespace Game.MyNPC
{
    public abstract class NPCBaseState : State
    {
        protected NPCStateMachine stateMachine;

        public NPCBaseState(NPCStateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }
    }
}

