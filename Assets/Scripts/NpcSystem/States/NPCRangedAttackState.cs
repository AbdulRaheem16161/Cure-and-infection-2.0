using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEngine;
using static WeaponRangedDefinition;

namespace Game.MyNPC
{
    public class NPCRangedAttackState : NpcBaseMovementState
    {
		public NPCRangedAttackState(NPCStateMachine stateMachine, int priority) : base(stateMachine, priority) { }

		private RangedWeaponItem EquippedWeapon => stateMachine.EquipmentHandler.itemInHands as RangedWeaponItem;

		//ads fields
		private float adsScore;
		private readonly float shouldAdsCooldown = 1f;
		private float shouldAdsTimer;

		//burst fire/shot delay fields
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
			shouldAdsTimer = 0f;
			burstFireDelay = 0f;
			shotDelay = 0f;
			shotsToBurstFireCount = GetBurstFireCount();
		}

        public override void Tick(float deltaTime)
        {
            if (stateMachine.StatsHandler.LifeState == EntityDefinition.LifeState.dead) return;

            ShouldAdsCheckTimer(deltaTime);
			ShotDelayTimer(deltaTime);
			BurstFireDelayTimer(deltaTime);

			if (Beliefs.Target == null) return;

			LookAtDirection(Beliefs.Target.Position);
			HandleShootingBehaviour();
		}

        public override void Exit()
        {
			stateMachine.Agent.updateRotation = true;
			stateMachine.Agent.isStopped = false;
		}

		#region Ads check timer
		private void ShouldAdsCheckTimer(float deltaTime)
		{
			if (shouldAdsTimer <= 0)
			{
				bool shouldBeAds = ShouldAds();

				if (shouldBeAds != (EquippedWeapon.Aim == RangedWeaponItem.AimState.ads))
				{
					if (shouldBeAds)
						EquippedWeapon.EnterAimDownSights();
					else
						EquippedWeapon.ExitAimDownSights();
				}

				shouldAdsTimer = shouldAdsCooldown;
			}

			if (shouldAdsTimer > 0)
				shouldAdsTimer -= deltaTime;
		}
		#endregion

		#region Ads checks
		private bool ShouldAds()
		{
			if (Beliefs.MovingToCover && Beliefs.ReturnFire) return false;

			adsScore = Beliefs.InCover ? 0.15f : 0f; //more likely to ads in cover
			adsScore += GetAdsScoreBasedOnDistance();

			bool currentlyAds = EquippedWeapon.Aim == RangedWeaponItem.AimState.ads;
			float threshold = currentlyAds ? 0.6f : 0.7f; //ads above 0.7f, stay ads'd till lower then 0.6f

			return adsScore >= threshold;
		}
		/// <summary>
		/// values need proper tweaking but they were roughly tweaked (and more tweaking if other considerations are added)
		/// consider adding an ads cost timer, so smgs ads a little more aggressively (independent of effective range) due to low ads time
		/// compared to longer ads times of assault rifles. + rifles will stay ads's more aggressively due to long ads times and having to un ads.
		/// </summary>
		private float GetAdsScoreBasedOnDistance()
		{
			float normalizedDistance = Beliefs.Target.SquaredDistance / EquippedWeapon.TypedDefinition.EffectiveSqrRange;

			switch (EquippedWeapon.TypedDefinition.Weapon)
			{
				case WeaponType.handgun:
				return normalizedDistance * 1.8f; //mostly ads due to low aim times and general weapon closeness

				case WeaponType.shotgun:
				return normalizedDistance * 0.8f; //hardly ever aim

				case WeaponType.smg:
				return normalizedDistance * 1.3f; //aim based roughly on when weapon type starts missing shots

				case WeaponType.assaultRifle:
				return normalizedDistance * 1.5f; //aim based roughly on when weapon type starts missing shots

				case WeaponType.marksmanRifle:
				return normalizedDistance * 5f; //always aim for the majority of there range

				case WeaponType.boltActionRifle:
				return normalizedDistance * 5f; //always aim for the majority of there range

				default:
				return normalizedDistance;
			}
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
				EquippedWeapon.Reload(stateMachine.InventoryHandler.ItemContainer, true);
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
			if (EquippedWeapon.CurrentFireMode == FireModeType.fullAuto)
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
				case WeaponType.handgun:
				minFireDelay = 0.25f;
				maxFireDelay = 0.35f;
				break;
				case WeaponType.shotgun:
				minFireDelay = 0.6f;
				maxFireDelay = 0.8f;
				break;
				case WeaponType.smg:
				minFireDelay = 0.18f;
				maxFireDelay = 0.25f;
				break;
				case WeaponType.assaultRifle:
				minFireDelay = 0.2f;
				maxFireDelay = 0.3f;
				break;
				case WeaponType.marksmanRifle:
				minFireDelay = 0.45f;
				maxFireDelay = 0.55f;
				break;
				case WeaponType.boltActionRifle:
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
				case WeaponType.handgun:
				minShots = 2;
				maxShots = 3;
				break;
				case WeaponType.shotgun:
				minShots = 1;
				maxShots = 3;
				break;
				case WeaponType.smg:
				minShots = 3;
				maxShots = 5;
				break;
				case WeaponType.assaultRifle:
				minShots = 2;
				maxShots = 4;
				break;
				case WeaponType.marksmanRifle:
				minShots = 2;
				maxShots = 3;
				break;
				case WeaponType.boltActionRifle:
				minShots = 1;
				maxShots = 1;
				break;
			}

			//bigger bursts for full auto guns
			if (EquippedWeapon.CurrentFireMode == FireModeType.fullAuto)
			{
				minShots += 1;
				maxShots += 3;
			}

			return systemRandom.Next(minShots, maxShots);
		}
		#endregion
	}
}

