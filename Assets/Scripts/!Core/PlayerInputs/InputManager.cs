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

    #region Test Input Action Map + Input Actions
    private InputActionMap testActionMap;

    private InputAction testMoveAction;
    private InputAction testOneAction;
    private InputAction testTwoAction;
    private InputAction testThreeAction;
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

    #region Test Input Action Api
    public Vector2 TestMoveAction => testMoveAction.ReadValue<Vector2>();
    public bool TestOneAction => testOneAction.WasPressedThisFrame();
    public bool TestTwoAction => testTwoAction.WasPressedThisFrame();
    public bool TestThreeAction => testThreeAction.WasPressedThisFrame();
    #endregion

    private void Awake()
    {
        Instance = this;
        SetupPlayerInputActionsAndMap();
        SetupUiInputActionsAndMap();
        SetupTestInputActionsAndMap();
        LoadInputControls();
    }

    private void Update()
    {
        LogTestInputs(true);
    }

    private void LogTestInputs(bool log)
    {
        if (!log) return;
        Debug.Log($"Key: TestMoveAction Input: {TestMoveAction}");
        if (TestOneAction) Debug.Log("Key: TestOneAction was pressed");
        if (TestTwoAction) Debug.Log("Key: TestTwoAction was pressed");
        if (TestThreeAction) Debug.Log("Key: TestThreeAction was pressed");
    }

    #region Save/Load Inputs
    public static void SaveInputControls()
    {
        string json = Instance.inputActions.SaveBindingOverridesAsJson();

        PlayerPrefs.SetString("Bindings", json);
    }
    public static void LoadInputControls()
    {
        string json = PlayerPrefs.GetString("Bindings", "");

        if (string.IsNullOrEmpty(json)) return;
        Instance.inputActions.LoadBindingOverridesFromJson(json);
    }
    #endregion

    #region Setup Input Actions + Map
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

    private void SetupTestInputActionsAndMap()
    {
        testActionMap = inputActions.FindActionMap("Test", true);

        testMoveAction = testActionMap.FindAction("TestMove", true);
        testOneAction = testActionMap.FindAction("TestOne", true);
        testTwoAction = testActionMap.FindAction("TestTwo", true);
        testThreeAction = testActionMap.FindAction("TestThree", true);
    }
    #endregion

    private void OnEnable()
    {
        GameManager.OnGameStateChange += OnGameStateChange;
        menuActionMap.Enable();
        gameplayActionMap.Enable();
        testActionMap.Enable();
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChange -= OnGameStateChange;
        menuActionMap.Disable();
        gameplayActionMap.Disable();
        testActionMap.Disable();
    }

    public static InputActionAsset GetInputActionAsset()
    {
        return Instance.inputActions;
    }

    #region OnGameStateChange Event Enable/Disable gameplayActionMap
    public void OnGameStateChange(GameStates newState)
    {
        if (newState == GameStates.MainMenu)
            gameplayActionMap.Disable();
        else
            gameplayActionMap.Enable();
    }
    #endregion

    #region HotbarPressed Action Inputs
    public bool HotbarPressed(int index)
    {
        if (index < 0 || index >= hotbarActions.Length)
            return false;

        return hotbarActions[index].WasPressedThisFrame();
    }
    #endregion
}
