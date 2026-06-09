using UnityEngine;
using UnityEngine.InputSystem;
using static GameManager;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [SerializeField] private InputActionAsset inputActions;

    #region Player Input Action Map + Input Actions
    private InputActionMap gameplayActionMap;

    private InputAction moveAction;
    private InputAction lookAction;

    private InputAction primaryAction;
    private InputAction secondaryAction;
    private InputAction reloadAction;
    private InputAction sprintAction;
    private InputAction interactAction;

    private InputAction[] hotbarActions;
    #endregion

    #region Ui Input Action Map + Input Actions
    private InputActionMap menuActionMap;

    private InputAction gameMenuAction;
    private InputAction playerInventoryAction;
    #endregion

    #region Player Input Action Api
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
    #endregion

    #region Ui Input Action Api
    public bool GameMenuAction => gameMenuAction.WasPressedThisFrame();
    public bool PlayerInventoryAction => playerInventoryAction.WasPressedThisFrame();
    #endregion

    private void Awake()
    {
        Instance = this;
        SetupPlayerInputActionsAndMap();
        SetupUiInputActionsAndMap();
    }

    private void SetupPlayerInputActionsAndMap()
    {
        gameplayActionMap = inputActions.FindActionMap("Gameplay", true);

        moveAction = gameplayActionMap.FindAction("Move", true);
        lookAction = gameplayActionMap.FindAction("Look", true);

        primaryAction = gameplayActionMap.FindAction("PrimaryAction", true);
        secondaryAction = gameplayActionMap.FindAction("SecondaryAction", true);
        reloadAction = gameplayActionMap.FindAction("Reload", true);
        sprintAction = gameplayActionMap.FindAction("Sprint", true);
        interactAction = gameplayActionMap.FindAction("Interact", true);

        hotbarActions = new InputAction[]
        {
            gameplayActionMap.FindAction("HotbarOne", true),
            gameplayActionMap.FindAction("HotbarTwo", true),
            gameplayActionMap.FindAction("HotbarThree", true),
            gameplayActionMap.FindAction("HotbarFour", true),
            gameplayActionMap.FindAction("HotbarFive", true),
            gameplayActionMap.FindAction("HotbarSix", true)
        };
    }

    private void SetupUiInputActionsAndMap()
    {
        menuActionMap = inputActions.FindActionMap("Menu", true);

        gameMenuAction = menuActionMap.FindAction("Back", true);
        playerInventoryAction = menuActionMap.FindAction("PlayerInventory", true);
    }

    private void OnEnable()
    {
        GameManager.OnGameStateChange += OnGameStateChange;
        menuActionMap.Enable();
        gameplayActionMap.Enable();
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChange -= OnGameStateChange;
        menuActionMap.Disable();
        gameplayActionMap.Disable();
    }

    public void OnGameStateChange(GameStates newState)
    {
        if (newState == GameStates.MainMenu)
            gameplayActionMap.Disable();
        else
            gameplayActionMap.Enable();
    }

    public bool HotbarPressed(int index)
    {
        if (index < 0 || index >= hotbarActions.Length)
            return false;

        return hotbarActions[index].WasPressedThisFrame();
    }
}
