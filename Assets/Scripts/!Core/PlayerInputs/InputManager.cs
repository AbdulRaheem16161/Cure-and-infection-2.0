using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [SerializeField] private InputActionAsset inputActions;

    private InputActionMap playerMap;

    private InputAction moveAction;
    private InputAction lookAction;

    private InputAction primaryAction;
    private InputAction secondaryAction;
    private InputAction reloadAction;
    private InputAction sprintAction;
    private InputAction interactAction;

    private InputAction[] hotbarActions;

    //Movement
    public Vector2 Move => moveAction.ReadValue<Vector2>();
    public Vector2 Look => lookAction.ReadValue<Vector2>();

    //held inputs
    public bool PrimaryAction => primaryAction.IsPressed();
    public bool SecondaryAction => secondaryAction.IsPressed();
    public bool Sprinting => sprintAction.IsPressed();
    public bool InteractPressAction => interactAction.WasPressedThisFrame();
    public bool InteractHoldAction => interactAction.IsPressed();

    //one shot inputs
    public bool ReloadAction => reloadAction.WasPressedThisFrame();

    private void Awake()
    {
        Instance = this;

        playerMap = inputActions.FindActionMap("Player");

        moveAction = playerMap.FindAction("Move");
        lookAction = playerMap.FindAction("Look");

        primaryAction = playerMap.FindAction("PrimaryAction");
        secondaryAction = playerMap.FindAction("SecondaryAction");
        reloadAction = playerMap.FindAction("Reload");
        sprintAction = playerMap.FindAction("Sprint");
        interactAction = playerMap.FindAction("Interact");

        hotbarActions = new InputAction[]
        {
            playerMap.FindAction("HotbarOne"),
            playerMap.FindAction("HotbarTwo"),
            playerMap.FindAction("HotbarThree"),
            playerMap.FindAction("HotbarFour"),
            playerMap.FindAction("HotbarFive"),
            playerMap.FindAction("HotbarSix")
        };
    }

    private void OnEnable()
    {
        playerMap.Enable();
    }

    private void OnDisable()
    {
        playerMap.Disable();
    }

    public bool HotbarPressed(int index)
    {
        if (index < 0 || index >= hotbarActions.Length)
            return false;

        return hotbarActions[index].WasPressedThisFrame();
    }
}
