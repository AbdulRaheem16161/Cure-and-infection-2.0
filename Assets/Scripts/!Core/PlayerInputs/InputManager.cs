using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static GameManager;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [SerializeField] private InputActionAsset inputActions;

    #region Input Block Sections
    private bool CanUseMove => !IsBlocked(InputBlock.Move);
    private bool CanUseLook => !IsBlocked(InputBlock.Look);
    private bool CanUseCombat => !IsBlocked(InputBlock.Combat);
    private bool CanUseUi => !IsBlocked(InputBlock.UI);

    private InputBlock inputBlocks;
    [Flags]
    public enum InputBlock
    {
        None = 0,
        Look = 1 << 0,
        Move = 1 << 1,
        Combat = 1 << 2,
        UI = 1 << 3
    }
    #endregion


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
    public Vector2 Move => CanUseMove ? moveAction.ReadValue<Vector2>() : Vector2.zero;
    public Vector2 Look => CanUseLook ? lookAction.ReadValue<Vector2>() : Vector2.zero;

    //held inputs
    public bool PrimaryActionHeld => CanUseCombat && primaryAction.IsPressed();
    public bool SecondaryActionHeld => CanUseCombat && secondaryAction.IsPressed();
    public bool Sprinting => CanUseCombat && sprintAction.IsPressed();

    //one shot inputs
    public bool PrimaryActionPressed => CanUseCombat && primaryAction.WasPressedThisFrame();
    public bool SecondaryActionPressed => CanUseCombat && secondaryAction.WasPressedThisFrame();
    public bool ReloadAction => CanUseCombat && reloadAction.WasPressedThisFrame();
    public bool ToggleFireModeAction => CanUseCombat && toggleFireModeAction.WasPressedThisFrame();

    //interact inputs
    public bool InteractPressAction => CanUseUi && interactAction.WasPressedThisFrame();
    public bool InteractHoldAction => CanUseUi && interactAction.IsPressed();
    #endregion

    #region Ui Input Action Api
    public bool GameMenuAction => CanUseUi && gameMenuAction.WasPressedThisFrame();
    public bool PlayerInventoryAction => CanUseUi && playerInventoryAction.WasPressedThisFrame();
    #endregion

    private void Awake()
    {
        Instance = this;
        SetupPlayerInputActionsAndMap();
        SetupUiInputActionsAndMap();
        LoadInputControls();
    }

    private void Update()
    {
        LogBlockedInputs(false);
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
        UiManager.OnUiScreenChange += OnUiScreenChange;
        menuActionMap.Enable();
        gameplayActionMap.Enable();
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChange -= OnGameStateChange;
        UiManager.OnUiScreenChange -= OnUiScreenChange;
        menuActionMap.Disable();
        gameplayActionMap.Disable();
    }

    #region OnGameStateChange Event Enable/Disable gameplayActionMap
    private void OnGameStateChange(GameStates newState)
    {
        if (newState == GameStates.MainMenu)
            gameplayActionMap.Disable();
        else
            gameplayActionMap.Enable();
    }
    #endregion

    #region OnUiScreenChange Event To Enable/Disable Specific Inputs
    private void OnUiScreenChange(InputBlock inputBlock)
    {
        inputBlocks = inputBlock;

        //uncomment section for easy loging of blocked input group changes
        /* 
        InputBlock previous = inputBlocks;
        inputBlocks = inputBlock;

        InputBlock added = inputBlocks & ~previous;
        InputBlock removed = previous & ~inputBlocks;

        if (added != InputBlock.None)
            Debug.LogError($"InputBlocks ADDED: {added}");

        if (removed != InputBlock.None)
            Debug.LogError($"InputBlocks REMOVED: {removed}");
        */
    }
    #endregion

    #region HotbarPressed Action Inputs
    public bool AnyHotbarPressed(out int hotbarPressed)
    {
        if (CanUseCombat && HotbarPressed(0)) { hotbarPressed = 0; return true; }
        if (CanUseCombat && HotbarPressed(1)) { hotbarPressed = 1; return true; }
        if (CanUseCombat && HotbarPressed(2)) { hotbarPressed = 2; return true; }
        if (CanUseCombat && HotbarPressed(3)) { hotbarPressed = 3; return true; }
        if (CanUseCombat && HotbarPressed(4)) { hotbarPressed = 4; return true; }
        if (CanUseCombat && HotbarPressed(5)) { hotbarPressed = 5; return true; }

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

    #region Is Input Blocked Check
    private bool IsBlocked(InputBlock blockGroup)
    {
        return (inputBlocks & blockGroup) != 0;
    }
    #endregion

    #region Log Blocked Inputs
    private void LogBlockedInputs(bool log)
    {
        if (!log) return;
        foreach (InputBlock block in Enum.GetValues(typeof(InputBlock)))
        {
            if (block == InputBlock.None)
                continue;

            bool isBlocked = (inputBlocks & block) != 0;
            Debug.LogError($"{block}: {(isBlocked ? "BLOCKED" : "ALLOWED")}");
        }
    }
    #endregion
}
