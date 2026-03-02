using UnityEngine;
using Game.MyPlayer;

public class PlayerCamera : MonoBehaviour
{
    
    #region General
    [Header("General")]
    public PlayerStateMachine stateMachine;
    public Transform StandingCameraTarget;
    public Transform CrouchedCameraTarget;
    public float mouseSensitivity = 2f;
    public int RotationSmoothness = 5;

    private float yaw = 0f;
    private float pitch = 20f;
    private bool cursorLocked = false;
    #endregion

    #region Camera Modes
    [Header("Camera Modes")]
    public GameObject ThirdPersonBody;
    public bool ThirdPerson;
    public bool FirstPerson;

    [Header("1st Person Settings")]
    public float firstPersonOffset = 0f;
    public bool LevitateCamera;
    public bool DescendCamera;

    [Header("3rd Person Settings")]
    public float thirdPersonOffset = 5f;
    #endregion

    #region Unity Methods
    void LateUpdate()
    {
        HandleControllerMode();
        HandleCameraMovement();
        HandleCursorLock();
    }
    #endregion

    void HandleControllerMode()
    {
        #region HandleControllerMode
        // Sync camera mode with player state
         FirstPerson = stateMachine.FirstPerson;  
         ThirdPerson = stateMachine.ThirdPerson;

        // Activate/deactivate third person body mesh
        ThirdPersonBody.SetActive(ThirdPerson);
        #endregion
    }

    void HandleCameraMovement()
    {
        #region HandleCameraMovement
        if (!cursorLocked) return;

        #region Camera Rotation
        // Handle mouse input and rotation
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -30f, 60f);

        // Apply rotation
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        #endregion

        #region Camera Positioning based on controller mode
        // Position camera AFTER rotation to prevent jitter
        if (ThirdPerson)
        {
            transform.position = StandingCameraTarget.position - transform.forward * thirdPersonOffset;
        }
        else if (FirstPerson)
        {
            Transform target = LevitateCamera ? StandingCameraTarget : CrouchedCameraTarget;
            if (target != null)
            {
                transform.position = target.position - transform.forward * firstPersonOffset;
            }
        }
        #endregion

        #endregion
    }


    void HandleCursorLock()
    {
        #region HandleCursorLock
        if (Input.GetMouseButtonDown(0))
        {
            cursorLocked = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            cursorLocked = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        #endregion
    }

}