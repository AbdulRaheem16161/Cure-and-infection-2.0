using Game.Core;

namespace Game.GenericStateMachine
{
    public class PlayerTestState : PlayerBaseState
    {
        public PlayerTestState(PlayerStateMachine_Generic stateMachine) : base(stateMachine) { }

        public override void Enter() { }
        public override void Tick(float deltaTime) { }
        public override void Exit() { }
    }
}
