using Game.Core;

namespace Game.MyPlayer
{
    public abstract class PlayerBaseState : State
    {
        protected readonly PlayerStateMachine stateMachine;

        public PlayerBaseState(PlayerStateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }
    }
}
