using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            UiManager.ShowSceneTransitionUi(true);
            await SceneHandler.LoadMainMenu();
            SetGameState(GameStates.MainMenu);
        }
        else
        {
            // Editor play-mode restoration
            SetGameState(sceneToRestore == SceneHandler.MainMenuScene ? GameStates.MainMenu : GameStates.Playing);
        }
    }
    #endregion

    #region Scene Events
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene {scene.name} loaded successfully");

        if (scene.name != SceneHandler.BootstrapScene && scene.name != SceneHandler.UiScene)
            SceneHandler.SetActiveScene(scene.name);
    }
    private void OnSceneUnloaded(Scene scene)
    {
        Debug.Log($"Scene {scene.name} unloaded successfully");
    }
    #endregion

    #region Game State And Scene Loading Management
    public async Task StartGame()
    {
        UiManager.ShowSceneTransitionUi(true);

        await SceneHandler.LoadGame();
        await Task.Yield();

        if (SceneHandler.IsLoaded(SceneHandler.MainMenuScene))
            await SceneHandler.UnloadSceneAsync(SceneHandler.MainMenuScene);

        SetGameState(GameStates.Playing);
        UiManager.ShowSceneTransitionUi(false);
    }
    public async Task QuitToMainMenu()
    {
        UiManager.ShowSceneTransitionUi(true);

        await SceneHandler.LoadMainMenu();
        await Task.Yield();

        if (SceneHandler.IsLoaded(SceneHandler.GameScene))
            await SceneHandler.UnloadSceneAsync(SceneHandler.GameScene);

        SetGameState(GameStates.MainMenu);
        UiManager.ShowSceneTransitionUi(false);
    }

    public void SetGameState(GameStates newState)
    {
        gameState = newState;

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
