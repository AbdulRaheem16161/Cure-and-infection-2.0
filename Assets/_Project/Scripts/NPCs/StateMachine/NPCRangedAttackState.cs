using Game.MyPlayer;
using UnityEngine;

namespace Game.MyNPC
{
    public class NPCRangedAttackState : NPCBaseState
    {
        #region Constructor
        public NPCRangedAttackState(NPCStateMachine stateMachine) : base(stateMachine) { }
        #endregion

        #region Fields
        private WeaponRanged EquippedWeapon => stateMachine.EquipmentHandler.rangedWeaponInHands;
		private float shotsToBurstFireCount;
		private float randomShotDelay;
		#endregion

		private readonly System.Random systemRandom = new();

        public override void Enter()
        {
            stateMachine.Agent.speed = 0f;
            BurstFireBehaviour();
        }

        public override void Tick(float deltaTime)
        {
            if (stateMachine.StatsHandler.IsDead) return;

			//need a better check, preferably wont enter ranged attack if it doesnt have a ranged weapon.
			if (!stateMachine.EquipmentHandler.HasRangedWeaponInHands) return;

            Debug.Log("ranged attack state ticking");

			#region burst fire shot delay
			randomShotDelay -= deltaTime;
			if (randomShotDelay > 0f)
				return;
			#endregion

			#region Shoot or reload behaviour
			if (EquippedWeapon.MagazineEmpty)
				EquippedWeapon.Reload(stateMachine.InventoryHandler, true);
            else
            {
				EquippedWeapon.Shoot(stateMachine.TargetTags.ToArray());
                shotsToBurstFireCount--;
                BurstFireBehaviour();
			}
            #endregion

            #region State Transitions 

            // ----------- Attack to Chase -------------
            if (!stateMachine.OpponentInRangedAttackRange || !stateMachine.HasEquippedRangedWeapon || !stateMachine.EnableRangedAttack)
            {
                stateMachine.SwitchState(new NPCChaseState(stateMachine));
                return;
            }

            // ----------- Ranged Attack to Melee Attack -------------
            if (stateMachine.OpponentInMeleeAttackRange && stateMachine.HasEquippedMeleeWeapon && stateMachine.EnableMeleeAttack)
            {
                stateMachine.SwitchState(new NPCMeleeAttackState(stateMachine));
                return;
            }
            #endregion

            #region Rotate Towards Follow Point

            if (stateMachine.NpcPerception.DetectedTarget != null)
            {
                stateMachine.CurrentFollowPoint = stateMachine.NpcPerception.DetectedTarget.transform;
            }
            stateMachine.Agent.SetDestination(stateMachine.CurrentFollowPoint.position);

            Vector3 direction = (stateMachine.CurrentFollowPoint.position - stateMachine.transform.position).normalized;
            if (direction != Vector3.zero) // Prevent errors when NPC is exactly at the target
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                stateMachine.transform.rotation = Quaternion.RotateTowards(
                    stateMachine.transform.rotation,
                    targetRotation,
                    stateMachine.RangedAttackRotSpeed * deltaTime * 100f
                );
            }
            #endregion
        }

        public override void Exit()
        {
            
        }

		#region human burst fire behaviour
		///<summery>
		/// simulate human shooting with short bursts (full auto gets longer bursts)
		/// extra delay for every shot to simulate npc recoil/aiming recovery with non full auto fire modes
		/// add longer pause after every burst fire (bolts ignore this)
		///<summery>
		private void BurstFireBehaviour()
        {
			if (EquippedWeapon.WeaponDefinition.FireMode != WeaponRangedDefinition.FireModeType.fullAuto)
				randomShotDelay = GetRandomShotDelay();

			if (shotsToBurstFireCount <= 0)
			{
				if (EquippedWeapon.WeaponDefinition.FireMode == WeaponRangedDefinition.FireModeType.fullAuto)
					randomShotDelay = GetRandomShotDelay(); //gets skipped above, set full auto here

				if (EquippedWeapon.WeaponDefinition.Weapon != WeaponRangedDefinition.WeaponType.boltActionRifle)
					randomShotDelay *= 3;

				shotsToBurstFireCount = GetBurstFireCount();
			}
		}
        private float GetRandomShotDelay()
		{
			float minFireDelay = 0;
			float maxFireDelay = 0;

			switch (EquippedWeapon.WeaponDefinition.Weapon)
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

			switch (EquippedWeapon.WeaponDefinition.Weapon)
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
			if (EquippedWeapon.WeaponDefinition.FireMode == WeaponRangedDefinition.FireModeType.fullAuto)
			{
				minShots *= 2;
				maxShots *= 2;
			}

			return systemRandom.Next(minShots, maxShots);
		}
		#endregion
	}
}

