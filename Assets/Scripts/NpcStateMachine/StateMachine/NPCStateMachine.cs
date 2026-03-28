using Game.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using static NPCSpawner;

namespace Game.MyNPC
{
	[RequireComponent(typeof(StatsHandler))]
	[RequireComponent(typeof(EquipmentHandler))]
	[RequireComponent(typeof(InventoryHandler))]
	[RequireComponent(typeof(NpcPerception))]
    public class NPCStateMachine : StateMachine
    {
        public StatsHandler StatsHandler { get; private set; }
        public EquipmentHandler EquipmentHandler { get; private set; }
        public InventoryHandler InventoryHandler { get; private set; }
		public NpcPerception NpcPerception { get; private set; }

		#region General Values
		[Header("General Values")]
        public Animator Animator;
        public NavMeshAgent Agent;
        public float CurrentSpeed;
        public string CurrentStateName;
        public Vector3 CurrentDestination;
        public float RotationSpeed;
        [Space(20)]
		#endregion

		#region FreeMove Settings
		[Header("FreeMove Settings")]
		public bool EnableFreeMove;
        public bool useBackupMovement = true;
		public float PatrolSpeed;
		public float minIdleTime;
        public float maxIdleTime;

		[Header("Random Move Settings")]
		public bool moveOnRandomPath = false;
		public RandomMovementManager RandomMovementManager;

		[Header("Patrol Move Settings")]
		public bool moveOnPatrolPath = false;
		public TrackGizmos PatrolPoints;
		public int currentPatrolPoint = 0;
		public bool reachedCurrentControlPoint = false;

		[Space(10)]
		#endregion

		#region Eat Corpse State
		public bool EnableEatCorpseState;
		#endregion

		#region Investigate State
		[Header("Investigate State")]
		public bool EnableInvestigate;
		public bool HasLocationToInvestigate;
		public bool HasInvestigatedLocation;
		public Vector3 locationToInvestigate;
		#endregion

		#region Chase State
		[Header("Chase State")]
		public bool EnableChase;
		public float ChaseSpeed;
		public bool TargetInChaseRange => NpcPerception.IsTargetDetected;
		[SerializeField, ReadOnly] private bool targetInChaseRange;
		[Space(10)]
		#endregion

        #region Melee Attack State
        [Header("Melee Attack State")]
        public bool EnableMeleeAttack;
		public bool HasEquippedMeleeWeapon => EquipmentHandler.meleeWeaponInHands;
		public bool TargetInMeleeRange => TargetInMeleeRangeCheck();

		[SerializeField, ReadOnly] private bool hasEquippedMeleeWeapon;
		[SerializeField, ReadOnly] private bool targetInMeleeRange;
        [Space(10)]
        #endregion

        #region Ranged Attack State
        [Header("Ranged Attack State")]
        public bool EnableRangedAttack;
		public bool HasEquippedRangedWeapon => EquipmentHandler.rangedWeaponInHands;
		public bool TargetInShootingRange => TargetInShootingRangeCheck();

		[SerializeField, ReadOnly] private bool hasEquippedRangedWeapon;
		[SerializeField, ReadOnly] private bool targetInShootingRange;
		#endregion

		///<summery>
		/// move respawn related logic into a higher level object for npc pooling and reusing at a later date
		///<summery>

		public static event Action<GameObject> OnDeathComplete;

		#region awake + Initialize state machine method
		private void Awake()
        {
            #region component initializations
            Agent = GetComponent<NavMeshAgent>();
            Animator = GetComponent<Animator>();
			StatsHandler = GetComponent<StatsHandler>();
			EquipmentHandler = GetComponent<EquipmentHandler>();
			InventoryHandler = GetComponent<InventoryHandler>();
			NpcPerception = GetComponent<NpcPerception>();
			#endregion
        }

