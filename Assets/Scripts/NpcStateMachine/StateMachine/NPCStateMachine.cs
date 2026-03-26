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
		public static event Action<GameObject> OnZombificationComplete;

        private void Awake()
        {
            #region component initializations
            Agent = GetComponent<NavMeshAgent>();
            Animator = GetComponent<Animator>();
			#endregion
        }

		public void InitializeStateMachine(StatsHandler statsHandler, EquipmentHandler equipmentHandler, InventoryHandler inventoryHandler, 
            NpcPerception npcPerception, NpcDefinition npcDefinition)
		{
			#region Initialize state machine
			StatsHandler = statsHandler;
            EquipmentHandler = equipmentHandler;
            InventoryHandler = inventoryHandler;
            NpcPerception = npcPerception;
            #endregion

            #region set values from definition
            RotationSpeed = npcDefinition.RotationSpeed;
            PatrolSpeed = npcDefinition.PatrolSpeed;
            ChaseSpeed = npcDefinition.ChaseSpeed;
            minIdleTime = npcDefinition.MinIdleTime;
            maxIdleTime = npcDefinition.MaxIdleTime;
			#endregion

			#region sub to events
			StatsHandler.OnDeath += HandleDeath;
			#endregion

			#region Transition to Default State
			SwitchState(new NPCMoveState(this));
			#endregion

			#region Enable Movement
			Agent.enabled = true;
			#endregion
		}

		#region assign follow/patrol/spawn points
		public void AssignFollowPoint(RandomMovementManager randomMovementManager)
        {
            moveOnRandomPath = true;
			RandomMovementManager = randomMovementManager;
		}
        public void AssignPatrolPoint(TrackGizmos trackGizmos)
        {
            moveOnRandomPath = false;
            moveOnPatrolPath = true;
            PatrolPoints = trackGizmos;
		}
		#endregion

		private void OnDestroy()
		{
			#region unsub from events
			StatsHandler.OnDeath -= HandleDeath;
			#endregion
		}

		private void LateUpdate()
        {
            #region Functions
            UpdateStateName();
            UpdateStateReadValues();
            RotateTowardsDestination();
            SingleLineUpdates();
            UpdateAnimations();
            #endregion
        }

        private void UpdateStateName()
        {
            #region Current State Name
            CurrentStateName = currentState != null ? currentState.GetType().Name : "No State";
            #endregion
        }

        private void UpdateStateReadValues()
        {
            targetInChaseRange = TargetInChaseRange;

            hasEquippedMeleeWeapon = HasEquippedMeleeWeapon;
            targetInMeleeRange = TargetInMeleeRange;

            hasEquippedRangedWeapon = HasEquippedRangedWeapon;
            targetInShootingRange = TargetInShootingRange;
        }

        private void RotateTowardsDestination()
        {
            #region RotateTowardsDestination
            if (Agent == null || !Agent.hasPath) return;

            // Direction from current position to destination
            Vector3 direction = (Agent.steeringTarget - transform.position).normalized;
            direction.y = 0f; // ignore vertical tilt

            if (direction.sqrMagnitude > 0.01f)
            {
                // Smooth rotate towards the target direction
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
            }
            #endregion
        }

        private void SingleLineUpdates()
        {
            #region Current Speed
            float smoothTime = 0.2f;
			if (Agent != null && Agent.enabled)
				CurrentSpeed = Mathf.Lerp(CurrentSpeed, Agent.velocity.magnitude, Time.deltaTime / smoothTime);
            #endregion
        }

        private void UpdateAnimations()
        {
            #region  Speed
            Animator.SetFloat("Speed", CurrentSpeed);
            #endregion
        }

		#region target in melee/ranged attack ranges check
		private bool TargetInMeleeRangeCheck()
		{
			if (!NpcPerception.IsTargetDetected || !hasEquippedMeleeWeapon) return false;

			Vector3 targetPos = NpcPerception.DetectedTarget.Transform.position;

			if (Vector3.Distance(transform.position, targetPos) > 3f)
				return false;
			else
				return true;
		}

		private bool TargetInShootingRangeCheck()
        {
            if (!NpcPerception.IsTargetDetected || !hasEquippedRangedWeapon) return false;

            Vector3 targetPos = NpcPerception.DetectedTarget.Transform.position;
            float weaponRange = EquipmentHandler.rangedWeaponInHands.TypedDefinition.EffectiveRange;

			if (Vector3.Distance(transform.position, targetPos) > weaponRange) 
                return false;
            else
                return true;
        }
		#endregion

		public void CompleteZombification()
		{
            OnZombificationComplete?.Invoke(gameObject);
		}

		public void HandleDeath()
        {
            StartCoroutine(Die());
        }

        public IEnumerator Die()
        {
            #region Change Tag
            gameObject.tag = "Dead";
            #endregion

            #region Stop Movement
            if (Agent != null)
            {
                Agent.isStopped = true;
                Agent.velocity = Vector3.zero;
                Agent.enabled = false;
            }

            #endregion

            #region Animator
            if (Animator != null)
            {
                Animator.SetTrigger("Died");
            }
            #endregion

			yield return new WaitForSeconds(3f);

            if (!StatsHandler.EnableZombification && StatsHandler.EnableRespawn)
				OnDeathComplete?.Invoke(gameObject);
		}
	}
}
