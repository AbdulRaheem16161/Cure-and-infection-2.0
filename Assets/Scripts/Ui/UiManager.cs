using System.Collections.Generic;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    public static UiManager Instance { get; private set; }

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
        Instance = this;
        CreateUiPanelDictionary();
    }

    private void Update()
    {
        HandleBackAction();
        HandlePlayerInventoryAction();
    }

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

    #region Reset Ui Screens Api
    public static void ResetUiScreens()
    {
        foreach (var kvp in Instance.uiPanels)
            kvp.Value.HideUi();
    }
    #endregion

    #region Show/Hide Ui Screens Api
    public static void ShowScreen(UiContext uiContext)
    {
        if (Instance.currentUiStack.Count > 0 && Instance.currentUiStack.Peek().uiScreen == uiContext.uiScreen)
            Instance.ShowPreviousUi(true);
        else
            Instance.PushAndShowUi(uiContext);
    }
    public static void HideTopScreen()
    {
        Instance.ShowPreviousUi(true);
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
                ShowScreen(new(UiScreens.menu));
                break;

            case GameManager.GameStates.Paused:
                if (currentUiStack.Count > 0 && currentUiStack.Peek().uiScreen == UiScreens.menu)
                    GameManager.Instance.SetGameState(GameManager.GameStates.Playing);

                ShowScreen(currentUiStack.Peek());
                break;

            case GameManager.GameStates.MainMenu:
                if (currentUiStack.Count > 0 && currentUiStack.Peek().uiScreen != UiScreens.menu)
                    ShowScreen(currentUiStack.Peek());
                break;
        }
    }

    private void HandlePlayerInventoryAction()
    {
        if (!InputManager.Instance.PlayerInventoryAction) return;
        if (GameManager.Instance.GameState == GameManager.GameStates.Playing)
            ShowScreen(new(UiScreens.playerInventory, GameManager.Instance.PlayerReference));
    }
    #endregion
}
