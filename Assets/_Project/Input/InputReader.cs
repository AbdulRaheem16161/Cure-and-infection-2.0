using System;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour, PlayerInputAction.IPlayerActions
{
    private Vector2 movementInputVector2;
    private PlayerInputAction action;

    public event Action JumpAction;

    [SerializeField] public bool WalkKey { get; private set; }
    [SerializeField] private bool movementKeys;
    [SerializeField] private bool jumpKey;

    private void Awake()
    {
        action = new PlayerInputAction();
        action.Player.SetCallbacks(this);
    }

    private void OnEnable()
    {
        action.Player.Enable();
    }

    private void OnDisable()
    {
        action.Player.Disable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        movementInputVector2 = context.ReadValue<Vector2>();

        if (context.performed) movementKeys = true;
        else if(context.canceled) movementKeys = false;
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            WalkKey = true;
        }
        else if (context.canceled) WalkKey = false;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed) {
            jumpKey = true;
            JumpAction.Invoke();
        }
        else if (context.canceled) jumpKey = false;
    }

    // Getters:
    public Vector3 GetMovementInputVector3()
    {
        Vector3 movementVector3 = new Vector3(movementInputVector2.x, 0, movementInputVector2.y);
        return movementVector3.normalized;
    }
}
