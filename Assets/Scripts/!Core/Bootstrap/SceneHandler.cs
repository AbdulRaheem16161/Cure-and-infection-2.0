using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{
    [SerializeField] private string bootstrapScene;
    [SerializeField] private string uiScene;
    [SerializeField] private string mainMenuScene;
    [SerializeField] private string gameScene;

    public string BootstrapScene => bootstrapScene;
    public string UiScene => uiScene;
    public string MainMenuScene => mainMenuScene;
    public string GameScene => gameScene;

    public Task LoadBootstrap() => LoadSceneAsync(bootstrapScene, LoadSceneMode.Single);
    public Task LoadUI() => LoadSceneAsync(uiScene);
    public Task LoadMainMenu() => LoadSceneAsync(mainMenuScene);
    public Task LoadGame() => LoadSceneAsync(gameScene);

    public static event Action<bool, string, float> OnSceneTransitionProgress;

    public async Task LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Additive)
    {
        if (sceneName == bootstrapScene && IsLoaded(bootstrapScene))
        {
            Debug.LogWarning($"Bootstrap scene is already loaded, cancelling");
            return;
        }
        else if (sceneName == uiScene && IsLoaded(uiScene))
        {
            Debug.LogWarning($"UI scene is already loaded, cancelling");
            return;
        }

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, mode);

        if (operation == null)
        {
            Debug.LogError($"Failed to load scene: {sceneName}");
            return;
        }

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            OnSceneTransitionProgress?.Invoke(true, sceneName, progress);
            await Task.Yield();
        }
    }

    public async Task UnloadSceneAsync(string sceneName)
    {
        if (!SceneManager.GetSceneByName(sceneName).isLoaded)
            return;

        AsyncOperation operation = SceneManager.UnloadSceneAsync(sceneName);

        if (operation == null)
        {
            Debug.LogError($"Failed to unload scene: {sceneName}");
            return;
        }

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            OnSceneTransitionProgress?.Invoke(false, sceneName, progress);
            await Task.Yield();
        }
    }

    public bool IsLoaded(string sceneName)
    {
        return SceneManager.GetSceneByName(sceneName).isLoaded;
    }

    public Scene GetScene(string sceneName)
    {
        return SceneManager.GetSceneByName(sceneName);
    }

    public Scene GetActiveScene()
    {
        return SceneManager.GetActiveScene();
    }

    public void SetActiveScene(Scene scene)
    {
        if (scene.name == BootstrapScene || scene.name == UiScene) return;
        SceneManager.SetActiveScene(scene);
    }
}
