using UnityEngine;

namespace Game.MyNPC
{
    public class NPCRangedAttackState : NpcBaseMovementState
    {
		public NPCRangedAttackState(NPCStateMachine stateMachine, int priority) : base(stateMachine, priority) { }

		private RangedWeaponItem EquippedWeapon => stateMachine.EquipmentHandler.itemInHands as RangedWeaponItem;

		private float shotsToBurstFireCount;
		private float burstFireDelay;
		private float shotDelay;

		private readonly System.Random systemRandom = new();

		public override bool IsValid()
		{
			bool canShoot = stateMachine.capabilityOverrides.HasFlag(EntityDefinition.Capability.rangedAttack) && Beliefs.FleeTarget == null &&
				Beliefs.Target != null && Beliefs.TargetInShootingRange && Beliefs.RangedWeaponInHands;

			return canShoot && (!Beliefs.MovingToCover || Beliefs.ReturnFire);
		}

		public override void Enter()
        {
			Beliefs.SetNewInvestigateLocation(null);
			lookingAtTarget = false;
			shotDelay = 0f;
			burstFireDelay = 0f;
			shotsToBurstFireCount = GetBurstFireCount();
		}

        public override void Tick(float deltaTime)
        {
            if (stateMachine.StatsHandler.LifeState == EntityDefinition.LifeState.dead) return;

			ShotDelayTimer(deltaTime);
			BurstFireDelayTimer(deltaTime);

			LookAtDirection(Beliefs.Target.Transform.position);
			HandleShootingBehaviour();
		}

        public override void Exit()
        {
			stateMachine.Agent.updateRotation = true;
			stateMachine.Agent.isStopped = false;
		}

		#region Handle Shooting behaviour
		private void HandleShootingBehaviour()
		{
			if (!lookingAtTarget) return;

			if (EquippedWeapon.MagazineEmpty)
				EquippedWeapon.Reload(stateMachine.InventoryHandler, true);
			else
			{
				if (shotDelay > 0f || burstFireDelay > 0f)
					return;

				EquippedWeapon.Shoot();
				HandlePerShotBehaviour();
				HandleBurstFireBehaviour();
			}
		}
		#endregion

		#region Burst Fire and Per Shot Behaviour (simulates more human behaviour)
		private void HandleBurstFireBehaviour()
		{
			shotsToBurstFireCount--;

			if (shotsToBurstFireCount > 0) return;

			burstFireDelay = GetShotDelay() * 3;// longer delay after burst fire
			shotsToBurstFireCount = GetBurstFireCount();

			if (Beliefs.ReturnFire)
				Beliefs.ReturnFire = false;
		}
		private void HandlePerShotBehaviour()
		{
			if (EquippedWeapon.TypedDefinition.FireMode == WeaponRangedDefinition.FireModeType.fullAuto)
				shotDelay = 0;
			else
				shotDelay = GetShotDelay();
		}
		#endregion

		#region Burst And Shot Delay Timers
		private bool BurstFireDelayTimer(float deltaTime)
		{
			burstFireDelay -= deltaTime;
			return burstFireDelay > 0f;
		}
		private bool ShotDelayTimer(float deltaTime)
		{
			shotDelay -= deltaTime;
			return shotDelay > 0f;
		}
		#endregion

		#region Get Shot Delay (reused for burst fire too)
		private float GetShotDelay()
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
		#endregion

		#region Get Burst Fire Count
		private int GetBurstFireCount()
        {
			int minShots = 0;
			int maxShots = 0;

			switch (EquippedWeapon.TypedDefinition.Weapon)
			{
				case WeaponRangedDefinition.WeaponType.handgun:
				minShots = 2;
				maxShots = 4;
				break;
				case WeaponRangedDefinition.WeaponType.shotgun:
				minShots = 1;
				maxShots = 3;
				break;
				case WeaponRangedDefinition.WeaponType.smg:
				minShots = 3;
				maxShots = 5;
				break;
				case WeaponRangedDefinition.WeaponType.assaultRifle:
				minShots = 2;
				maxShots = 4;
				break;
				case WeaponRangedDefinition.WeaponType.marksmanRifle:
				minShots = 2;
				maxShots = 4;
				break;
				case WeaponRangedDefinition.WeaponType.boltActionRifle:
				minShots = 1;
				maxShots = 1;
				break;
			}

			//bigger bursts for full auto guns
			if (EquippedWeapon.TypedDefinition.FireMode == WeaponRangedDefinition.FireModeType.fullAuto)
			{
				minShots += 2;
				maxShots += 3;
			}

			return systemRandom.Next(minShots, maxShots);
		}
		#endregion
	}
}

