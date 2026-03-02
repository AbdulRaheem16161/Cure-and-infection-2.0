using Game.Core;

namespace Game.GenericStateMachine
{
    public abstract class PlayerBaseState : State
    {
        protected PlayerStateMachine_Generic stateMachine;

        public PlayerBaseState(PlayerStateMachine_Generic stateMachine)
        {
            this.stateMachine = stateMachine;
        }
    }
}
