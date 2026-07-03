using System;
using System.Collections.Generic;
using UnityEngine;
using static InputManager;

public class UiManager : MonoBehaviour
{
    public static UiManager Instance { get; private set; }
    public static bool CursorLocked { get; private set; }

    public LoadingTransitionUi LoadingTransitionPanel;

    public MainMenuUi MainMenuPanel;
    public SettingsUi SettingsPanel;
    public GameplaySettingsUi GameplaySettingsPanel;
    public ControlSettingsUi ControlSettingsPanel;
    public GraphicsSettingsUi GraphicsSettingsPanel;
    public AudioSettingsUi AudioSettingsPanel;

    public PlayerHudUi PlayerHudPanel;
    public PlayerInventoryUi PlayerInventoryPanel;
    public LootablesInventoryUi LootablesInventoryPanel;

    public Stack<UiContext> currentUiStack = new();

    private Dictionary<UiScreens, IUiPanel> uiPanels;

    public static event Action<InputBlock> OnUiScreenChange;

    public enum UiScreens
    {
        menu,
        saveGame,
        loadGame,
        settings,
        gameplaySettings,
        controlSettings,
        graphicsSettings,
        audioSettings,

        playerHud,
        playerInventory,
        LootableInventory,
    }

    private void Awake()
    {
        CursorLocked = true;
        Instance = this;
        CreateUiPanelDictionary();
    }

    private void Update()
    {
        HandleBackAction();
        HandlePlayerInventoryAction();
        LogCurrentUiStack(false);
    }

    #region Log Current Ui Stack
    private void LogCurrentUiStack(bool log)
    {
        if (!log || currentUiStack.Count <= 0) return;

        string message = "CurrentUi Stack: ";

        foreach (var ui in currentUiStack)
            message += $"{ui.uiScreen} | ";

        Debug.LogError(message);
    }
    #endregion

    #region CreateUiPanelDictionary
    private void CreateUiPanelDictionary()
    {
        uiPanels = new Dictionary<UiScreens, IUiPanel>
        {
            { UiScreens.menu, MainMenuPanel },
            { UiScreens.settings, SettingsPanel },
            { UiScreens.gameplaySettings, GameplaySettingsPanel },
            { UiScreens.controlSettings, ControlSettingsPanel },
            { UiScreens.graphicsSettings, GraphicsSettingsPanel },
            { UiScreens.audioSettings, AudioSettingsPanel },

            { UiScreens.playerInventory, PlayerInventoryPanel },
            { UiScreens.LootableInventory, LootablesInventoryPanel },
        };
    }
    #endregion

    #region Reset Ui Screens Api
    public static void ResetUiScreens()
    {
        foreach (var kvp in Instance.uiPanels)
            kvp.Value.HideUi();

        Instance.currentUiStack.Clear();
        Instance.RefreshUiState();
    }
    #endregion

    #region Show/Hide Ui Screens Api
    public static void ToggleScreen(UiContext uiContext)
    {
        if (UiScreenAlreadyVisible(uiContext))
            Instance.ShowPreviousUi(true);
        else
            Instance.PushAndShowUi(uiContext);

        Instance.RefreshUiState();
    }
    public static void HideTopScreen()
    {
        Instance.ShowPreviousUi(true);
        Instance.RefreshUiState();
    }
    private static bool UiScreenAlreadyVisible(UiContext uiContext)
    {
        return Instance.currentUiStack.Count > 0 && Instance.currentUiStack.Peek().uiScreen == uiContext.uiScreen;
    }
    #endregion

    #region Ui Panel Stacking Logic
    public void ShowPreviousUi(bool popTopUi)
    {
        if (currentUiStack.Count <= 0) return;

        var currentUi = currentUiStack.Peek();

        if (!CanClose(currentUi.uiScreen)) return;

        HideUi(currentUi);
        if (popTopUi) currentUiStack.Pop();
        if (currentUiStack.Count > 0) ShowUi(currentUiStack.Peek());
    }

    private void PushAndShowUi(UiContext uiContext)
    {
        if (currentUiStack.Count > 0)
            HideUi(currentUiStack.Peek());

        currentUiStack.Push(uiContext);
        ShowUi(uiContext);
    }

    private bool CanClose(UiScreens screen)
    {
        if (screen == UiScreens.playerHud) return false;
        if (screen != UiScreens.menu) return true;
        return GameManager.Instance.GameState != GameManager.GameStates.MainMenu;
    }
    #endregion

    #region Show/Hide Different Ui Panels
    private void ShowUi(UiContext uiContext)
    {
        if (uiPanels.TryGetValue(uiContext.uiScreen, out var panel))
        {
            panel.ShowUi(uiContext);
            return;
        }

        Debug.LogError($"No UI registered for {uiContext.uiScreen}");
    }
    private void HideUi(UiContext uiContext)
    {
        if (uiPanels.TryGetValue(uiContext.uiScreen, out var panel))
        {
            panel.HideUi();
            return;
        }

        Debug.LogError($"No UI registered for {uiContext.uiScreen}");
    }
    #endregion

    #region Refresh Ui Input + Cursor States
    private void RefreshUiState()
    {
        InputBlock block = InputBlock.None;

        if (currentUiStack.Count > 0)
        {
            block = uiPanels[currentUiStack.Peek().uiScreen].GetInputBlock();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        OnUiScreenChange?.Invoke(block);
    }
    #endregion

    #region Toggle Player Hud Visibility
    public static void TogglePlayerHudVisibility(bool showHud, UiContext uiContext)
    {
        if (showHud) 
            Instance.PlayerHudPanel.ShowUi(uiContext);
        else
            Instance.PlayerHudPanel.HideUi();
    }
    #endregion

    #region Show Loading Transition
    public static void ShowSceneTransitionUi(bool open)
    {
        if (open)
            Instance.LoadingTransitionPanel.ShowUi(new(UiScreens.menu)); //uses interface but its overlay so context should never matter
        else
            Instance.LoadingTransitionPanel.HideUi();
    }
    #endregion

    #region Handle Player Ui Actions
    private void HandleBackAction()
    {
        if (!InputManager.Instance.GameMenuAction) return;

        switch (GameManager.Instance.GameState)
        {
            case GameManager.GameStates.Playing:
                GameManager.Instance.SetGameState(GameManager.GameStates.Paused);
                ToggleScreen(new(UiScreens.menu));
                break;

            case GameManager.GameStates.Paused:
                if (currentUiStack.Count > 0 && currentUiStack.Peek().uiScreen == UiScreens.menu)
                    GameManager.Instance.SetGameState(GameManager.GameStates.Playing);

                ToggleScreen(currentUiStack.Peek());
                break;

            case GameManager.GameStates.MainMenu:
                if (currentUiStack.Count > 0 && currentUiStack.Peek().uiScreen != UiScreens.menu)
                    ToggleScreen(currentUiStack.Peek());
                break;
        }
    }

    private void HandlePlayerInventoryAction()
    {
        if (!InputManager.Instance.PlayerInventoryAction) return;
        if (GameManager.Instance.GameState == GameManager.GameStates.Playing)
            ToggleScreen(new(UiScreens.playerInventory, GameManager.Instance.PlayerReference));
    }
    #endregion
}
