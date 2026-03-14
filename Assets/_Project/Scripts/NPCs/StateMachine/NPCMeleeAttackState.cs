using Game.MyPlayer;
using UnityEngine;

namespace Game.MyNPC
{
    public class NPCMeleeAttackState : NPCBaseState
    {
        #region Constructor
        public NPCMeleeAttackState(NPCStateMachine stateMachine) : base(stateMachine) { }
		#endregion

		private WeaponMelee EquippedWeapon => stateMachine.EquipmentHandler.meleeWeaponInHands;

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

			if (!stateMachine.EquipmentHandler.HasMeleeWeaponInHands) return;

			randomSwingDelay -= deltaTime;
			if (randomSwingDelay > 0f)
				return;



			#region Update Attack Timer
			_attackDurationTimer += deltaTime;
            #endregion

            #region Enable Hitbox
            if (_attackDurationTimer >= stateMachine.HitboxActivationDelay)
            {
                if (stateMachine.Hitbox != null)
                    stateMachine.Hitbox.SetActive(true);
            }

            #endregion

            #region State Transitions 

            // Otherwise, return to Chase state
            if (_attackDurationTimer >= stateMachine.AttackDuration || !stateMachine.HasEquippedMeleeWeapon)
            {
                stateMachine.SwitchState(new NPCChaseState(stateMachine));
                return;
            }
            #endregion
        }

        public override void Exit()
        {
            #region Disable Hitbox
            if (stateMachine.Hitbox != null)
                stateMachine.Hitbox.gameObject.SetActive(false);
            #endregion
        }

		private float GetRandomSwingDelay()
		{
			float minFireDelay = 0.1f;
			float maxFireDelay = 0.25f;

			return (float)(systemRandom.NextDouble() * (maxFireDelay - minFireDelay) + minFireDelay);
		}
	}
}

