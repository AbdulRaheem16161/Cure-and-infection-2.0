using System.Collections.Generic;
using Game.Core;
using UnityEngine;

namespace Game.GenericStateMachine
{
    public class PlayerStateMachine_Generic : StateMachine
    {
        [field: SerializeField] public InputReader InputReader { get; private set; }

        [SerializeField] private List<string> targetTags = new List<string>();
        public override List<string> TargetTags
        {
            get { return targetTags; }
            set { targetTags = value; }
        }

        private void Start()
        {
            SwitchState(new PlayerTestState(this));
        }
    }
}
