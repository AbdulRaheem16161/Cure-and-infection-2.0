using Game.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.InputSystem.XR;
using System.Collections.Generic;

namespace Game.MyNPC
{
    public class NPCStateMachine : StateMachine, IDamageable
    {
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

        #region Health
        [Header("Health")]
        public bool EnableHeatlh;
        public int TotalHealth = 100;
        public int CurrentHealth;
        #endregion

        #region Death
        [Header("Death")]
        public bool EnableDeath;
        public bool isDead;
        [Space(10)]
        #endregion

        #region Respawn
        [Header("Respawn")]
        public bool EnableRespawn;
        public Transform SpawnPoint;
        public GameObject BodyParts;
        public int WaitBeforeRespawn;
        #endregion

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
            #region initializations
            Agent = GetComponent<NavMeshAgent>();
            Animator = GetComponent<Animator>();

            CurrentHealth = TotalHealth;
            #endregion

            #region Transition to Default State
            SwitchState(new NPCIdleState(this)); //
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

        public void RecieveDamage(int Damage, GameObject Attacker)
        {
            #region Recieve Damage

            CurrentHealth -= Damage;

            DetectionRadius.EnableAlertMode(Attacker);
            DetectionCone.EnableAlertMode(Attacker);

            #endregion

            #region Die

            if (CurrentHealth <= 0)
            {
                StartCoroutine(Die());
            }

            #endregion
        }

        public IEnumerator Die()
        {
            #region Die

            if (!EnableDeath) yield return null;

            if (isDead) yield return null; else isDead = true;

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

            #endregion

            #region Respawn

            if (EnableRespawn)
            {
                StartCoroutine(Respawn());
            }
            #endregion
        }

        public IEnumerator Respawn()
        {
            #region Respawn

            yield return new WaitForSeconds(WaitBeforeRespawn);

            isDead = false;

            #region Change Tag
            this.gameObject.tag = "Enemy";
            #endregion

            #region Teleport
            transform.position = SpawnPoint.transform.position;
            #endregion

            #region ReEnable Movement
            if (Agent != null)
            {
                Agent.isStopped = false;
                Agent.enabled = true;
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

            #region Animator
            if (Animator != null)
            {
                Animator.SetTrigger("Respawn");
            }
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

            #region Reset Health
            CurrentHealth = TotalHealth;
            #endregion

            #endregion
        }
    }
}
