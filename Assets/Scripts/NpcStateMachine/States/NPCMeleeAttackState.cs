using UnityEngine;

namespace Game.MyNPC
{
    public class NPCMeleeAttackState : NPCBaseState
    {
        #region Constructor
        public NPCMeleeAttackState(NPCStateMachine stateMachine) : base(stateMachine) { }
		#endregion

		private float randomSwingDelay;

		private readonly System.Random systemRandom = new();

		#region Fields
		private float _attackDurationTimer;
        #endregion

        public override void Enter()
        {
            #region Enter Animation
            stateMachine.Animator.SetTrigger("Attack");
            #endregion
        }

        public override void Tick(float deltaTime)
        {
            if (stateMachine.StatsHandler.IsDead) return;

			randomSwingDelay -= deltaTime;
			if (randomSwingDelay > 0f)
				return;

			#region Update Attack Timer
			_attackDurationTimer += deltaTime;
            #endregion

            #region State Transitions 
            // Otherwise, return to Chase state
            if (!stateMachine.TargetInMeleeRange || !stateMachine.HasEquippedMeleeWeapon)
            {
                stateMachine.SwitchState(new NPCChaseState(stateMachine));
                return;
            }
            #endregion
        }

        public override void Exit()
        {

        }

		private float GetRandomSwingDelay()
		{
			float minFireDelay = 0.1f;
			float maxFireDelay = 0.25f;

			return (float)(systemRandom.NextDouble() * (maxFireDelay - minFireDelay) + minFireDelay);
		}
	}
}

