using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Animator animator;

    #region Getters
    public Animator Animator => animator;
    #endregion

    [Space(10)]

    [Header("Settings")]
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float speedSmoothness = 0.2f;
    [SerializeField] private float gravity = 9.8f;

    [Space(10)]

    [Header("Live Values")]
    [SerializeField] private float currentSpeed;

    [Space(10)]

    [Header("Runtime Private Variables")]
    private Vector3 smoothHorizontalVelocity;
    private Vector3 velocitySmoothRef;
    private Vector3 verticalVelocity;

    #region Getters
    public Vector3 VerticalVelocity => verticalVelocity;
    #endregion

    [Space(10)]

    [Header("References")]
    [SerializeField] private const string IS_GROUNDED = "isGrounded";
    [SerializeField] private const string VELOCITY = "velocity";


    private void Update()
    {
        UpdateAnimatorParameters(); 
    }

    public void Move(Vector3 movementInputVector3, float speed) // Called from states such as moveState
    {
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camRight.y = 0f;
        camForward.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = camForward * movementInputVector3.z + camRight * movementInputVector3.x;
        moveDirection.Normalize();

        if (moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Applying Gravity
        if (characterController.isGrounded && verticalVelocity.y <= 0f) verticalVelocity.y = -2f;
        else verticalVelocity.y -= gravity * Time.deltaTime;

        // separating horizontal (movement) and Vertical (gravity & jump) Vectors so that moveSpeed dont effect the vertical motion
        Vector3 horizontal = moveDirection * speed;
        Vector3 vertical = new Vector3(0, verticalVelocity.y, 0);

        // smooth transition of velocity
        smoothHorizontalVelocity = Vector3.SmoothDamp(
            smoothHorizontalVelocity,
            horizontal,
            ref velocitySmoothRef,
            speedSmoothness
        );
        characterController.Move((smoothHorizontalVelocity + vertical) * Time.deltaTime);

        Vector3 horizontalSpeed = new Vector3(characterController.velocity.x, 0, characterController.velocity.z);
        currentSpeed = horizontalSpeed.magnitude; // ACTUAL CALCULATED VELOCITY
    }

    public void Jump(float jumpHeight)  // Called from jumpState
    {
        if (characterController.isGrounded)
        {
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * 2f * gravity);
        }
    }

    private void UpdateAnimatorParameters()
    {
        animator.SetBool(IS_GROUNDED, IsGrounded());
        animator.SetFloat(VELOCITY, currentSpeed);
    }

    // Getter functions
    public bool IsGrounded()
    {
        return characterController.isGrounded;
    }
}
