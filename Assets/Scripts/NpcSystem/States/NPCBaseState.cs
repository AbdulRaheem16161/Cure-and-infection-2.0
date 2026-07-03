using Game.Core;

namespace Game.MyNPC
{
    public abstract class NPCBaseState
    {
		protected NPCStateMachine stateMachine;
		protected NpcBeliefs Beliefs => stateMachine.Beliefs;

		public int Priority { get; private set; }

		public NPCBaseState(NPCStateMachine stateMachine, int priority)
		{
			this.stateMachine = stateMachine;
			Priority = priority;
		}

		public abstract bool IsValid();

		public abstract void Enter();
		public abstract void Tick(float deltaTime);
		public abstract void Exit();
	}
}

