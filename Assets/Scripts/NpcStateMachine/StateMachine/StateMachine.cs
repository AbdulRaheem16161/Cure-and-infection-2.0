using UnityEngine;

namespace Game.Core
{
    public abstract class StateMachine : MonoBehaviour
    {
        protected State currentState;
		public State CurrentState => currentState;

        public void SwitchState(State newState, bool logStateSwitch = false)
        {
			if (logStateSwitch)
				Debug.Log($"switched from state: {currentState} to {newState}");

			currentState?.Exit();
            currentState = newState;
            currentState?.Enter();
        }

        protected virtual void Update()
        {
            currentState?.Tick(Time.deltaTime);
        }
    }
}
