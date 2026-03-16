using System.Collections.Generic;
using Game.Core;
using UnityEngine;

namespace Game.GenericStateMachine
{
    public class PlayerStateMachine_Generic : StateMachine
    {
        [field: SerializeField] public InputReader InputReader { get; private set; }

        private void Start()
        {
            SwitchState(new PlayerTestState(this));
        }
    }
}
