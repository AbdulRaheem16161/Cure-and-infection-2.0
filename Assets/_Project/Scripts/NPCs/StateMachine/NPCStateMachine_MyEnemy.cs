using Game.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.InputSystem.XR;
using System.Collections.Generic;
using System;

namespace Game.MyNPC
{
    public class NPCStateMachine : StateMachine
    {
		public StatsHandler StatsHandler { get; private set; }

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

        #region FreeMove Settings
        [Header("FreeMove Settings")]
        public bool EnableFreeMove;
        public Transform PatrolFollowPoint;
        public Transform RandomFollowPoint;
        public bool moveOnPatrolPath = true;
        public bool moveOnRandomPath = false;
        public float PatrolSpeed;
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
        public GameObject WeaponHolder;
        public float RangedAttackRotSpeed;
        [Space(10)]
        #endregion

        ///<summery>
        /// move respawn related logic into a higher level object for npc pooling and reusing at a later date
        ///<summery>

		#region Respawn
		[Header("Respawn")]
        public Transform SpawnPoint;
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

		public void InitilizeStateMachine(NpcController npcController)
		{		
            #region initilize state machine
            StatsHandler = npcController.StatsHandler;
			#endregion

			#region set values from definition
			RotationSpeed = npcController.NpcDefinition.RotationSpeed;
            PatrolSpeed = npcController.NpcDefinition.PatrolSpeed;
            ChaseSpeed = npcController.NpcDefinition.ChaseSpeed;
			#endregion

			#region sub to events
			StatsHandler.OnDeath += HandleDeath;
			#endregion

			#region Transition to Default State
			SwitchState(new NPCIdleState(this)); //
			#endregion
		}

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

        public void HandleDeath()
        {
            Debug.LogError("handling death");
            StartCoroutine(Die());
        }

        public IEnumerator Die()
        {
            #region Disable Attack

            OpponentInMeleeAttackRange = false;

            #endregion

            #region Remove Gun
            WeaponHolder = null;
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

            yield return new WaitForSeconds(1f);

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
