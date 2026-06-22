using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UiManager;

[RequireComponent(typeof(SceneHandler))]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public SceneHandler SceneHandler { get; private set; }

    [SerializeField] private GameStates gameState;
    public GameStates GameState => gameState;

    public enum GameStates
    {
        Initializing,
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    public PlayerController PlayerReference;

    public static event Action<GameStates> OnGameStateChange;

    #region Game Initialization
    private void Awake()
    {
        SceneHandler = GetComponent<SceneHandler>();
        Instance = this;
    }

    private async void Start()
    {
        await InitializeGame();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private async Task InitializeGame()
    {
        SetGameState(GameStates.Initializing);
        await SceneHandler.LoadUI();

        var sceneToRestore = SessionState.GetString(BootstrapPlayMode.Key, "");

        if (string.IsNullOrEmpty(sceneToRestore))
        {
            // Normal build startup
            ShowSceneTransitionUi(true);
            await LoadMainMenu();
        }
        else
        {
            // Editor play-mode restoration
            ShowSceneTransitionUi(true);

            await SceneHandler.LoadSceneAsync(sceneToRestore, LoadSceneMode.Additive);
            SetGameState(sceneToRestore == SceneHandler.MainMenuScene ? GameStates.MainMenu : GameStates.Playing);

            if (GameState == GameStates.Playing && PlayerReference == null)
            {
                PlayerReference = PlayerSpawner.SpawnPlayer(null);
                TogglePlayerHudVisibility(true, new(UiScreens.playerHud, PlayerReference));
            }

            ResetUiScreens();
            ShowSceneTransitionUi(false);
        }
    }
    #endregion

    #region Scene Events
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene {scene.name} loaded successfully");

        SceneHandler.SetActiveScene(scene);
    }
    private void OnSceneUnloaded(Scene scene)
    {
        Debug.Log($"Scene {scene.name} unloaded successfully");
    }
    #endregion

    #region Game State And Scene Loading Management
    public async Task StartGame()
    {
        ShowSceneTransitionUi(true);

        await SceneHandler.LoadGame();
        await Task.Yield();

        if (SceneHandler.IsLoaded(SceneHandler.MainMenuScene))
            await SceneHandler.UnloadSceneAsync(SceneHandler.MainMenuScene);

        SetGameState(GameStates.Playing);

        if (PlayerReference == null)
            PlayerReference = PlayerSpawner.SpawnPlayer(null);

        ResetUiScreens();
        TogglePlayerHudVisibility(true, new(UiScreens.playerHud, PlayerReference));
        ShowSceneTransitionUi(false);

        Debug.Log($"Start Game Finished");
    }
    public async Task LoadMainMenu()
    {
        ShowSceneTransitionUi(true);

        await SceneHandler.LoadMainMenu();
        await Task.Yield();

        if (SceneHandler.IsLoaded(SceneHandler.GameScene))
            await SceneHandler.UnloadSceneAsync(SceneHandler.GameScene);

        SetGameState(GameStates.MainMenu);

        ResetUiScreens();
        TogglePlayerHudVisibility(false, null);
        ShowScreen(new(UiScreens.menu));
        ShowSceneTransitionUi(false);
    }

    public void SetGameState(GameStates newState)
    {
        gameState = newState;
        OnGameStateChange?.Invoke(gameState);

        switch (newState)
        {
            case GameStates.Paused:
            Time.timeScale = 0f;
            break;
            default:
            Time.timeScale = 1f;
            break;
        }
    }
    #endregion
}
