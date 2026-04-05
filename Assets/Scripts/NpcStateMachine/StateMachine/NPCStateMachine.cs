using Game.Core;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

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
		public Animator Animator { get; private set; }
		public NavMeshAgent Agent { get; private set; }
		public NpcBeliefs Beliefs { get; private set; }
		public NpcPerception NpcPerception { get; private set; }
		public NpcDefinition NpcDefinition { get; private set; }
		public StatsHandler StatsHandler { get; private set; }
        public EquipmentHandler EquipmentHandler { get; private set; }
        public InventoryHandler InventoryHandler { get; private set; }

		#region Runtime Info
		[Header("Runtime Info")]
		public string CurrentStateName;
		public float CurrentSpeed;
		public Vector3 CurrentDestination;
		[Space(10)]
		#endregion

		#region NpcStates;
		private NpcStunnedState stunnedState;
		private NpcUseConsumableState useConsumableState;
		private NpcFleeState fleeState;
		private NPCRangedAttackState rangedAttackState;
		private NPCMeleeAttackState meleeAttackState;
		private NPCChaseState chaseState;
		private NPCEatCorpseState eatCorpseState;
		private NPCInvestigateState investigateState;
		private NpcIdleMovementState moveState;
		#endregion

		#region Npc State Toggles
		[Header("Npc State Toggles")]
		public bool EnableConsumableUse;
		public bool EnableFlee;
		public bool EnableRangedAttack;
		public bool EnableMeleeAttack;
		public bool EnableChase;
		public bool EnableEatCorpseState;
		public bool EnableInvestigate;
		#endregion

		#region Movement State Toggles
		[Header("Movement State Toggles")]
		public bool EnableMovement;

		[Header("Patrol Move")]
		public bool usePatrolMove = false;
		public TrackGizmos PatrolPoints;
		public int currentPatrolPoint = 0;
		public bool reachedCurrentControlPoint = false;

		[Header("Random Area Move")]
		public bool useRandomAreaMove = false;
		public RandomMovementManager RandomMovementManager;

		[Header("Random Move Settings")]
		public bool useRandomMove = true;
		#endregion

		///<summery>
		/// move respawn related logic into a higher level object for npc pooling and reusing at a later date
		///<summery>

		public static event Action<GameObject> OnDeathComplete;

		#region awake + Initialize state machine method
		private void Awake()
        {
            Agent = GetComponent<NavMeshAgent>();
            Animator = GetComponent<Animator>();
			StatsHandler = GetComponent<StatsHandler>();
			EquipmentHandler = GetComponent<EquipmentHandler>();
			InventoryHandler = GetComponent<InventoryHandler>();
			Beliefs = GetComponent<NpcBeliefs>();
			NpcPerception = GetComponent<NpcPerception>();

			stunnedState = new NpcStunnedState(this);
			useConsumableState = new NpcUseConsumableState(this);
			fleeState = new NpcFleeState(this);
			rangedAttackState = new NPCRangedAttackState(this);
			meleeAttackState = new NPCMeleeAttackState(this);
			chaseState = new NPCChaseState(this);
			eatCorpseState = new NPCEatCorpseState(this);
			investigateState = new NPCInvestigateState(this);
			moveState = new NpcIdleMovementState(this);
		}

		public void InitializeStateMachine(NpcDefinition npcDefinition)
		{
			NpcDefinition = npcDefinition;

			EnableConsumableUse = true;
			EnableFlee = true;
			EnableRangedAttack = true;
			EnableMeleeAttack = true;
			EnableChase = true;
			EnableInvestigate = true;
			EnableMovement = true;
			useRandomMove = true;

			if (npcDefinition.StartingLifeState == NpcDefinition.LifeState.zombified)
				EnableEatCorpseState = true;

			Agent.speed = npcDefinition.WalkSpeed;
			Agent.angularSpeed = npcDefinition.RotationSpeed;

			SwitchState(new NpcIdleMovementState(this));
			Agent.enabled = true;
		}
		#endregion

		#region assign follow/patrol/spawn points
		public void AssignPatrolPoints(TrackGizmos trackGizmos)
		{
			usePatrolMove = true;
			PatrolPoints = trackGizmos;
		}
		public void AssignMoveArea(RandomMovementManager randomMovementManager)
        {
			useRandomAreaMove = true;
			RandomMovementManager = randomMovementManager;
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

		protected override void Update()
		{
			// ---------- STATE PRIORITY DESCENDING ----------

			// Stay in current state if its conditions are still valid
			if (currentState == stunnedState && Beliefs.Stunned) { base.Update(); return; }
			if (currentState == useConsumableState && ShouldHeal()) { base.Update(); return; }
			if (currentState == fleeState && ShouldFlee()) { base.Update(); return; }

			if (currentState == rangedAttackState && ShouldRangedAttack()) { base.Update(); return; }
			if (currentState == meleeAttackState && ShouldMeleeAttack()) { base.Update(); return; }

			if (currentState == chaseState && ShouldChase()) { base.Update(); return; }
			if (currentState == eatCorpseState && ShouldEatCorpse()) { base.Update(); return; }
			if (currentState == investigateState && ShouldInvestigate()) { base.Update(); return; }

			if (currentState == useConsumableState && ShouldDrink()) { base.Update(); return; }
			if (currentState == useConsumableState && ShouldEat()) { base.Update(); return; }
			if (currentState == moveState && ShouldMove()) { base.Update(); return; }

			// Otherwise, switch to the highest-priority valid state
			if (Beliefs.Stunned) SwitchState(stunnedState);
			else if (ShouldHeal()) SwitchState(useConsumableState);
			else if (ShouldFlee()) SwitchState(fleeState);

			else if (ShouldRangedAttack()) SwitchState(rangedAttackState);
			else if (ShouldMeleeAttack()) SwitchState(meleeAttackState);

			else if (ShouldChase()) SwitchState(chaseState);
			else if (ShouldEatCorpse()) SwitchState(eatCorpseState);
			else if (ShouldInvestigate()) SwitchState(investigateState);

			else if (ShouldDrink()) SwitchState(useConsumableState);
			else if (ShouldEat()) SwitchState(useConsumableState);
			else if (ShouldMove()) SwitchState(moveState);
		}

		private void LateUpdate()
        {
			CurrentStateName = currentState != null ? currentState.GetType().Name : "No State";
			UpdateAnimationMoveSpeed();
        }

		#region State Transition Checks (based mostly on NpcBeliefs)
		private bool ShouldHeal()
		{
			return EnableConsumableUse && !Beliefs.Alert && Beliefs.Hurt && Beliefs.CanHeal;
		}
		private bool ShouldFlee()
		{
			return EnableFlee && Beliefs.TargetInFleeRange && !Beliefs.SafeFromFleeTarget && !Beliefs.MeleeWeaponInHands;
		}
		private bool ShouldRangedAttack()
		{
			return EnableRangedAttack && Beliefs.TargetInShootingRange && Beliefs.RangedWeaponInHands;
		}
		private bool ShouldMeleeAttack()
		{
			return EnableMeleeAttack && Beliefs.TargetInMeleeRange && Beliefs.MeleeWeaponInHands;
		}
		private bool ShouldChase()
		{
			return EnableChase && Beliefs.HasTarget && !Beliefs.TargetInShootingRange && !Beliefs.TargetInMeleeRange;
		}
		private bool ShouldEatCorpse()
		{
			return EnableEatCorpseState && !Beliefs.HasTarget && 
				StatsHandler.LifeState == NpcDefinition.LifeState.zombified && Beliefs.HasEatableTarget;
		}
		private bool ShouldInvestigate()
		{
			return EnableInvestigate && !Beliefs.HasTarget && Beliefs.FreeToInvestigate;
		}
		private bool ShouldMove()
		{
			return !Beliefs.HasTarget && !Beliefs.FreeToInvestigate &&
				!Beliefs.CanHeal && !Beliefs.CanDrink && !Beliefs.CanEat;
		}
		private bool ShouldDrink()
		{
			return EnableConsumableUse && !Beliefs.Alert && Beliefs.Thirsty && Beliefs.CanDrink;
		}
		private bool ShouldEat()
		{
			return EnableConsumableUse && !Beliefs.Alert && Beliefs.Hungry && Beliefs.CanEat;
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
