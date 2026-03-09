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

        #region General Values
        [Header("General Values")]
        public Animator Animator;
        public NavMeshAgent Agent;
        public float CurrentSpeed;
        public string CurrentStateName;
        public Transform CurrentFollowPoint;
        public float RotationSpeed;
        [Space(20)]
		#endregion

		#region Attack (General)
		[SerializeField] private List<string> targetTags = new List<string>();
		public override List<string> TargetTags
		{
			get { return targetTags; }
			set { targetTags = value; }
		}

		[Space(10)]
		#endregion

		#region Melee Attack State
		[Header("Melee Attack State")]
		public bool EnableMeleeAttack;
		public GameObject Hitbox;
		public GameObject AttackRangeTrigger;
		public int Damage = 5;
		public bool OpponentInMeleeAttackRange;
		public bool HasEquippedMeleeWeapon => EquipmentHandler.meleeWeaponInHands;
		public float AttackDuration;
		public float HitboxActivationDelay;
		public float MinWaitBeforeFreeMove;
		public float MaxWaitBeforeFreeMove;
		[Space(10)]
		#endregion

		#region Ranged Attack State
		[Header("Ranged Attack State")]
		public bool EnableRangedAttack;
		public bool OpponentInRangedAttackRange;
        public bool HasEquippedRangedWeapon => EquipmentHandler.rangedWeaponInHands;
		public float RangedAttackRotSpeed;
		[Space(10)]
		#endregion

		#region Chase State
		[Header("Chase State")]
		public bool EnableChase;
		public DetectionCone DetectionCone;
		public DetectionRadius DetectionRadius;
		public float ChaseSpeed;
        [Space(10)]
        #endregion

        #region Investigate State
        [Header("Investigate State")]
        public bool EnableInvestigate;
        public bool HasLocationToInvestigate;
		public bool HasInvestigatedLocation;
        public Vector3 locationToInvestigate;
		#endregion

		#region FreeMove Settings
		[Header("FreeMove Settings")]
        public bool EnableFreeMove;

        [Header("Random Move Settings")]
		public bool moveOnRandomPath = false;
		public GameObject RandomFollowPoint;

		[Header("Patrol Move Settings")]
		public bool moveOnPatrolPath = false;
        public GameObject PatrolFollowPoint;
		public float PatrolSpeed;

		[Space(10)]
        #endregion

        ///<summery>
        /// move respawn related logic into a higher level object for npc pooling and reusing at a later date
        ///<summery>

		#region Respawn
		[Header("Respawn")]
        public GameObject SpawnPoint;
		#endregion

		public static event Action<GameObject> OnDeathComplete;

		private void OnOff()
        {
            #region Editor Refresh (throttled for performance)
#if UNITY_EDITOR
            // Only refresh in edit mode, and not every frame (every ~0.5s)
            if (!Application.isPlaying && Time.frameCount % 30 == 0)
            {
                EditorApplication.QueuePlayerLoopUpdate();
                SceneView.RepaintAll();
            }
#endif
            #endregion

            #region Free Move Modes
            // Fix your logic (your previous code made them fight each other endlessly)
            if (moveOnPatrolPath) moveOnRandomPath = false;
            if (moveOnRandomPath) moveOnPatrolPath = false;
            #endregion

            #region ON/OFF
            if (DetectionCone != null)
                DetectionCone.enabled = EnableChase;

            if (DetectionRadius != null)
                DetectionRadius.enabled = EnableChase;

            if (Hitbox != null)

                if (AttackRangeTrigger != null)
                    AttackRangeTrigger.SetActive(EnableMeleeAttack);
            #endregion
        }

        private void Awake()
        {
            #region component initializations
            Agent = GetComponent<NavMeshAgent>();
            Animator = GetComponent<Animator>();
			#endregion
        }

		public void InitializeStateMachine(
            StatsHandler statsHandler, EquipmentHandler equipmentHandler, InventoryHandler inventoryHandler, NpcDefinition npcDefinition, Teams NPCsTeam)
		{
			#region Initialize state machine
			StatsHandler = statsHandler;
            EquipmentHandler = equipmentHandler;
            InventoryHandler = inventoryHandler;
			#endregion

			#region set values from definition
			RotationSpeed = npcDefinition.RotationSpeed;
            PatrolSpeed = npcDefinition.PatrolSpeed;
            ChaseSpeed = npcDefinition.ChaseSpeed;
			#endregion

			#region Set NPC's Team and Opponent Tags of the NPC
			tag = NPCsTeam.ToString().Replace("Team", "Team "); // assign Team tag while changing Team(n) to Team (n)

			List<string> NPCsTargetTags = new List<string>();

			foreach (Teams t in Enum.GetValues(typeof(Teams)))
			{
				// add all the teams of the Enum "Teams" in the TargetTags list except for its own team

				if (t == NPCsTeam && NPCsTeam != Teams.FreeFighter) // if its not a free fighter then skip its own tag
					continue;

				string teamName = t.ToString();
				teamName = teamName.Replace("Team", "Team "); // change Team(n) to Team (n)

				NPCsTargetTags.Add(teamName);
			}

			TargetTags = NPCsTargetTags;
			#endregion

			#region sub to events
			StatsHandler.OnDeath += HandleDeath;
			#endregion

			#region Transition to Default State
			SwitchState(new NPCIdleState(this)); //
			#endregion

			#region Enable Movement
			Agent.enabled = true;
			#endregion

			#region Enable Scripts
			MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
			foreach (MonoBehaviour script in scripts)
			{
				if (script != this)
				{
					script.enabled = true;
				}
			}
			#endregion

			#region Enable Colliders

			Collider[] colliders;

			colliders = GetComponentsInChildren<Collider>();

			if (colliders != null)
			{
				foreach (var col in colliders)
				{
					col.enabled = true;
				}
			}
			#endregion
		}

		#region assign follow/patrol/spawn points
		public void AssignFollowPoint(GameObject followPoint)
        {
            moveOnRandomPath = true;
            RandomFollowPoint = followPoint;
		}
        public void AssignPatrolPoint(GameObject patrolPoint, TrackGizmos trackGizmos)
        {
            moveOnRandomPath = false;
            moveOnPatrolPath = true;
            PatrolFollowPoint = patrolPoint;
			PatrolFollowPoint.GetComponent<PatrolFollowPoint>().ItsFollower = gameObject;
			PatrolFollowPoint.GetComponent<PatrolFollowPoint>().TrackGizmos = trackGizmos;
		}
        public void AssignSpawnPoint(GameObject spawnPoint)
        {
            SpawnPoint = spawnPoint;
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
            RotateTowardsDestination();
            SingleLineUpdates();
            UpdateAnimations();
            OnOff();
            #endregion
        }

        private void UpdateStateName()
        {
            #region Current State Name
            CurrentStateName = base.currentState != null ? base.currentState.GetType().Name : "No State"; //
            #endregion
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
            CurrentSpeed = Mathf.Lerp(CurrentSpeed, Agent.velocity.magnitude, Time.deltaTime / smoothTime);
            #endregion
        }

        private void UpdateAnimations()
        {
            #region  Speed
            Animator.SetFloat("Speed", CurrentSpeed);
            #endregion
        }

        public void DetectSound(Vector3 soundPosition)
        {
            locationToInvestigate = soundPosition;

            //ignore if doing more important things
            if (currentState is NPCRangedAttackState || currentState is NPCMeleeAttackState || currentState is NPCChaseState) return;
			SwitchState(new NPCInvestigateState(this));
		}

        public void HandleDeath()
        {
            StartCoroutine(Die());
        }

        public IEnumerator Die()
        {
            #region Disable Attack

            OpponentInMeleeAttackRange = false;

            #endregion

            #region Change Tag
            this.gameObject.tag = "Dead";
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

            #region Disable Scripts
            MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                if (script != this)
                {
                    script.enabled = false;
                }
            }
            #endregion

            yield return new WaitForSeconds(3f);

            #region Disable Colliders

            Collider[] colliders;

            colliders = GetComponentsInChildren<Collider>();

            if (colliders != null)
            {
                foreach (var col in colliders)
                {
                    col.enabled = false;
                }
            }
            #endregion

            OnDeathComplete?.Invoke(gameObject);
        }
	}
}
