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
		private readonly float DebugChanceToAds = 0.5f;


		public override bool IsValid()
		{
			bool canShoot = stateMachine.capabilityOverrides.HasFlag(EntityDefinition.Capability.rangedAttack) && Beliefs.FleeTarget == null &&
				Beliefs.Target != null && Beliefs.TargetInShootingRange && Beliefs.RangedWeaponInHands;

			return canShoot && (!Beliefs.MovingToCover || Beliefs.ReturnFire);
		}

		public override void Enter()
        {
			bool shouldBeAds = ShouldAds();

			if (shouldBeAds != (EquippedWeapon.Aim == RangedWeaponItem.AimState.ads))
			{
				if (shouldBeAds)
					EquippedWeapon.EnterAimDownSights();
				else
					EquippedWeapon.ExitAimDownSights();
			}

			if (shouldBeAds && EquippedWeapon.Aim == RangedWeaponItem.AimState.hipfire)
				EquippedWeapon.EnterAimDownSights();
			else if (!shouldBeAds && EquippedWeapon.Aim == RangedWeaponItem.AimState.ads)
				EquippedWeapon.ExitAimDownSights();

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

			if (Beliefs.Target == null) return;

			LookAtDirection(Beliefs.Target.Transform.position);
			HandleShootingBehaviour();
		}

        public override void Exit()
        {
			stateMachine.Agent.updateRotation = true;
			stateMachine.Agent.isStopped = false;
		}

		#region Handle Should Ads
		/// <summary>
		/// should npc ads check, will need to eventually be called every 1s (or similar) + more complex considerations for npc
		/// like not adsing if in cover and target close. or different distance scailing based on weapon type like nearly always
		/// aiming with a pistol and sniper rifle, shotgun/smgs having lower thresholds etc...
		/// </summary>
		/// <returns></returns>
		private bool ShouldAds()
		{
			if (Beliefs.MovingToCover && Beliefs.ReturnFire) return false; //never ads when moving to cover and returning fire

			if (Beliefs.InCover) return true; //always ads from cover (change later)

			//always ads when target above 1/3 of weapons effective range (can be switched to percentage + base it on weapon type more)
			if (Beliefs.Target.SquaredDistance > (EquippedWeapon.TypedDefinition.EffectiveSqrRange * 0.33)) return true;

			return systemRandom.NextDouble() < DebugChanceToAds; //random chance for now (replace with better considerations later)
		}
		#endregion

		#region Handle Shooting behaviour
		private void HandleShootingBehaviour()
		{
			if (!lookingAtTarget)
			{
				EquippedWeapon.StopShooting();
				return;
			}

			if (EquippedWeapon.MagazineEmpty)
			{
				EquippedWeapon.StopShooting();
				EquippedWeapon.Reload(stateMachine.InventoryHandler, true);
				return;
			}

			if (EquippedWeapon.canShoot && shotDelay <= 0 && burstFireDelay <= 0)
			{
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
			EquippedWeapon.StopShooting();

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
		private void BurstFireDelayTimer(float deltaTime)
		{
			if (burstFireDelay <= 0f) return;
			burstFireDelay -= deltaTime;
		}
		private void ShotDelayTimer(float deltaTime)
		{
			if (shotDelay <= 0f) return;
			shotDelay -= deltaTime;
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
				maxShots = 3;
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
				maxShots = 3;
				break;
				case WeaponRangedDefinition.WeaponType.boltActionRifle:
				minShots = 1;
				maxShots = 1;
				break;
			}

			//bigger bursts for full auto guns
			if (EquippedWeapon.TypedDefinition.FireMode == WeaponRangedDefinition.FireModeType.fullAuto)
			{
				minShots += 1;
				maxShots += 3;
			}

			return systemRandom.Next(minShots, maxShots);
		}
		#endregion
	}
}