		public void InitializeStateMachine(NpcDefinition npcDefinition)
		{
            RotationSpeed = npcDefinition.RotationSpeed;
            PatrolSpeed = npcDefinition.PatrolSpeed;
            ChaseSpeed = npcDefinition.ChaseSpeed;
            minIdleTime = npcDefinition.MinIdleTime;
            maxIdleTime = npcDefinition.MaxIdleTime;

			Agent.speed = npcDefinition.PatrolSpeed;
			Agent.angularSpeed = npcDefinition.RotationSpeed;

			SwitchState(new NPCMoveState(this));
			Agent.enabled = true;
		}
		#endregion

		#region assign follow/patrol/spawn points
		public void AssignFollowPoint(RandomMovementManager randomMovementManager)
        {
            useBackupMovement = false;
            moveOnRandomPath = true;
			RandomMovementManager = randomMovementManager;
		}
        public void AssignPatrolPoint(TrackGizmos trackGizmos)
        {
			useBackupMovement = false;
			moveOnRandomPath = false;
            moveOnPatrolPath = true;
            PatrolPoints = trackGizmos;
		}
		#endregion

		#region event subbing/unsubbing
		private void OnEnable()
		{
			StatsHandler.OnDeath += HandleDeath;
		}
		private void OnDisable()
		{
			StatsHandler.OnDeath -= HandleDeath;
		}
		#endregion

		private void LateUpdate()
        {
            UpdateStateReadValues();
			UpdateAnimationMoveSpeed();
        }

		#region update read values
		private void UpdateStateReadValues()
        {
			CurrentStateName = currentState != null ? currentState.GetType().Name : "No State";

			targetInChaseRange = TargetInChaseRange;

            hasEquippedMeleeWeapon = HasEquippedMeleeWeapon;
            targetInMeleeRange = TargetInMeleeRange;

            hasEquippedRangedWeapon = HasEquippedRangedWeapon;
            targetInShootingRange = TargetInShootingRange;
        }
		#endregion

		#region update animation speed based on move speed
        private void UpdateAnimationMoveSpeed()
		{
			float smoothTime = 0.2f;
			if (Agent != null && Agent.enabled)
				CurrentSpeed = Mathf.Lerp(CurrentSpeed, Agent.velocity.magnitude, Time.deltaTime / smoothTime);

			Animator.SetFloat("Speed", CurrentSpeed);
        }
		#endregion

		#region target in melee/ranged attack ranges check
		private bool TargetInMeleeRangeCheck()
		{
			if (!NpcPerception.IsTargetDetected || !HasEquippedMeleeWeapon) return false;

			Vector3 targetPos = NpcPerception.DetectedTarget.Transform.position;

			if (Vector3.Distance(transform.position, targetPos) > Agent.stoppingDistance + 0.1f)
				return false;
			else
				return true;
		}

		private bool TargetInShootingRangeCheck()
        {
            if (!NpcPerception.IsTargetDetected || !HasEquippedRangedWeapon) return false;

            Vector3 targetPos = NpcPerception.DetectedTarget.Transform.position;
            float weaponRange = EquipmentHandler.rangedWeaponInHands.TypedDefinition.EffectiveRange;

			if (Vector3.Distance(transform.position, targetPos) > weaponRange) 
                return false;
            else
                return true;
        }
		#endregion

		#region death event listener + die coroutine and death complete invoking
		private void HandleDeath()
        {
            StartCoroutine(Die());
        }

        private IEnumerator Die()
        {
            if (Agent != null)
            {
                Agent.isStopped = true;
                Agent.velocity = Vector3.zero;
                Agent.enabled = false;
            }

            if (Animator != null)
                Animator.SetTrigger("Died");

            yield return new WaitForSeconds(3f);

            //will need replacing as spawners take over when to despawn dead enemies when player moves far away
            if (!StatsHandler.NpcDefinition.Player)
            {
                if (StatsHandler.forceRespawn)
				    OnDeathComplete?.Invoke(gameObject);
			}
            else
            {

            }
		}
		#endregion
	}
}
