using System.Collections.Generic;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    public static UiManager Instance { get; private set; }

    public MainMenuUi MainMenuPanel;
    public SettingsUi SettingsPanel;
    public LoadingTransitionUi LoadingTransitionPanel;

    public PlayerInventoryUi PlayerInventoryPanel;
    public LootablesInventoryUi LootablesInventoryPanel;

    public Stack<UiScreens> currentUiStack = new();

    private Dictionary<UiScreens, IUiPanel> uiPanels;

    public enum UiScreens
    {
        menu,
        saveGame,
        loadGame,
        settings,
        audioSettings,
        keybindsSettings,
        graphicsSettings,

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

    #region Show Ui Screens Api
    public static void ShowScreen(UiContext uiContext)
    {
        if (Instance.currentUiStack.Count > 0 && Instance.currentUiStack.Peek() == uiContext.uiScreen)
            Instance.PopAndHideUi();
        else
            Instance.PushAndShowUi(uiContext);
    }
    #endregion

    #region Ui Panel Stacking Logic
    private void PushAndShowUi(UiContext uiContext)
    {
        HideTopUi();
        currentUiStack.Push(uiContext.uiScreen);
        ShowUi(uiContext);
    }

    private void HideTopUi()
    {
        if (currentUiStack.Count <= 0) return;
        HideUi(currentUiStack.Peek());
    }

    private void PopAndHideUi()
    {
        if (currentUiStack.Count <= 0) return;

        var screen = currentUiStack.Peek();

        if (!CanClose(screen)) return;

        HideUi(screen);
        currentUiStack.Pop();
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
    private void HideUi(UiScreens screen)
    {
        if (uiPanels.TryGetValue(screen, out var panel))
        {
            panel.HideUi();
            return;
        }

        Debug.LogError($"No UI registered for {screen}");
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
                if (currentUiStack.Count > 0 && currentUiStack.Peek() == UiScreens.menu)
                    GameManager.Instance.SetGameState(GameManager.GameStates.Playing);

                PopAndHideUi();
                break;

            case GameManager.GameStates.MainMenu:
                if (currentUiStack.Count > 0 && currentUiStack.Peek() != UiScreens.menu)
                    PopAndHideUi();
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
