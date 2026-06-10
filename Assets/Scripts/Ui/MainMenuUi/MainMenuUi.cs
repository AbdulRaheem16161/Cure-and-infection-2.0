using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static GameManager;
using static UiManager;

public class MainMenuUi : MonoBehaviour, IUiPanel
{
    public GameObject mainMenuPanel;

    public Button quitGameButton;
    public Button closeMenuButton;

    public Button newGameButton;
    public Button saveGameButton;
    public Button loadGameButton;
    public Button settingsButton;
    public Button quitToMainMenuButton;

    #region Initialize Ui + Button Listeners
    private void Start()
    {
        InitializeUi();
    }

    private void InitializeUi()
    {
        quitGameButton.onClick.AddListener(QuitGame);
        closeMenuButton.onClick.AddListener(() => ShowScreen(new(UiScreens.menu)));
        newGameButton.onClick.AddListener(async () => await GameManager.Instance.StartGame());
        saveGameButton.onClick.AddListener(() => ShowScreen(new(UiScreens.saveGame)));
        loadGameButton.onClick.AddListener(() => ShowScreen(new(UiScreens.loadGame)));
        settingsButton.onClick.AddListener(() => ShowScreen(new(UiScreens.settings)));
        quitToMainMenuButton.onClick.AddListener(async () => await GameManager.Instance.LoadMainMenu());
    }

    private void OnDestroy()
    {
        quitGameButton.onClick.RemoveAllListeners();
        closeMenuButton.onClick.RemoveAllListeners();
        newGameButton.onClick.RemoveAllListeners();
        saveGameButton.onClick.RemoveAllListeners();
        loadGameButton.onClick.RemoveAllListeners();
        settingsButton.onClick.RemoveAllListeners();
        quitToMainMenuButton.onClick.RemoveAllListeners();
    }
    #endregion

    #region Show/Hide Ui Api
    public void ShowUi(UiContext uiContext)
    {
        GameStates gameState = GameManager.Instance.GameState;

        if (gameState == GameStates.MainMenu)
        {
            SetButtonVisibilityStates(
                quitGame: true,
                closeMenu: false,
                newGame: true,
                saveGame: false,
                loadGame: true,
                settings: true,
                quitToMenu: false);
        }
        else if (gameState == GameStates.Playing || gameState == GameStates.Paused)
        {
            SetButtonVisibilityStates(
                quitGame: false,
                closeMenu: true,
                newGame: false,
                saveGame: true,
                loadGame: true,
                settings: true,
                quitToMenu: true);
        }
        else
            Debug.LogError($"Tried showing menu buttons in incorrect GameState: {GameManager.Instance.GameState}");

        mainMenuPanel.SetActive(true);
    }
    public void HideUi()
    {
        mainMenuPanel.SetActive(false);
    }
    #endregion

    #region Handle Setting Button States
    private void SetButtonVisibilityStates(
        bool quitGame, bool closeMenu, bool newGame, bool saveGame, bool loadGame, bool settings, bool quitToMenu)
    {
        quitGameButton.gameObject.SetActive(quitGame);
        closeMenuButton.gameObject.SetActive(closeMenu);
        newGameButton.gameObject.SetActive(newGame);
        saveGameButton.gameObject.SetActive(saveGame);
        loadGameButton.gameObject.SetActive(loadGame);
        settingsButton.gameObject.SetActive(settings);
        quitToMainMenuButton.gameObject.SetActive(quitToMenu);
    }
    #endregion

    private void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
        return;
#else
        Application.Quit();
#endif
    }
}
