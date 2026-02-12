using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Game.Core
{
    public abstract class StateMachine : MonoBehaviour
    {
        protected State currentState;

        public abstract List<string> TargetTags { get; set; }

        public void SwitchState(State newState)
        {
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
