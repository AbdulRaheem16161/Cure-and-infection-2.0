using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class PlayerStateMachine : StateMachine
{
    public PlayerIdleState IdleState { get; private set; }
    public PlayerMoveState moveState { get; private set; }
    public PlayerJumpState JumpState { get; private set; }

    [Header("Allowed States")]
    [SerializeField] private bool canMove;
    [SerializeField] private bool canJump;

    #region Getters;
    public bool CanMove => canMove;
    public bool CanJump => canJump;
    #endregion

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private InputReader inputReader;
    [SerializeField] private PlayerStats stats;
    [SerializeField] private PlayerController playerController;

    #region Getters
    public PlayerController PlayerController => playerController;
    #endregion

    [Header("Input Intent Flags")]
    public bool JumpTriggered;
    public bool WalkPressed => inputReader.WalkKey;

    [Header("Stats Reference")]
    [HideInInspector] public float RunSpeed => stats.runSpeed;
    [HideInInspector] public float WalkSpeed => stats.walkSpeed;
    [HideInInspector] public float JumpHeight => stats.jumpHeight;
    [Space(10)]

    [Header("Debug")]
    [SerializeField] private string currentState;

    #region Setters
    public void setCurrentStateString(string newState) => currentState = newState; //  called from PlayerBaseState
    #endregion

    [Space(10)]

    [Header("Runtime Variables / Debug")]
    [SerializeField] private Vector3 movementVector3;
    [SerializeField] private Vector3 velocity;

    #region Getters
    public Vector3 MovementVector3 => movementVector3;
    #endregion

    private void OnEnable()
    {
        inputReader.JumpAction += OnJumpPressed;
    }

    private void OnDisable()
    {
        inputReader.JumpAction -= OnJumpPressed;
    }

    protected void Awake()
    {
        IdleState = new PlayerIdleState(this);
        moveState = new PlayerMoveState(this);
        JumpState = new PlayerJumpState(this);

        SwitchState(IdleState);
    }

    protected void Update()
    {
        base.Update();
    }

    public Vector3 GetMovementInputVector3()
    {
        movementVector3 = inputReader.GetMovementInputVector3();
        return movementVector3;
    }

    private void OnJumpPressed()  
    {
        if (!canJump) return;

        if (!playerController.IsGrounded()) return;

        JumpTriggered = true;
    }

}
