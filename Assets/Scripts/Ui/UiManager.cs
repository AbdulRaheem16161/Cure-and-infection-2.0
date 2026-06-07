using System.Collections.Generic;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    /// <summary>
    /// UI RULES:
    /// only one ui panel open at a time, opening a new panel closes the previous one (loading transitions and player hud are always shown)
    /// if request to open a ui panel thats already open, close it (toggle)
    /// if request to open a ui panel that is not currently open, open it and close the previous one
    /// </summary>
    /// 
    public static UiManager Instance { get; private set; }

    public LoadingTransitionUi LoadingTransitionUi;

    public Stack<UiScreens> currentUiStack = new();

    public enum UiScreens
    {
        menu,
        saveGame,
        loadGame,
        settings,
        audioSettings,
        keybindsSettings,
        graphicsSettings,


        inventory,
    }

    private void Awake()
    {
        Instance = this;
    }

    public static void ShowScreen(UiScreens screen)
    {
        if (Instance.currentUiStack.Count > 0 && Instance.currentUiStack.Peek() == screen)
        {
            Instance.PopAndHideUi();
        }
        else
        {
            Instance.PushAndShowUi(screen);
        }
    }

    private void PopAndHideUi()
    {
        if (currentUiStack.Count <= 0) return;

        var screen = currentUiStack.Peek();

        if (!CanClose(screen)) return;

        HideUi(screen);
        currentUiStack.Pop();
    }

    private void PushAndShowUi(UiScreens screen)
    {
        HideTopUi();
        currentUiStack.Push(screen);
        ShowUi(screen);
    }

    private void HideTopUi()
    {
        if (currentUiStack.Count <= 0) return;
        HideUi(currentUiStack.Peek());
    }

    private bool CanClose(UiScreens screen)
    {
        if (screen != UiScreens.menu) return true;
        return GameManager.Instance.GameState != GameManager.GameStates.MainMenu;
    }

    private void ShowUi(UiScreens screen)
    {
        switch (screen)
        {
            case UiScreens.menu:
                //show main menu ui
                break;
            case UiScreens.saveGame:
                //show save game ui
                break;
            case UiScreens.loadGame:
                //show load game ui
                break;
            case UiScreens.settings:
                //show settings ui
                break;
            case UiScreens.audioSettings:
                //show audio settings ui
                break;
            case UiScreens.keybindsSettings:
                //show keybinds settings ui
                break;
            case UiScreens.graphicsSettings:
                //show graphics settings ui
                break;
            case UiScreens.inventory:
                //show inventory ui
                break;
        }
    }
    private void HideUi(UiScreens screen)
    {
        switch (screen)
        {
            case UiScreens.menu:
                //Hide main menu ui
                break;
            case UiScreens.saveGame:
                //Hide save game ui
                break;
            case UiScreens.loadGame:
                //Hide load game ui
                break;
            case UiScreens.settings:
                //Hide settings ui
                break;
            case UiScreens.audioSettings:
                //Hide audio settings ui
                break;
            case UiScreens.keybindsSettings:
                //Hide keybinds settings ui
                break;
            case UiScreens.graphicsSettings:
                //Hide graphics settings ui
                break;
            case UiScreens.inventory:
                //Hide inventory ui
                break;
        }
    }

    public static void ShowSceneTransitionUi(bool open)
    {
        if (open)
            Instance.LoadingTransitionUi.ShowUi();
        else
            Instance.LoadingTransitionUi.HideUi();
    }

    private void HandleBackInput()
    {
        if (!InputManager.Instance.GameMenuAction) return;

        switch (GameManager.Instance.GameState)
        {
            case GameManager.GameStates.Playing:
                GameManager.Instance.SetGameState(GameManager.GameStates.Paused);
                ShowScreen(UiScreens.menu);
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
}
