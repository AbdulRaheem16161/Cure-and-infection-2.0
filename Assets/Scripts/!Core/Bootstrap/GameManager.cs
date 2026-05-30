using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SceneHandler))]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public SceneHandler SceneHandler { get; private set; }
    public UiManager UiManager { get; private set; }

    public GameStates GameState { get; private set; }

    public enum GameStates
    {
        Initializing,
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    private void Awake()
    {
        SceneHandler = GetComponent<SceneHandler>();
        Instance = this;
    }

    private async void Start()
    {
        await InitializeGame();
    }

    private async Task InitializeGame()
    {
        SetGameState(GameStates.Initializing);
        await SceneHandler.LoadUI();
        await SceneHandler.LoadMainMenu();
        SetGameState(GameStates.MainMenu);
    }

    public async Task StartGame()
    {
        await SceneHandler.LoadGame();
        SetGameState(GameStates.Playing);
    }
    public async Task QuitToMainMenu()
    {
        await SceneHandler.LoadMainMenu();
        SetGameState(GameStates.MainMenu);
    }

    public void SetGameState(GameStates newState)
    {
        GameState = newState;

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
}
