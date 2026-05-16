using Game.Core;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static NpcBaseMovementState;

namespace Game.MyNPC
{
	[RequireComponent(typeof(Animator))]
	[RequireComponent(typeof(NavMeshAgent))]
	[RequireComponent(typeof(NpcBeliefs))]
	[RequireComponent(typeof(NpcPerception))]
	[RequireComponent(typeof(StatsHandler))]
	[RequireComponent(typeof(EquipmentHandler))]
	[RequireComponent(typeof(InventoryHandler))]
    public class NPCStateMachine : StateMachine
	{
        private static readonly int SpeedHash = Animator.StringToHash("Speed");

        public EntityDefinition Definition { get; private set; }
		public Animator Animator { get; private set; }
		public NavMeshAgent Agent { get; private set; }
		public NpcController NpcController { get; private set; }
		public NpcBeliefs Beliefs { get; private set; }
		public NpcPerception NpcPerception { get; private set; }
		public StatsHandler StatsHandler { get; private set; }
        public EquipmentHandler EquipmentHandler { get; private set; }
        public InventoryHandler InventoryHandler { get; private set; }

		#region Runtime Info
		[Header("Runtime Info")]
		public string CurrentStateName;
		[HideInInspector] public MoveType moveIntent;
		[HideInInspector] public bool IsSprinting;
		public float CurrentSpeed;
		public Vector3 CurrentDestination;
		[Space(10)]
		#endregion

		#region NpcStates;
		public List<NPCBaseState> states = new();

		private NpcStunnedState stunnedState;
		private NpcFleeState fleeState;
		private NpcMoveToCoverState moveToCoverState;
		private NpcHealState healState;
		private NPCRangedAttackState rangedAttackState;
		private NPCMeleeAttackState meleeAttackState;
		private NPCChaseState chaseState;
		private NPCEatCorpseState eatCorpseState;
		private NPCInvestigateState investigateState;
		private NpcDrinkState drinkState;
		private NpcEatState eatState;
		private NpcIdleMovementState moveState;
		#endregion

		#region Npc State Toggles
		[Header("Npc State Toggles")]
		public EntityDefinition.Capability capabilityOverrides;
		#endregion

		#region Movement State Toggles
		[Header("Movement State Toggles")]
		public bool EnableMovement;
		public MovementType movementType;
		public enum MovementType
		{
			randomMove, randomAreaMove, patrolMove
		}

		[Header("Patrol Move")]
		public PatrolPathManager PatrolPathManager;
		public int currentPatrolPoint = 0;
		public bool reachedCurrentControlPoint = false;

		[Header("Random Area Move")]
		public RandomAreaMoveManager RandomAreaMoveManager;
		#endregion

		#region Npc Range Consideration Toggles
		[Header("Npc Range Consideration Toggles")]
		[HideInInspector] public bool showUnholsteredWeaponRange;
		[HideInInspector] public bool showFleeRange;
		#endregion

		public event Action<NpcController> OnDeathComplete;

		///<summery>
		/// move respawn related logic into a higher level object for npc pooling and reusing at a later date
		///<summery>

		#region awake + Initialize state machine method
		private void Awake()
        {
            Agent = GetComponent<NavMeshAgent>();
            Animator = GetComponent<Animator>();
			NpcController = GetComponent<NpcController>();
			Beliefs = GetComponent<NpcBeliefs>();
			NpcPerception = GetComponent<NpcPerception>();
			StatsHandler = GetComponent<StatsHandler>();
			EquipmentHandler = GetComponent<EquipmentHandler>();
			InventoryHandler = GetComponent<InventoryHandler>();
		}

		public void InitializeStateMachine(EntityDefinition definition)
		{
			Definition = definition;

			stunnedState = new NpcStunnedState(this, 100);
			fleeState = new NpcFleeState(this, 95);
			moveToCoverState = new NpcMoveToCoverState(this, 90);
			healState = new NpcHealState(this, 85);
			rangedAttackState = new NPCRangedAttackState(this, 75);
			meleeAttackState = new NPCMeleeAttackState(this, 70);
			chaseState = new NPCChaseState(this, 60);
			eatCorpseState = new NPCEatCorpseState(this, 55);
			investigateState = new NPCInvestigateState(this, 50);
			drinkState = new NpcDrinkState(this, 35);
			eatState = new NpcEatState(this, 30);
			moveState = new NpcIdleMovementState(this, 10);

			states.Add(stunnedState);
			states.Add(fleeState);
			states.Add(moveToCoverState);
			states.Add(healState);
			states.Add(rangedAttackState);
			states.Add(meleeAttackState);
			states.Add(chaseState);
			states.Add(eatCorpseState);
			states.Add(investigateState);
			states.Add(drinkState);
			states.Add(eatState);
			states.Add(moveState);

			capabilityOverrides = Definition.Capabilities;
			Agent.speed = Definition.WalkSpeed;
			Agent.angularSpeed = Definition.RotationSpeed;
			Agent.acceleration = Definition.Acceleration;
			Agent.stoppingDistance = Definition.StoppingDistance;

			SwitchState(moveState);
			Agent.enabled = true;
		}
		#endregion

