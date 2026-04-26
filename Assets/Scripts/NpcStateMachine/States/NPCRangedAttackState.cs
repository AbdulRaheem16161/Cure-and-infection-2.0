using UnityEngine;

namespace Game.MyNPC
{
    public class NPCRangedAttackState : NpcBaseMovementState
    {
		public NPCRangedAttackState(NPCStateMachine stateMachine, int priority) : base(stateMachine, priority) { }

		private RangedWeaponItem EquippedWeapon => stateMachine.EquipmentHandler.itemInHands as RangedWeaponItem;
		private float shotsToBurstFireCount;
		private float randomShotDelay;
		private float coverSearchDelay;

		private readonly System.Random systemRandom = new();

		public override bool IsValid()
		{
			return stateMachine.capabilityOverrides.HasFlag(EntityDefinition.Capability.rangedAttack) && 
				Beliefs.TargetFleeingFrom == null && Beliefs.Target != null && Beliefs.TargetInShootingRange && Beliefs.RangedWeaponInHands;
		}

		public override void Enter()
        {
			lookingAtTarget = false;
			Beliefs.SetNewInvestigateLocation(null);
			Beliefs.MovingToCover = false;
			Beliefs.InCover = false;
			coverSearchDelay = 0f;
			BurstFireBehaviour();
		}

        public override void Tick(float deltaTime)
        {
            if (stateMachine.StatsHandler.LifeState == EntityDefinition.LifeState.dead) return;

			HandleFindingAndMovingToCover(deltaTime);

			if (Beliefs.MovingToCover && HasReachedDestination())
			{
				Beliefs.InCover = true; 
				Beliefs.MovingToCover = false; 
				return; 
			}

			if (Beliefs.MovingToCover) return;

			LookAtDirection(Beliefs.Target.Transform.position);
			HandleShootingBehaviour(deltaTime);
		}

        public override void Exit()
        {
			stateMachine.Agent.updateRotation = true;
			stateMachine.Agent.isStopped = false;
		}

		#region Handle Finding And Moving To Cover
		private void HandleFindingAndMovingToCover(float deltaTime)
		{
			if (!ShouldUseCover() || Beliefs.InCover || Beliefs.MovingToCover) return;

			coverSearchDelay -= deltaTime;
			if (coverSearchDelay > 0f)
				return;

			coverSearchDelay = 2f;

			if (stateMachine.NpcPerception.FindValidCover(Beliefs.Target, out Vector3? coverMovePosition))
			{
				MoveToDestination(coverMovePosition.Value, MoveType.sprint);
				lookingAtTarget = false;
				Beliefs.MovingToCover = true;
			}
		}
		#endregion

		#region Should Use Cover Logic Check
		//limits use of cover when target is a non zombified humanoid (ignores animals/zombies basically)
		private bool ShouldUseCover()
		{
			EntityDefinition targetDefinition = Beliefs.Target.StatsHandler.Definition;
			if (targetDefinition is HumanoidDefinition humanoid)
			{
				if (humanoid.Flags.HasFlag(EntityDefinition.EntityFlags.canBecomeZombie))
					return true;
			}

			return false;
		}
		#endregion

		#region Handle Shooting behaviour
		private void HandleShootingBehaviour(float deltaTime)
		{
			randomShotDelay -= deltaTime;
			if (randomShotDelay > 0f)
				return;

			if (!lookingAtTarget) return;

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

