using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [SerializeField] private InputActionAsset inputActions;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction sprintAction;

    public Vector2 Move => moveAction.ReadValue<Vector2>();
    public Vector2 Look => lookAction.ReadValue<Vector2>();
    public bool Sprinting => sprintAction.IsPressed();

    private void Awake()
    {
        Instance = this;
        InputActionMap playerMap = inputActions.FindActionMap("Player");

        moveAction = playerMap.FindAction("Move");
        lookAction = playerMap.FindAction("Look");
        sprintAction = playerMap.FindAction("Sprint");
    }

    private void OnEnable()
    {
        moveAction.Enable();
        lookAction.Enable();
        sprintAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
        sprintAction.Disable();
    }
}
