using System;
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
    private InputAction toggleFireModeAction;
    private InputAction sprintAction;
    public InputAction interactAction { get; private set; }

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
    public bool PrimaryActionHeld => primaryAction.IsPressed();
    public bool SecondaryActionHeld => secondaryAction.IsPressed();
    public bool Sprinting => sprintAction.IsPressed();

    //one shot inputs
    public bool PrimaryActionPressed => primaryAction.WasPressedThisFrame();
    public bool SecondaryActionPressed => secondaryAction.WasPressedThisFrame();
    public bool ReloadAction => reloadAction.WasPressedThisFrame();
    public bool ToggleFireModeAction => toggleFireModeAction.WasPressedThisFrame();

    //interact inputs
    public bool InteractPressAction => interactAction.WasPressedThisFrame();
    public bool InteractHoldAction => interactAction.IsPressed();
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
        LoadInputControls();
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
        toggleFireModeAction = gameplayActionMap.FindAction("ToggleFireMode", true);
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
    #endregion

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
    public bool AnyHotbarPressed(out int hotbarPressed)
    {
        if (HotbarPressed(0)) { hotbarPressed = 0; return true; }
        if (HotbarPressed(1)) { hotbarPressed = 1; return true; }
        if (HotbarPressed(2)) { hotbarPressed = 2; return true; }
        if (HotbarPressed(3)) { hotbarPressed = 3; return true; }
        if (HotbarPressed(4)) { hotbarPressed = 4; return true; }
        if (HotbarPressed(5)) { hotbarPressed = 5; return true; }

        hotbarPressed = -1;
        return false;
    }
    public bool HotbarPressed(int index)
    {
        if (index < 0 || index >= hotbarActions.Length)
            return false;

        return hotbarActions[index].WasPressedThisFrame();
    }
    #endregion
}
