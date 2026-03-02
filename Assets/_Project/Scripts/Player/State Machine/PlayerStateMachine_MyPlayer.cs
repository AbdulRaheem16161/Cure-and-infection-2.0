using Game.Core;
using UnityEngine;
using static UnityEditor.SceneView;
using System.Collections.Generic;

namespace Game.MyPlayer
{
    public class PlayerStateMachine : StateMachine, IDamageable
    {
        #region Player References
        [Header(" Player References")]
        public CharacterController Controller;
        public InputReader InputReader;
        public PlayerWeaponController GunManager;
        public Transform Body;
        public Transform CameraTransform;
        public Animator Animator;
        public GameObject Hitbox;
        public LadderTrigger LadderTrigger;
        public Transform SpawnPoint;

        #endregion

        [Space(10)]
        #region State Info
        [Header(" State Info")]
        [SerializeField, ReadOnly] public string CurrentStateName;
        [SerializeField, ReadOnly] public float CurrentSpeed;
        [SerializeField, ReadOnly] public bool IsGrounded;
        [SerializeField, ReadOnly] public bool IsCrouched;
        [SerializeField, ReadOnly] public bool FirstPerson;
        [SerializeField, ReadOnly] public bool ThirdPerson;

        #endregion

        #region Camera Targets
        public PlayerCamera PlayerCamera;
        public Transform StandingCameraTarget;
        public Transform CrouchedCameraTarget;
        public GameObject ThirdPersonBody;

        #endregion 

        [Space(10)]
        #region Movement Settings
        [Header(" Movement Settings")]
        public float MaxMoveSpeed;
        public float MinMoveSpeed;
        public float SpeedChangeRate;
        public float RotationSpeed;
        public float Gravity;
        #endregion

        [Space(10)]
        #region Crouch Settings
        [Header(" Crouch Settings")]
        public float MaxCrouchSpeed;
        public float MinCrouchSpeed;
        #endregion

        [Space(10)]
        #region Sprint Settings
        [Header(" Sprint Settings")]
        public float SprintSpeed;
        #endregion

        [Space(10)]
        #region Jump Settings
        [Header("Jump Settings")]
        [SerializeField, ReadOnly] public float VerticalVelocity;
        [SerializeField, ReadOnly] public bool CanJump = true;
        public float JumpHeight;
        public float JumpBufferDuration;
        private float _jumpBufferTimer;
        public float JumpGravity;
        #endregion

        [Space(10)]
        #region Attack
        [Header("Attack Settings")]
        public int Damage = 10;
        public float AttackDuration;

        [SerializeField] private List<string> targetTags = new List<string>();
        public override List<string> TargetTags
        {
            get { return targetTags; }
            set { targetTags = value; }
        }
        #endregion

        #region Health
        [Header("Health")]
        public int TotalHealth = 100;
        public int CurrentHealth;
        #endregion

        [Space(10)]
        #region Death
        [Header("Death Settings")]
        public bool isDead;
        #endregion

        [Space(10)]
        #region Ground Check
        [Header("Ground Check")]
        public Vector3 GroundCheckOffset = Vector3.down * 0.1f;
        public float GroundCheckRadius = 0.3f;
        public LayerMask GroundMask;
        public bool ShowGroundGizmo = true;
        #endregion

        [Space(10)]
        #region Ladder
        [Header("Ladder")]
        [SerializeField, ReadOnly] public bool IsNearLadder = false;
        public float LadderClimbingSpeed;
        #endregion

        private void Awake()
        {
            #region Transition to Default State
            SwitchState(new PlayerIdleState(this));
            #endregion

            #region Initializations
            CurrentHealth = TotalHealth;
            #endregion
        }

        private void LateUpdate()
        {
            #region Functions
            if (isDead) return;

            base.Update();
            UpdateStateName();
            ApplyGravity(Time.deltaTime);
            UpdateJumpBuffer();
            ReleaseLadderAtBottom();
            SingleLineUpdates();
            CameraMode();
            UpdateAnimator();
            #endregion
        }

        private void UpdateStateName()
        {
            #region Current State Name
            CurrentStateName = base.currentState != null ? base.currentState.GetType().Name : "No State";
            #endregion
        }

        private void ApplyGravity(float deltaTime)
        {
            #region Gravity    
            VerticalVelocity = IsGrounded && VerticalVelocity < 0f ? -2f : VerticalVelocity + Gravity * deltaTime;
            Controller.Move(new Vector3(0f, VerticalVelocity, 0f) * deltaTime);
            #endregion
        }

        public void UpdateJumpBuffer()
        {
            #region Update Jump Buffer
            if (IsGrounded)
            {
                _jumpBufferTimer += Time.deltaTime;
            }
            else
            {
                _jumpBufferTimer = 0f;
            }

            CanJump = _jumpBufferTimer >= JumpBufferDuration;
            #endregion
        }

        public void ReleaseLadderAtBottom()
        {
            #region Exit Ladder On Reaching The Ground
            if (InputReader.MovementInput.y <= -0.1f && IsGrounded)
            {
                IsNearLadder = false;
            }
            else
            {
                IsNearLadder = LadderTrigger.IsNearLadder;
            }
            #endregion
        }

        private bool IsGroundedSphere()
        {
            #region Ground Check
            return Physics.CheckSphere(transform.position + GroundCheckOffset, GroundCheckRadius, GroundMask);
            #endregion
        }

        private void SingleLineUpdates()
        {
            #region Single Line Updates

            IsGrounded = IsGroundedSphere();
            IsCrouched = InputReader.CrouchPressed;

            #endregion
        }

        private void UpdateAnimator()
        {
            #region Animator Parameters
            Animator.SetFloat("Speed", CurrentSpeed);
            Animator.SetBool("IsGrounded", IsGrounded);

            #endregion
        }

        public void RecieveDamage(int Damage, GameObject Attacker)
        {
            #region Recieve Damage

            Debug.Log($"Player received {Damage} damage.");
            CurrentHealth -= Damage;

            #endregion

            #region Die

            if (CurrentHealth <= 0)
            {
                Die();
            }

            #endregion
        }

        public void Die()
        {
            #region Die

            if (isDead) return; else isDead = true;

            #region Change tag
            this.gameObject.tag = "Dead";
            #endregion

            #region Stop Movement
            if (Controller != null)
            {
                Controller.enabled = false;
            }

            if (InputReader != null)
            {
                InputReader.enabled = false;
            }
            #endregion

            #region HitBox
            if (Hitbox != null)
            {
                Hitbox.SetActive(false);
            }
            #endregion

            #region Animator
            if (Animator != null)
            {
                Animator.SetTrigger("Died");
            }
            #endregion

            #region Disable all the Scripts  
            MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                if (script != this) // keep the state machine alive for respawn logic
                {
                    script.enabled = false;
                }
            }
            this.enabled = false; // in the end, disable its self too
            #endregion


            #endregion
        }

        public void CameraMode()
        {
            #region CameraMode
            FirstPerson = InputReader.FirstPersonCamera;
            ThirdPerson = InputReader.ThirdPersonCamera;
            #endregion
        }

        public void RangedAttack()
        {
            GunManager.Shoot();
        }

        private void OnDrawGizmosSelected()
        {
            #region IsGrounded Gizmos
            if (!ShowGroundGizmo) return;

            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position + GroundCheckOffset, GroundCheckRadius);
            #endregion
        }
    }
}