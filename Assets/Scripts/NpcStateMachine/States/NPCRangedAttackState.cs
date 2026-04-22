using UnityEngine;

namespace Game.MyNPC
{
    public class NPCRangedAttackState : NpcBaseMovementState
    {
		public NPCRangedAttackState(NPCStateMachine stateMachine, int priority) : base(stateMachine, priority) { }

		private RangedWeaponItem EquippedWeapon => stateMachine.EquipmentHandler.itemInHands as RangedWeaponItem;
		private float shotsToBurstFireCount;
		private float randomShotDelay;

		private readonly System.Random systemRandom = new();

		public override bool IsValid()
		{
			return stateMachine.capabilityOverrides.HasFlag(EntityDefinition.Capability.rangedAttack) && 
				Beliefs.TargetFleeingFrom == null && Beliefs.Target != null && Beliefs.TargetInShootingRange && Beliefs.RangedWeaponInHands;
		}

		public override void Enter()
        {
			lookingAtTarget = false;
			stateMachine.Beliefs.SetNewInvestigateLocation(null);
			BurstFireBehaviour();
		}

        public override void Tick(float deltaTime)
        {
            if (stateMachine.StatsHandler.LifeState == EntityDefinition.LifeState.dead) return;

			//logic to handle looking at target and handling shooting
			HandleShootingBehaviour(deltaTime);
			LookAtDirection(stateMachine.Beliefs.Target.Transform.position);
		}

        public override void Exit()
        {
			stateMachine.Agent.updateRotation = true;
			stateMachine.Agent.isStopped = false;
		}

		#region Handle Shooting behaviour
		private void HandleShootingBehaviour(float deltaTime)
		{
			if (!lookingAtTarget) return;

			randomShotDelay -= deltaTime;
			if (randomShotDelay > 0f)
				return;

			if (EquippedWeapon.MagazineEmpty)
				EquippedWeapon.Reload(stateMachine.InventoryHandler, true);
			else
			{
				EquippedWeapon.Shoot();
				shotsToBurstFireCount--;
				BurstFireBehaviour();
			}
		}
		#endregion

		#region human burst fire behaviour
		///<summery>
		/// simulate human shooting with short bursts (full auto gets longer bursts)
		/// extra delay for every shot to simulate npc recoil/aiming recovery with non full auto fire modes
		/// add longer pause after every burst fire (bolts ignore this)
		///<summery>
		private void BurstFireBehaviour()
        {
			if (EquippedWeapon.TypedDefinition.FireMode != WeaponRangedDefinition.FireModeType.fullAuto)
				randomShotDelay = GetRandomShotDelay();

			if (shotsToBurstFireCount <= 0)
			{
				if (EquippedWeapon.TypedDefinition.FireMode == WeaponRangedDefinition.FireModeType.fullAuto)
					randomShotDelay = GetRandomShotDelay(); //gets skipped above, set full auto here

				if (EquippedWeapon.TypedDefinition.Weapon != WeaponRangedDefinition.WeaponType.boltActionRifle)
					randomShotDelay *= 3;

				shotsToBurstFireCount = GetBurstFireCount();
			}
		}
        private float GetRandomShotDelay()
		{
			float minFireDelay = 0;
			float maxFireDelay = 0;

			switch (EquippedWeapon.TypedDefinition.Weapon)
			{
				case WeaponRangedDefinition.WeaponType.handgun:
				minFireDelay = 0.25f;
				maxFireDelay = 0.35f;
				break;
				case WeaponRangedDefinition.WeaponType.shotgun:
				minFireDelay = 0.6f;
				maxFireDelay = 0.8f;
				break;
				case WeaponRangedDefinition.WeaponType.smg:
				minFireDelay = 0.18f;
				maxFireDelay = 0.25f;
				break;
				case WeaponRangedDefinition.WeaponType.assaultRifle:
				minFireDelay = 0.2f;
				maxFireDelay = 0.3f;
				break;
				case WeaponRangedDefinition.WeaponType.marksmanRifle:
				minFireDelay = 0.45f;
				maxFireDelay = 0.55f;
				break;
				case WeaponRangedDefinition.WeaponType.boltActionRifle:
				minFireDelay = 1.2f;
				maxFireDelay = 1.5f;
				break;
			}

			return (float)(systemRandom.NextDouble() * (maxFireDelay - minFireDelay) + minFireDelay);
		}
        private int GetBurstFireCount()
        {
			int minShots = 0;
			int maxShots = 0;

			switch (EquippedWeapon.TypedDefinition.Weapon)
			{
				case WeaponRangedDefinition.WeaponType.handgun:
				minShots = 2;
				maxShots = 5;
				break;
				case WeaponRangedDefinition.WeaponType.shotgun:
				minShots = 2;
				maxShots = 4;
				break;
				case WeaponRangedDefinition.WeaponType.smg:
				minShots = 2;
				maxShots = 5;
				break;
				case WeaponRangedDefinition.WeaponType.assaultRifle:
				minShots = 2;
				maxShots = 5;
				break;
				case WeaponRangedDefinition.WeaponType.marksmanRifle:
				minShots = 2;
				maxShots = 4;
				break;
				case WeaponRangedDefinition.WeaponType.boltActionRifle:
				minShots = 0;
				maxShots = 0;
				break;
			}

			//bigger bursts for full auto guns
			if (EquippedWeapon.TypedDefinition.FireMode == WeaponRangedDefinition.FireModeType.fullAuto)
			{
				minShots *= 2;
				maxShots *= 2;
			}

			return systemRandom.Next(minShots, maxShots);
		}
		#endregion
	}
}