		#region assign follow/patrol/spawn points
		public void SetMovementType(MovementType type, PatrolPathManager patrolPathManager)
		{
			PatrolPathManager = patrolPathManager;
			movementType = type;
		}
		public void SetMovementType(MovementType type, RandomAreaMoveManager randomAreaMoveManager)
		{
			RandomAreaMoveManager = randomAreaMoveManager;
			movementType = type;
		}
		public void SetMovementType(MovementType type)
		{
			movementType = type;
		}
		#endregion

		#region event subbing/unsubbing
		private void OnEnable()
		{
			StatsHandler.OnDeath += HandleDeath;
			StatsHandler.OnExhausted += HandleExhausted;
		}
		private void OnDisable()
		{
			StatsHandler.OnDeath -= HandleDeath;
			StatsHandler.OnExhausted -= HandleExhausted;
		}
		#endregion

		protected override void Update()
		{
			if (StatsHandler.LifeState == EntityDefinition.LifeState.dead) return;

			///<summary>
			/// old way, if performance becomes an issue, revert back to this. only ticking currentState IsValid before priority switching
			/// all states IsValid methods would need updating to include more complex checks (chase state checking melee/weapon range etc)
			/// if (currentState != null && currentState.IsValid()) { currentState.Tick(Time.deltaTime); return; }
			/// </summary>

			currentState?.Tick(Time.deltaTime);
			HandlePriorityStateSwitches();
		}

		private void LateUpdate()
        {
			CurrentStateName = currentState != null ? currentState.GetType().Name : "No State";
			UpdateAnimationMoveSpeed();
        }

		#region Handle Priority State Switches
		private void HandlePriorityStateSwitches()
		{
			NPCBaseState bestState = null;
			int bestPriority = int.MinValue;

			foreach (NPCBaseState state in states)
			{
				if (!state.IsValid()) continue;

				if (state.Priority > bestPriority)
				{
					bestState = state;
					bestPriority = state.Priority;
				}
			}

			if (bestState != null && bestState != currentState)
				SwitchState(bestState);
		}
		#endregion

		#region update animation speed based on move speed
		private void UpdateAnimationMoveSpeed()
		{
			float smoothTime = 0.2f;
			if (Agent != null && Agent.enabled)
				CurrentSpeed = Mathf.Lerp(CurrentSpeed, Agent.velocity.magnitude, Time.deltaTime / smoothTime);

			Animator.SetFloat(SpeedHash, CurrentSpeed);
        }
		#endregion

		#region Handle Exhausted Event Listener
		protected void HandleExhausted(bool exhausted)
		{
			if (currentState is NpcBaseMovementState baseMovementState)
				baseMovementState.UpdateMoveSpeed(moveIntent);
		}
		#endregion

		#region Handle Death Event Listener
		private void HandleDeath()
        {
			if (Agent != null)
			{
				Agent.isStopped = true;
				Agent.velocity = Vector3.zero;
				Agent.enabled = false;
			}

			if (Animator != null)
				Animator.SetTrigger("Died");
		}
		#endregion

		#region Gizmos
		private void OnDrawGizmos()
		{
			if (showUnholsteredWeaponRange)
			{
				Gizmos.color = Color.green;

				if (EquipmentHandler == null || EquipmentHandler.itemInHands == null) return;

				if (EquipmentHandler.itemInHands.ItemDefinition is WeaponRangedDefinition weaponRanged)
					Gizmos.DrawWireSphere(transform.position, weaponRanged.EffectiveRange);
			}

			if (showFleeRange)
			{
				Gizmos.color = Color.red;
				if (Definition == null) return;
				Gizmos.DrawWireSphere(transform.position, Definition.FleeDistance);
			}
		}
		#endregion
	}
}
