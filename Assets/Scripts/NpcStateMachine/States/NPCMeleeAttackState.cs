using UnityEngine;

namespace Game.MyNPC
{
    public class NPCMeleeAttackState : NPCBaseState
    {
		public NPCMeleeAttackState(NPCStateMachine stateMachine, int priority) : base(stateMachine, priority) { }

		private WeaponMelee EquippedWeapon => stateMachine.EquipmentHandler.itemInHands as WeaponMelee;
		private float randomSwingDelay;

		private readonly System.Random systemRandom = new();

		/// <summary>
		/// some way to have attack with weapon animation speed set to weapon attack speed + random swing delay
		/// then not being able to exiting this state till animation is complete
		/// </summary>

		public override bool IsValid()
		{
			return stateMachine.capabilityOverrides.HasFlag(EntityDefinition.Capability.meleeAttack) && 
				Beliefs.TargetFleeingFrom == null && Beliefs.Target != null && Beliefs.TargetInMeleeRange && Beliefs.MeleeWeaponInHands;
		}

		public override void Enter()
        {
			stateMachine.Agent.isStopped = true;
			stateMachine.Beliefs.SetNewInvestigateLocation(null);
			randomSwingDelay = GetRandomSwingDelay();
        }

        public override void Tick(float deltaTime)
        {
			if (stateMachine.StatsHandler.LifeState == EntityDefinition.LifeState.dead) return;

			//attack logic
			HandleMeleeAttack(deltaTime);
		}

        public override void Exit()
        {
			stateMachine.Agent.isStopped = false;
		}

		private void HandleMeleeAttack(float deltaTime)
		{
			randomSwingDelay -= deltaTime;
			if (randomSwingDelay > 0f)
				return;

			stateMachine.Animator.SetTrigger("Attack");
			EquippedWeapon.LightAttack();
		}

		private float GetRandomSwingDelay()
		{
			float minFireDelay = 0.1f;
			float maxFireDelay = 0.25f;

			return (float)(systemRandom.NextDouble() * (maxFireDelay - minFireDelay) + minFireDelay);
		}
	}
}

